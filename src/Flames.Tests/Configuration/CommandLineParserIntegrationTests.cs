using Flames.Configuration;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Configuration;

/// <summary>
/// Интеграционные тесты для парсера командной строки
/// </summary>
public class CommandLineParserIntegrationTests
{
    /// <summary>
    /// Парсер корректно обрабатывает полный набор CLI-опций
    /// и возвращает сформированную конфигурацию без ошибок
    /// </summary>
    [Fact]
    public void Parse_WithAllOptions_ShouldParseCorrectly()
    {
        var args = new[]
        {
            "-w", "800", "-h", "600", "-i", "1000",
            "-o", "output.png", "-t", "4", "--seed", "123",
            "-f", "swirl:1.0,horseshoe:0.8",
            "-ap", "1.0,0.0,0.0,0.0,1.0,0.0/0.5,0.0,0.5,0.0,0.5,0.5",
            "-g", "--gamma", "2.5", "-s", "4"
        };

        var (config, _, exitCode) = CommandLineParser.Parse(args);

        exitCode.Should().Be(0);
        config.Should().NotBeNull();
        config!.Size.Width.Should().Be(800);
        config.Size.Height.Should().Be(600);
        config.IterationCount.Should().Be(1000);
        config.OutputPath.Should().Be("output.png");
        config.Threads.Should().Be(4);
        config.Seed.Should().Be(123);
        config.IsGammaCorrectionEnabled.Should().BeTrue();
        config.GammaCorrection.Should().Be(2.5);
        config.SymmetryLevel.Should().Be(4);
    }

    /// <summary>
    /// При использовании аргумента <c>--config</c>
    /// парсер:
    /// - НЕ собирает CLI-конфигурацию,
    /// - возвращает путь к файлу конфигурации,
    /// - завершает выполнение без ошибок
    /// </summary>
    [Fact]
    public void Parse_WithConfigOption_ShouldReturnConfigPath()
    {
        var args = new[] { "--config", "test.json" };
        var (_, configPath, exitCode) = CommandLineParser.Parse(args);

        exitCode.Should().Be(0);
        configPath.Should().Be("test.json");
    }
}
