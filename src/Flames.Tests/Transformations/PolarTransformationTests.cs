using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для трансформации Polar
/// </summary>
public class PolarTransformationTests
{
    /// <summary>
    /// Проверяет основное поведение Polar:
    ///  - результат НЕ равен исходной точке,
    ///  - координаты конечные (не NaN / не Infinity),
    ///  - X должен быть в диапазоне [-1, 1], т.к. это θ / π,
    ///  - Y должен быть >= -1 (т.к. формула: r - 1)
    /// </summary>
    [Fact]
    public void Transform_ShouldApplyPolar()
    {
        var transformation = new PolarTransformation();
        var point = new Point(1.0, 0.0);

        var result = transformation.Transform(point);

        result.Should().NotBe(point);

        double.IsFinite(result.X).Should().BeTrue();
        double.IsFinite(result.Y).Should().BeTrue();

        result.X.Should().BeInRange(-1.0, 1.0);     // θ / π
        result.Y.Should().BeGreaterThanOrEqualTo(-1.0); // r - 1 (r >= 0)
    }

    /// <summary>
    /// Особый случай: точка (0,0)
    /// r = 0 => θ не определён, но в реализации atan2(0,0) = 0,
    /// поэтому ожидаемый результат: (0 / π, 0 - 1) = (0, -1)
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnExpectedPolarValue()
    {
        var transformation = new PolarTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.X.Should().Be(0.0);
        result.Y.Should().Be(-1.0);
    }
}

