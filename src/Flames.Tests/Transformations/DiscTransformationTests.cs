using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для трансформации Disc
/// </summary>
public class DiscTransformationTests
{
    /// <summary>
    /// Проверяет, что трансформация Disc возвращает корректное новое положение точки
    /// </summary>
    [Fact]
    public void Transform_ShouldApplyDisc()
    {
        var transformation = new DiscTransformation();
        var point = new Point(1.0, 0.0);

        var result = transformation.Transform(point);

        result.Should().NotBe(point);
        result.X.Should().BeInRange(-1, 1);
        result.Y.Should().BeInRange(-1, 1);
    }

    /// <summary>
    /// Disc имеет особый случай: если входная точка (0,0),
    /// её нельзя нормализовать, поэтому трансформация обязана
    /// вернуть (0,0), иначе произойдёт деление на ноль
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnZeroPoint()
    {
        var transformation = new DiscTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.X.Should().Be(0.0);
        result.Y.Should().Be(0.0);
    }
}
