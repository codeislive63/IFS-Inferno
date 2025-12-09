using Flames.Configuration;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Configuration;

/// <summary>
/// Тесты для парсера командной строки
/// </summary>
public class CommandLineParserTests
{
    /// <summary>
    /// Базовые числовые параметры парсятся корректно
    /// </summary>
    [Fact]
    public void Parse_WithValidArguments_ShouldReturnConfig()
    {
        var args = new[] { "-w", "800", "-h", "600", "-i", "1000" };
        var (config, _, exitCode) = CommandLineParser.Parse(args);

        exitCode.Should().Be(0);
        config.Should().NotBeNull();
        config!.Size.Width.Should().Be(800);
        config.Size.Height.Should().Be(600);
        config.IterationCount.Should().Be(1000);
    }

    /// <summary>
    /// Проверяет, что строка конфигурации функций
    /// корректно парсится в список <c>Functions</c>
    /// </summary>
    [Fact]
    public void Parse_WithFunctions_ShouldParseFunctions()
    {
        var args = new[] { "-f", "swirl:1.0,horseshoe:0.8" };
        var (config, _, exitCode) = CommandLineParser.Parse(args);

        exitCode.Should().Be(0);
        config.Should().NotBeNull();
        config!.Functions.Should().HaveCount(2);
        config.Functions[0].Name.Should().Be("swirl");
        config.Functions[0].Weight.Should().Be(1.0);
        config.Functions[1].Name.Should().Be("horseshoe");
        config.Functions[1].Weight.Should().Be(0.8);
    }

    /// <summary>
    /// Проверяет, что строка аффинных параметров
    /// корректно парсится в список <c>AffineParams</c>
    /// </summary>
    [Fact]
    public void Parse_WithAffineParams_ShouldParseAffineParams()
    {
        var args = new[]
        {
            "-ap",
            "1.0,0.0,0.0,0.0,1.0,0.0/0.5,0.0,0.5,0.0,0.5,0.5"
        };

        var (config, _, exitCode) = CommandLineParser.Parse(args);

        exitCode.Should().Be(0);
        config.Should().NotBeNull();

        config!.AffineParams.Should().HaveCount(2);

        config.AffineParams[0].A.Should().Be(1.0);
        config.AffineParams[0].E.Should().Be(1.0);
        config.AffineParams[0].C.Should().Be(0.0);

        config.AffineParams[1].A.Should().Be(0.5);
        config.AffineParams[1].C.Should().Be(0.5);
        config.AffineParams[1].F.Should().Be(0.5);
    }
}
