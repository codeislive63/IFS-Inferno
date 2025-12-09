using Flames.Configuration;
using Flames.Models;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Configuration;

/// <summary>
/// Тесты для парсера конфигурации
/// </summary>
public class ConfigParserTests
{
    /// <summary>
    /// <see cref="ConfigParser.CreateDefault"/> 
    /// возвращает валидную конфигурацию с ожидаемыми значениями по умолчанию
    /// </summary>
    [Fact]
    public void CreateDefault_ShouldReturnValidConfig()
    {
        var config = ConfigParser.CreateDefault();

        config.Should().NotBeNull();

        config.Size.Width.Should().Be(1920);
        config.Size.Height.Should().Be(1080);

        config.IterationCount.Should().Be(2500);
        config.OutputPath.Should().Be("result.png");
        config.Threads.Should().Be(1);
        config.Seed.Should().Be(5);

        config.Functions.Should().NotBeEmpty();
        config.AffineParams.Should().NotBeEmpty();

        config.IsGammaCorrectionEnabled.Should().BeFalse();
        config.GammaCorrection.Should().Be(2.2);
        config.SymmetryLevel.Should().Be(1);
    }

    /// <summary>
    /// При наличии CLI-конфигурации
    /// значения из неё имеют приоритет над JSON-конфигурацией
    /// </summary>
    [Fact]
    public void Merge_WithCliConfig_ShouldPrioritizeCli()
    {
        var jsonConfig = new FlameConfig { IterationCount = 1000 };
        var cliConfig = new FlameConfig { IterationCount = 2000 };

        var result = ConfigParser.Merge(jsonConfig, cliConfig);

        result.IterationCount.Should().Be(2000);
    }

    /// <summary>
    /// При отсутствии CLI-конфигурации
    /// используется JSON-конфигурация
    /// </summary>
    [Fact]
    public void Merge_WithOnlyJsonConfig_ShouldUseJson()
    {
        var jsonConfig = new FlameConfig { IterationCount = 1000 };
        FlameConfig? cliConfig = null;

        var result = ConfigParser.Merge(jsonConfig, cliConfig);

        result.IterationCount.Should().Be(1000);
    }

    /// <summary>
    /// При отсутствии JSON и CLI-конфигурации
    /// используется конфигурация по умолчанию
    /// </summary>
    [Fact]
    public void Merge_WithOnlyDefault_ShouldUseDefault()
    {
        var result = ConfigParser.Merge(null, null);

        result.IterationCount.Should().Be(2500);
    }

    /// <summary>
    /// При объединении конфигураций значения из CLI имеют приоритет
    /// над JSON-конфигурацией для всех ключевых полей
    /// </summary>
    [Fact]
    public void Merge_ShouldRespectPriorityForAllMainFields()
    {
        var jsonConfig = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 1000,
            OutputPath = "json.png",
            Threads = 2,
            Seed = 42,
            Functions = [],
            AffineParams = [],
            IsGammaCorrectionEnabled = false,
            GammaCorrection = 1.8,
            SymmetryLevel = 2
        };

        var cliConfig = new FlameConfig
        {
            Size = new Size(200, 300),
            IterationCount = 2000,
            OutputPath = "cli.png",
            Threads = 4,
            Seed = 777,
            Functions = [],
            AffineParams = [],
            IsGammaCorrectionEnabled = true,
            GammaCorrection = 2.5,
            SymmetryLevel = 3
        };

        var result = ConfigParser.Merge(jsonConfig, cliConfig);

        result.Size.Width.Should().Be(200);
        result.Size.Height.Should().Be(300);
        result.IterationCount.Should().Be(2000);
        result.OutputPath.Should().Be("cli.png");
        result.Threads.Should().Be(4);
        result.Seed.Should().Be(777);

        result.IsGammaCorrectionEnabled.Should().BeTrue();
        result.GammaCorrection.Should().Be(2.5);
        result.SymmetryLevel.Should().Be(3);
    }
}
