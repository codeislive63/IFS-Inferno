using System.CommandLine;
using System.Globalization;
using Flames.Models;

namespace Flames.Configuration;

/// <summary>
/// Парсер командной строки
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Парсит аргументы командной строки
    /// </summary>
    public static (FlameConfig? cliConfig, string? configPath, int exitCode) Parse(string[] args)
    {
        // ===== ОПЦИИ =====
        var widthOption = new Option<int>(
            aliases: ["-w", "--width"],
            description: "Ширина изображения");

        var heightOption = new Option<int>(
            aliases: ["-h", "--height"],
            description: "Высота изображения");

        var seedOption = new Option<long>(
            aliases: ["--seed"],
            description: "Начальное значение генератора");

        var iterationCountOption = new Option<int>(
            aliases: ["-i", "--iteration-count"],
            description: "Количество итераций");

        var outputPathOption = new Option<string?>(
            aliases: ["-o", "--output-path"],
            description: "Путь для сохранения изображения");

        var threadsOption = new Option<int>(
            aliases: ["-t", "--threads"],
            description: "Количество потоков");

        var affineParamsOption = new Option<string?>(
            aliases: ["-ap", "--affine-params"],
            description: "Аффинные параметры в формате a1,b1,c1,d1,e1,f1/a2,b2,c2,d2,e2,f2");

        var functionsOption = new Option<string?>(
            aliases: ["-f", "--functions"],
            description: "Функции трансформации в формате name1:weight1,name2:weight2");

        var configOption = new Option<string?>(
            aliases: ["--config"],
            description: "Путь к JSON файлу конфигурации");

        var gammaCorrectionOption = new Option<bool>(
            aliases: ["-g", "--gamma-correction"],
            description: "Включить гамма-коррекцию");

        var gammaOption = new Option<string?>(
            aliases: ["--gamma"],
            description: "Значение гаммы для коррекции");

        var symmetryLevelOption = new Option<int>(
            aliases: ["-s", "--symmetry-level"],
            description: "Уровень симметрии");

        // ===== КОМАНДА =====
        var rootCommand = new RootCommand("Генератор фрактального пламени");
        rootCommand.AddOption(widthOption);
        rootCommand.AddOption(heightOption);
        rootCommand.AddOption(seedOption);
        rootCommand.AddOption(iterationCountOption);
        rootCommand.AddOption(outputPathOption);
        rootCommand.AddOption(threadsOption);
        rootCommand.AddOption(affineParamsOption);
        rootCommand.AddOption(functionsOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(gammaCorrectionOption);
        rootCommand.AddOption(gammaOption);
        rootCommand.AddOption(symmetryLevelOption);

        FlameConfig? cliConfig = null;
        string? configPath = null;

        // ===== HANDLER =====
        rootCommand.SetHandler(context =>
        {
            var pr = context.ParseResult;

            configPath = pr.GetValueForOption(configOption);

            var width = pr.GetValueForOption(widthOption);
            var height = pr.GetValueForOption(heightOption);
            var seed = pr.GetValueForOption(seedOption);
            var iterationCount = pr.GetValueForOption(iterationCountOption);
            var outputPath = pr.GetValueForOption(outputPathOption);
            var threads = pr.GetValueForOption(threadsOption);
            var affineParamsRaw = pr.GetValueForOption(affineParamsOption);
            var functionsRaw = pr.GetValueForOption(functionsOption);
            var isGammaCorrectionEnabled = pr.GetValueForOption(gammaCorrectionOption);
            var gammaStr = pr.GetValueForOption(gammaOption);
            var symmetryLevel = pr.GetValueForOption(symmetryLevelOption);

            var gamma = ParseDoubleOrDefault(gammaStr, 2.2);

            var functions = string.IsNullOrWhiteSpace(functionsRaw)
                ? []
                : ParseFunctions(functionsRaw);

            var affineParams = string.IsNullOrWhiteSpace(affineParamsRaw)
                ? []
                : ParseAffineParams(affineParamsRaw);

            cliConfig = new FlameConfig
            {
                Size = new Size(width, height),
                Seed = seed,
                IterationCount = iterationCount,
                OutputPath = outputPath ?? string.Empty,
                Threads = threads,
                IsGammaCorrectionEnabled = isGammaCorrectionEnabled,
                GammaCorrection = gamma,
                SymmetryLevel = symmetryLevel,
                Functions = functions,
                AffineParams = affineParams
            };
        });

        int exitCode;

        try
        {
            exitCode = rootCommand.Invoke(args);
        }
        catch
        {
            exitCode = 1;
        }

        return (cliConfig, configPath, exitCode);
    }

    /// <summary>
    /// Парсит строку с функциями трансформации
    /// </summary>
    private static List<FunctionConfig> ParseFunctions(string functionsStr)
    {
        var functions = new List<FunctionConfig>();
        var parts = functionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var colonIndex = part.IndexOf(':');
            
            if (colonIndex <= 0 || colonIndex >= part.Length - 1)
            {
                continue;
            }

            var name = part[..colonIndex].Trim();
            var weightStr = part[(colonIndex + 1)..].Trim();

            var weight = ParseDoubleOrDefault(weightStr, defaultValue: 1.0);
            functions.Add(new FunctionConfig { Name = name, Weight = weight });
        }

        return functions;
    }

    /// <summary>
    /// Парсит строку с параметрами аффинных преобразований
    /// </summary>
    private static List<AffineParameters> ParseAffineParams(string affineParamsStr)
    {
        var paramsList = new List<AffineParameters>();
        var parts = affineParamsStr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var values = part.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            if (values.Length != 6)
            {
                continue;
            }

            var doubles = values
                .Select(v => ParseDoubleOrDefault(v, double.NaN))
                .ToArray();

            if (doubles.Any(double.IsNaN))
            {
                continue;
            }

            var a = doubles[0];
            var b = doubles[1];
            var c = doubles[2];
            var d = doubles[3];
            var e = doubles[4];
            var f = doubles[5];

            paramsList.Add(new AffineParameters(a, b, c, d, e, f));
        }

        return paramsList;
    }

    /// <summary>
    /// Парсит число с плавающей точкой с учетом инвариантной культуры
    /// </summary>
    private static double ParseDoubleOrDefault(string? value, double defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        var normalized = value.Replace(',', '.');

        return double.TryParse(
            normalized,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out var result)
            ? result
            : defaultValue;
    }
}
