using Flames.Core;
using Flames.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Flames.Tests.Core;

/// <summary>
/// Тесты для рендерера фрактального пламени FlameRenderer
/// </summary>
public class FlameRendererTests
{
    /// <summary>
    /// Логгер-заглушка для использования в тестах
    /// </summary>
    private static readonly ILogger<FlameRenderer> Logger = NullLogger<FlameRenderer>.Instance;

    /// <summary>
    /// Простейшая тестовая трансформация, немного смещающая и сжимающая точку
    /// </summary>
    private sealed class ShiftTransformation : ITransformation
    {
        public Point Transform(Point p) => new(p.X * 0.8 + 0.1, p.Y * 0.8 - 0.05);
    }

    /// <summary>
    /// Создает минимальную конфигурацию рендера для тестов
    /// </summary>
    private static FlameConfig CreateConfig(
        string outputPath,
        int threads = 1,
        int iterations = 500,
        bool gammaCorrection = false)
    {
        return new FlameConfig
        {
            Size = new Size(64, 64),
            IterationCount = iterations,
            Threads = threads,
            SymmetryLevel = 3,
            OutputPath = outputPath,
            Seed = 42,
            IsGammaCorrectionEnabled = gammaCorrection,
            GammaCorrection = 2.2
        };
    }

    /// <summary>
    /// Создает одну тестовую функцию пламени с аффинным преобразованием и ShiftTransformation
    /// </summary>
    private static List<FlameFunction> CreateTestFunctions()
    {
        var affine = new AffineParameters(
            a: 1.0,
            b: 0.0,
            c: 0.0,
            d: 1.0,
            e: 0.0,
            f: 0.0
        );

        return
        [
            new FlameFunction(
                affine,
                new ShiftTransformation(),
                weight: 1.0
            )
        ];
    }

    /// <summary>
    /// Возвращает путь к временному PNG файлу
    /// </summary>
    private static string GetTempPng() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");

    /// <summary>
    /// В однопоточном режиме должен создаваться непустой PNG файл
    /// </summary>
    [Fact]
    public void Render_SingleThreaded_CreatesNonEmptyPng()
    {
        var output = GetTempPng();
        var config = CreateConfig(output, threads: 1, iterations: 1000);
        var functions = CreateTestFunctions();
        var renderer = new FlameRenderer(config, functions, Logger);

        renderer.Render();

        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length > 0);
    }

    /// <summary>
    /// В многопоточном режиме должен создаваться непустой PNG файл
    /// </summary>
    [Fact]
    public void Render_MultiThreaded_CreatesNonEmptyPng()
    {
        var output = GetTempPng();
        var config = CreateConfig(output, threads: 4, iterations: 4000);
        var functions = CreateTestFunctions();
        var renderer = new FlameRenderer(config, functions, Logger);

        renderer.Render();

        Assert.True(File.Exists(output));
        Assert.True(new FileInfo(output).Length > 0);
    }

    /// <summary>
    /// При нулевом количестве итераций рендер не должен падать и должен создать файл
    /// </summary>
    [Fact]
    public void Render_WithZeroIterations_DoesNotThrow_AndCreatesImage()
    {
        var output = GetTempPng();
        var config = CreateConfig(output, threads: 1, iterations: 0);
        var functions = CreateTestFunctions();
        var renderer = new FlameRenderer(config, functions, Logger);

        renderer.Render();

        Assert.True(File.Exists(output));
    }

    /// <summary>
    /// Включенная гамма-коррекция должна приводить к отличающемуся результату изображения
    /// </summary>
    [Fact]
    public void Render_WithGammaCorrection_ProducesDifferentImage()
    {
        var outputNoGamma = GetTempPng();
        var outputGamma = GetTempPng();
        var functions = CreateTestFunctions();

        var configNoGamma = CreateConfig(
            outputNoGamma,
            threads: 1,
            iterations: 2000,
            gammaCorrection: false);

        var configGamma = CreateConfig(
            outputGamma,
            threads: 1,
            iterations: 2000,
            gammaCorrection: true);

        var r1 = new FlameRenderer(configNoGamma, functions, Logger);
        var r2 = new FlameRenderer(configGamma, functions, Logger);

        r1.Render();
        r2.Render();

        Assert.True(File.Exists(outputNoGamma));
        Assert.True(File.Exists(outputGamma));

        var bytes1 = File.ReadAllBytes(outputNoGamma);
        var bytes2 = File.ReadAllBytes(outputGamma);

        Assert.NotEqual(bytes1, bytes2);
    }
}
