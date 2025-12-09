using System.Diagnostics;
using Flames.Configuration;
using Flames.Core;
using Flames.Models;
using Flames.Transformations;
using Microsoft.Extensions.Logging;


try
{
    var commandLineArgs = Environment
        .GetCommandLineArgs()
        .Skip(1)
        .ToArray();

    var (cliConfig, configPath, parseExitCode) = CommandLineParser.Parse(commandLineArgs);

    if (parseExitCode != 0)
    {
        Environment.Exit(parseExitCode);
    }

    FlameConfig? jsonConfig = null;

    if (!string.IsNullOrWhiteSpace(configPath))
    {
        jsonConfig = ConfigParser.LoadFromJson(configPath);

        if (jsonConfig is null)
        {
            Console.Error.WriteLine($"Ошибка: не удалось загрузить конфигурацию из файла '{configPath}'");
            Environment.Exit(1);
        }
    }

    var config = ConfigParser.Merge(jsonConfig, cliConfig);
    var errors = ConfigValidator.Validate(config);

    if (errors.Count > 0)
    {
        Console.Error.WriteLine("Ошибки валидации конфигурации:");
        
        foreach (var error in errors)
        {
            Console.Error.WriteLine($"  - {error}");
        }

        Environment.Exit(1);
    }

    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .SetMinimumLevel(LogLevel.Information)
            .AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
    });

    var logger = loggerFactory.CreateLogger("Flames.App");

    logger.LogInformation(
        "Инициализация генератора фрактального пламени, конфигурация: {Width}x{Height}, Iter={Iterations}, Threads={Threads}",
        config.Size.Width,
        config.Size.Height,
        config.IterationCount,
        config.Threads
    );

    var flameFunctions = BuildFlameFunctions(config, logger);

    var rendererLogger = loggerFactory.CreateLogger<FlameRenderer>();
    var renderer = new FlameRenderer(config, flameFunctions, rendererLogger);

    var stopwatch = Stopwatch.StartNew();
    renderer.Render();
    stopwatch.Stop();

    logger.LogInformation(
        "Генерация завершена за {Seconds:F3} секунд",
        stopwatch.Elapsed.TotalSeconds
    );
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Критическая ошибка: {ex.Message}");
    Console.Error.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}

/// <summary>
/// Строит набор функций пламени на основе конфигурации
/// </summary>
static List<FlameFunction> BuildFlameFunctions(FlameConfig config, ILogger logger)
{
    var flameFunctions = new List<FlameFunction>(config.Functions.Count);

    for (int i = 0; i < config.Functions.Count; i++)
    {
        var funcConfig = config.Functions[i];

        var transformation = TransformationFactory.Create(funcConfig.Name);
        if (transformation is null)
        {
            logger.LogError("Неизвестная функция трансформации: {FunctionName}", funcConfig.Name);
            Environment.Exit(1);
        }

        var affine = i < config.AffineParams.Count
            ? config.AffineParams[i]
            : new AffineParameters(
                a: 0.5,
                b: 0.0,
                c: 0.0,
                d: 0.0,
                e: 0.5,
                f: 0.0);

        flameFunctions.Add(new FlameFunction(affine, transformation, funcConfig.Weight));
    }

    return flameFunctions;
}
