using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для фабрики трансформаций
/// </summary>
public class TransformationFactoryTests
{
    /// <summary>
    /// Проверяет, что фабрика корректно создаёт все зарегистрированные трансформации,
    /// когда подаётся валидное имя
    /// </summary>
    [Theory]
    [InlineData("linear")]
    [InlineData("swirl")]
    [InlineData("horseshoe")]
    [InlineData("disc")]
    [InlineData("polar")]
    [InlineData("spherical")]
    public void Create_WithValidName_ShouldReturnTransformation(string name)
    {
        var transformation = TransformationFactory.Create(name);

        transformation.Should().NotBeNull($"фабрика должна уметь создавать '{name}'");
    }

    /// <summary>
    /// Проверяет, что при отсутствии трансформации с указанным именем
    /// фабрика возвращает null, а не бросает исключение
    /// </summary>
    [Fact]
    public void Create_WithInvalidName_ShouldReturnNull()
    {
        var transformation = TransformationFactory.Create("invalid");

        transformation.Should().BeNull();
    }

    /// <summary>
    /// Проверяет, что фабрика нечувствительна к регистру
    /// и корректно создаёт трансформации независимо от формата имени
    /// </summary>
    [Fact]
    public void Create_WithCaseInsensitiveName_ShouldReturnTransformation()
    {
        var transformation = TransformationFactory.Create("SWIRL");

        transformation.Should().NotBeNull();
    }

    /// <summary>
    /// Проверяет, что список доступных трансформаций содержит
    /// все ключи, зарегистрированные в словаре фабрики
    /// </summary>
    [Fact]
    public void GetAvailableTransformations_ShouldReturnAllTransformations()
    {
        var transformations = TransformationFactory.GetAvailableTransformations().ToList();

        transformations.Should().Contain("linear");
        transformations.Should().Contain("swirl");
        transformations.Should().Contain("horseshoe");
        transformations.Should().Contain("disc");
        transformations.Should().Contain("polar");
        transformations.Should().Contain("spherical");
    }

    /// <summary>
    /// Проверяет, что фабрика создаёт новый экземпляр трансформации при каждом вызове,
    /// а не возвращает один и тот же объект повторно
    /// </summary>
    [Fact]
    public void Create_ShouldReturnNewInstanceEachTime()
    {
        // Act
        var first = TransformationFactory.Create("swirl");
        var second = TransformationFactory.Create("swirl");

        // Assert
        first.Should().NotBeSameAs(second);
    }
}
