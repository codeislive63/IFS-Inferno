using Flames.Models;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Models;

/// <summary>
/// Тесты для конфигурации функции
/// </summary>
public class FunctionConfigTests
{
    /// <summary>
    /// Проверяет, что конструктор по умолчанию инициализирует объект
    /// корректными значениями
    [Fact]
    public void DefaultValues_ShouldBeSet()
    {
        var config = new FunctionConfig();

        config.Name.Should().BeEmpty();
        config.Weight.Should().Be(1.0);
    }

    /// <summary>
    /// Параметризованный конструктор должен корректно устанавливать имя и вес
    /// </summary>
    [Fact]
    public void Ctor_WithArguments_ShouldSetProperties()
    {
        var config = new FunctionConfig("swirl", 0.75);

        config.Name.Should().Be("swirl");
        config.Weight.Should().Be(0.75);
    }
}
