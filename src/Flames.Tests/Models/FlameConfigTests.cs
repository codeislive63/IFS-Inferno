using Flames.Models;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Models;

/// <summary>
/// Тесты для конфигурации пламени
/// </summary>
public class FlameConfigTests
{
    /// <summary>
    /// Дефолтный конструктор должен выставлять ожидаемые значения по умолчанию
    /// </summary>
    [Fact]
    public void DefaultValues_ShouldBeSet()
    {
        var config = new FlameConfig();

        config.Size.Width.Should().Be(1920);
        config.Size.Height.Should().Be(1080);
        config.IterationCount.Should().Be(2500);
        config.OutputPath.Should().Be("result.png");
        config.Threads.Should().Be(1);
        config.Seed.Should().Be(5);
        config.IsGammaCorrectionEnabled.Should().BeFalse();
        config.GammaCorrection.Should().Be(2.2);
        config.SymmetryLevel.Should().Be(1);
    }

    /// <summary>
    /// Параметризованный конструктор должен корректно устанавливать все значения
    /// </summary>
    [Fact]
    public void Ctor_WithAllArguments_ShouldSetProperties()
    {
        var size = new Size(800, 600);

        var functions = new List<FunctionConfig>
        {
            new("swirl", 1.0),
            new("linear", 0.5)
        };

        var affineParams = new List<AffineParameters>
        {
            new(0.5, 0, 0, 0, 0.5, 0),
            new(0.3, 0.1, -0.2, 0.4, 0.7, 0.2)
        };

        var config = new FlameConfig(
            size: size,
            iterationCount: 1234,
            outputPath: "custom.png",
            threads: 4,
            seed: 99,
            functions: functions,
            affineParams: affineParams,
            gammaCorrection: true,
            gamma: 2.4,
            symmetryLevel: 5
        );

        config.Size.Should().Be(size);
        config.IterationCount.Should().Be(1234);
        config.OutputPath.Should().Be("custom.png");
        config.Threads.Should().Be(4);
        config.Seed.Should().Be(99);
        config.Functions.Should().BeSameAs(functions);
        config.AffineParams.Should().BeSameAs(affineParams);
        config.IsGammaCorrectionEnabled.Should().BeTrue();
        config.GammaCorrection.Should().Be(2.4);
        config.SymmetryLevel.Should().Be(5);
    }

    /// <summary>
    /// Параметризованный конструктор должен выбрасывать исключение при null пути вывода
    /// </summary>
    [Fact]
    public void Ctor_WithNullOutputPath_ShouldThrow()
    {
        var size = new Size(100, 100);
        
        var functions = new List<FunctionConfig> 
        { 
            new("linear", 1.0) 
        };

        var affineParams = new List<AffineParameters> 
        { 
            new(0.5, 0, 0, 0, 0.5, 0) 
        };

        var act = () => new FlameConfig(
            size,
            iterationCount: 10,
            outputPath: null!,
            threads: 1,
            seed: 1,
            functions: functions,
            affineParams: affineParams,
            gammaCorrection: false,
            gamma: 2.2,
            symmetryLevel: 1
        );

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("outputPath");
    }

    /// <summary>
    /// Параметризованный конструктор должен выбрасывать исключение при null списке функций
    /// </summary>
    [Fact]
    public void Ctor_WithNullFunctions_ShouldThrow()
    {
        var size = new Size(100, 100);

        var affineParams = new List<AffineParameters> 
        { 
            new(0.5, 0, 0, 0, 0.5, 0) 
        };

        var act = () => new FlameConfig(
            size,
            iterationCount: 10,
            outputPath: "out.png",
            threads: 1,
            seed: 1,
            functions: null!,
            affineParams: affineParams,
            gammaCorrection: false,
            gamma: 2.2,
            symmetryLevel: 1
        );

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("functions");
    }

    /// <summary>
    /// Параметризованный конструктор должен выбрасывать исключение при null списке аффинных параметров
    /// </summary>
    [Fact]
    public void Ctor_WithNullAffineParams_ShouldThrow()
    {
        var size = new Size(100, 100);

        var functions = new List<FunctionConfig> 
        { 
            new("linear", 1.0) 
        };

        var act = () => new FlameConfig(
            size,
            iterationCount: 10,
            outputPath: "out.png",
            threads: 1,
            seed: 1,
            functions: functions,
            affineParams: null!,
            gammaCorrection: false,
            gamma: 2.2,
            symmetryLevel: 1
        );

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("affineParams");
    }
}
