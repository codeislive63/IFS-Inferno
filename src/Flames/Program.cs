using System.Diagnostics;
using Flames.Configuration;
using Flames.Configuration.Exceptions;
using Flames.Core;
using Flames.Models;
using Flames.Transformations;
using Microsoft.Extensions.Logging;

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
        logger.LogInformation("Конфигурация загружена из файла: {ConfigPath}", configPath);
    }

    var config = ConfigParser.Merge(jsonConfig, cliConfig);

    var errors = ConfigValidator.Validate(config);
    
    if (errors.Count > 0)
    {
        logger.LogError("Ошибки валидации конфигурации:");
        
        foreach (var error in errors)
        {
            logger.LogError(" - {Error}", error);
        }

        Environment.Exit(1);
    }

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

    logger.LogInformation("Генерация завершена за {Seconds:F3} секунд", stopwatch.Elapsed.TotalSeconds);
}
catch (ConfigurationException ex)
{
    logger.LogError(ex, "Ошибка конфигурации");
    Environment.Exit(1);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Критическая ошибка");
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

        var transformation = TransformationFactory.Create(funcConfig.Name) 
            ?? throw new ConfigurationException(
                $"Неизвестная функция трансформации '{funcConfig.Name}' (index={i}).",
                new InvalidOperationException($"Unknown transformation: '{funcConfig.Name}'.")
            );

        var affine = i < config.AffineParams.Count
            ? config.AffineParams[i]
            : new AffineParameters(
                a: 0.5, b: 0.0, c: 0.0,
                d: 0.0, e: 0.5, f: 0.0
            );

        flameFunctions.Add(new FlameFunction(affine, transformation, funcConfig.Weight));
    }

    return flameFunctions;
}
