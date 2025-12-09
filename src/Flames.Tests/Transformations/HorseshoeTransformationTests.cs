using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для трансформации Horseshoe
/// </summary>
public class HorseshoeTransformationTests
{
    /// <summary>
    /// Проверяет, что трансформация Horseshoe корректно трансформирует точку
    /// </summary>
    [Fact]
    public void Transform_ShouldApplyHorseshoe()
    {
        var transformation = new HorseshoeTransformation();
        var point = new Point(1.0, 1.0);

        var result = transformation.Transform(point);

        result.Should().NotBe(point);

        double.IsFinite(result.X).Should().BeTrue();
        double.IsFinite(result.Y).Should().BeTrue();

        result.X.Should().BeInRange(-2, 2);
        result.Y.Should().BeInRange(-2, 2);
    }

    /// <summary>
    /// Horseshoe имеет особый случай:
    /// при r = 0 невозможно делить на r, поэтому функция обязана вернуть исходную точку
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnZeroPoint()
    {
        var transformation = new HorseshoeTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.X.Should().Be(0.0);
        result.Y.Should().Be(0.0);
    }
}
