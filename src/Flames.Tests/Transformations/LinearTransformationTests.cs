using Flames.Models;
using Flames.Transformations;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Transformations;

/// <summary>
/// Тесты для линейной трансформации
/// </summary>
public class LinearTransformationTests
{
    /// <summary>
    /// Линейная трансформация обязана возвращать ту же точку без изменений
    /// </summary>
    [Fact]
    public void Transform_ShouldReturnSamePoint()
    {
        var transformation = new LinearTransformation();
        var point = new Point(1.0, 2.0);

        var result = transformation.Transform(point);

        result.Should().Be(point);
    }

    /// <summary>
    /// Нулевая точка должна оставаться нулевой
    /// </summary>
    [Fact]
    public void Transform_WithZeroPoint_ShouldReturnZeroPoint()
    {
        var transformation = new LinearTransformation();
        var point = new Point(0.0, 0.0);

        var result = transformation.Transform(point);

        result.Should().Be(point);
    }
}
