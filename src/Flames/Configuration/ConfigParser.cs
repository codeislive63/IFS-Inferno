using Flames.Configuration.Exceptions;
using Flames.Models;
using System.Text.Json;

namespace Flames.Configuration;

/// <summary>
/// Парсер конфигурации из JSON и CLI аргументов
/// </summary>
public static class ConfigParser
{
    private static readonly JsonSerializerOptions CachedJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Загружает конфигурацию из JSON файла
    /// </summary>
    public static FlameConfig LoadFromJson(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ConfigurationException(
                "Путь к конфигурационному файлу пустой", 
                new ArgumentException("filePath is null or whitespace", nameof(filePath))
            );
        }

        try
        {
            var json = File.ReadAllText(filePath);

            var config = JsonSerializer.Deserialize<FlameConfig>(
                json, 
                CachedJsonOptions
            );

            return config ?? throw new JsonException("Десериализация вернула null: некорректный json");
        }
        catch (Exception ex) when (ex is IOException 
                                      or UnauthorizedAccessException
                                      or JsonException
                                      or ArgumentException)
        {
            throw new ConfigurationException(
                $"Не удалось загрузить конфигурацию из файла '{filePath}'.",
                ex
            );
        }
    }

    /// <summary>
    /// Создает конфигурацию по умолчанию
    /// </summary>
    public static FlameConfig CreateDefault()
    {
        return new FlameConfig
        {
            Size = new Size(1920, 1080),
            IterationCount = 2500,
            OutputPath = "result.png",
            Threads = 1,
            Seed = 5,
            Functions =
            [
                new() { Name = "linear", Weight = 1.0 }
            ],
            AffineParams =
            [
                new AffineParameters(
                    a: 0.5,
                    b: 0.0,
                    c: 0.0,
                    d: 0.0,
                    e: 0.5,
                    f: 0.0
                )
            ],
            IsGammaCorrectionEnabled = false,
            GammaCorrection = 2.2,
            SymmetryLevel = 1
        };
    }

    /// <summary>
    /// Объединяет конфигурации с приоритетом: cli > json > default
    /// </summary>
    public static FlameConfig Merge(FlameConfig? json, FlameConfig? cli)
    {
        var baseConfig = json ?? CreateDefault();

        if (cli is null)
        {
            return baseConfig;
        }

        var size = cli.Size.Width > 0 && cli.Size.Height > 0
            ? cli.Size
            : baseConfig.Size;

        var iterationCount = cli.IterationCount > 0
            ? cli.IterationCount
            : baseConfig.IterationCount;

        var outputPath = !string.IsNullOrWhiteSpace(cli.OutputPath)
            ? cli.OutputPath
            : baseConfig.OutputPath;

        var threads = cli.Threads > 0
            ? cli.Threads
            : baseConfig.Threads;

        var seed = cli.Seed != 0
            ? cli.Seed
            : baseConfig.Seed;

        var functions = cli.Functions.Count > 0
            ? cli.Functions
            : baseConfig.Functions;

        var affineParams = cli.AffineParams.Count > 0
            ? cli.AffineParams
            : baseConfig.AffineParams;

        var isGammaCorrectionEnabled = cli.IsGammaCorrectionEnabled || baseConfig.IsGammaCorrectionEnabled;

        var gamma = cli.GammaCorrection > 0
            ? cli.GammaCorrection
            : baseConfig.GammaCorrection;

        var symmetryLevel = cli.SymmetryLevel > 0
            ? cli.SymmetryLevel
            : baseConfig.SymmetryLevel;

        return new FlameConfig
        {
            Size = size,
            IterationCount = iterationCount,
            OutputPath = outputPath,
            Threads = threads,
            Seed = seed,
            Functions = functions,
            AffineParams = affineParams,
            IsGammaCorrectionEnabled = isGammaCorrectionEnabled,
            GammaCorrection = gamma,
            SymmetryLevel = symmetryLevel
        };
    }
}
