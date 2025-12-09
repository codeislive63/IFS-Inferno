using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для трансформации Swirl
/// </summary>
public class SwirlTransformationTests
{
    /// <summary>
    /// Проверяет, что Swirl корректно изменяет координаты точки:
    ///  - результат отличается от исходной точки,
    ///  - координаты остаются конечными (не NaN / Infinity),
    ///  - поведение согласовано с формулой:
    ///    x' = x * sin(r^2) - y * cos(r^2)
    ///    y' = x * cos(r^2) + y * sin(r^2)
    /// </summary>
    [Fact]
    public void Transform_ShouldApplySwirl()
    {
        var transformation = new SwirlTransformation();
        var point = new Point(1.0, 0.0);

        var result = transformation.Transform(point);

        var expectedX = Math.Sin(1.0);
        var expectedY = Math.Cos(1.0);

        result.X.Should().BeApproximately(expectedX, 1e-6);
        result.Y.Should().BeApproximately(expectedY, 1e-6);

        double.IsFinite(result.X).Should().BeTrue();
        double.IsFinite(result.Y).Should().BeTrue();
    }

    /// <summary>
    /// В нуле (0,0) радиус r^2 = 0, sin(0) = 0, cos(0) = 1,
    /// поэтому точка должна остаться на месте
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnZeroPoint()
    {
        var transformation = new SwirlTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.Should().Be(point);
    }
}
