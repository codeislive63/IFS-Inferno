using Flames.Core;
using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Core;

/// <summary>
/// Тесты для FlameFunction
/// </summary>
public class FlameFunctionTests
{
    /// <summary>
    /// Линейная трансформация должна вернуть точку после применения
    /// только аффинного преобразования
    /// </summary>
    [Fact]
    public void Apply_ShouldApplyAffineAndLinearTransformation()
    {
        var affine = new AffineParameters(
            a: 2.0, b: 0.0, c: 0.0,
            d: 0.0, e: 2.0, f: 0.0
        );

        var transformation = new LinearTransformation();
        var function = new FlameFunction(affine, transformation, weight: 1.0);

        var point = new Point(1.0, 1.0);

        var result = function.Apply(point);

        result.X.Should().Be(2.0);
        result.Y.Should().Be(2.0);
    }

    /// <summary>
    /// Swirl-трансформация должна корректно применяться поверх аффинной
    /// Проверяем, что результат НЕ равен входной точке и что вычислен корректный тип
    /// </summary>
    [Fact]
    public void Apply_WithSwirl_ShouldTransformPoint()
    {
        var affine = new AffineParameters(
            a: 1.0, b: 0.0, c: 0.0,
            d: 0.0, e: 1.0, f: 0.0
        );

        var transformation = new SwirlTransformation();
        var function = new FlameFunction(affine, transformation, weight: 1.0);

        var point = new Point(1.0, 0.0);

        var result = function.Apply(point);

        result.Should().NotBe(point);
        result.X.Should().NotBe(0);
        result.Y.Should().NotBe(0);
    }

    /// <summary>
    /// Проверяем, что вес функции сохраняется и доступен для renderer
    /// </summary>
    [Fact]
    public void Weight_ShouldBeStoredCorrectly()
    {
        var affine = new AffineParameters();
        var function = new FlameFunction(affine, new LinearTransformation(), weight: 3.14);

        function.Weight.Should().Be(3.14);
    }

    /// <summary>
    /// Транформация сферическая должна уменьшать координаты
    /// </summary>
    [Fact]
    public void Apply_SphericalTransformation_ShouldShrinkPoint()
    {
        var affine = new AffineParameters(
            a: 1.0, b: 0.0, c: 0.0,
            d: 0.0, e: 1.0, f: 0.0
        );

        var transformation = new SphericalTransformation();
        var function = new FlameFunction(affine, transformation, 1.0);

        var point = new Point(2.0, 0.0);

        var result = function.Apply(point);

        result.X.Should().BeApproximately(0.5, 1e-6);
        result.Y.Should().BeApproximately(0.0, 1e-6);
    }
}
