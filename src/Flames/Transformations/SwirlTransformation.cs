using Flames.Core;
using Flames.Models;

namespace Flames.Transformations;

/// <summary>
/// Трансформация Swirl
/// </summary>
public sealed class SwirlTransformation : ITransformation
{
    /// <inheritdoc/>
    public Point Transform(Point point)
    {
        double x = point.X;
        double y = point.Y;
        double r2 = x * x + y * y;
        double sinR2 = Math.Sin(r2);
        double cosR2 = Math.Cos(r2);

        return new Point(
            x * sinR2 - y * cosR2,
            x * cosR2 + y * sinR2
        );
    }
}
