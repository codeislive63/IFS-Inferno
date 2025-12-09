using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для трансформации Spherical
/// </summary>
public class SphericalTransformationTests
{
    /// <summary>
    /// Проверяет корректность сферической трансформации:
    ///  - результат должен отличаться от исходной точки;
    ///  - координаты должны быть конечными (не NaN / Infinity);
    ///  - формула должна работать точно: (x/r^2, y/r^2)
    /// </summary>
    [Fact]
    public void Transform_ShouldApplySphericalCorrectly()
    {
        var transformation = new SphericalTransformation();
        var point = new Point(2.0, 0.0);

        var result = transformation.Transform(point);

        result.X.Should().BeApproximately(0.5, 1e-6);
        result.Y.Should().BeApproximately(0.0, 1e-6);

        double.IsFinite(result.X).Should().BeTrue();
        double.IsFinite(result.Y).Should().BeTrue();
    }

    /// <summary>
    /// Проверяет особый случай r^2 = 0
    /// По реализации — возвращаем исходную точку, чтобы избежать деления на ноль
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnZeroPoint()
    {
        var transformation = new SphericalTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.X.Should().Be(0.0);
        result.Y.Should().Be(0.0);
    }
}
