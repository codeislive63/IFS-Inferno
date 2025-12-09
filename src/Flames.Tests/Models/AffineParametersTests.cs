using Flames.Models;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Models;

/// <summary>
/// Тесты для аффинных параметров
/// </summary>
public class AffineParametersTests
{

    /// <summary>
    /// Проверяет, что преобразование корректно применяет сдвиг,
    /// когда матрица — единичная, а C и F задают смещение координат
    /// </summary>
    [Fact]
    public void Transform_ShouldApplyAffineTransformation()
    {
        var affine = new AffineParameters(
            a: 1.0, b: 0.0, c: 1.0,
            d: 0.0, e: 1.0, f: 1.0
        );

        var (x, y) = affine.Transform(0.0, 0.0);

        x.Should().Be(1.0);
        y.Should().Be(1.0);
    }

    /// <summary>
    /// Проверяет, что аффинное преобразование корректно масштабирует координаты,
    /// когда матрица задаёт коэффициенты масштабирования
    /// </summary>
    [Fact]
    public void Transform_WithScale_ShouldScalePoint()
    {
        var affine = new AffineParameters(
            a: 2.0, b: 0.0, c: 0.0,
            d: 0.0, e: 2.0, f: 0.0
        );

        var (x, y) = affine.Transform(1.0, 1.0);

        x.Should().Be(2.0);
        y.Should().Be(2.0);
    }
}
