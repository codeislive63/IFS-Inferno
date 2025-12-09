using Flames.Core;
using Flames.Models;

namespace Flames.Transformations;

/// <summary>
/// Трансформация Spherical
/// </summary>
public sealed class SphericalTransformation : ITransformation
{
    /// <inheritdoc/>
    public Point Transform(Point point)
    {
        double x = point.X;
        double y = point.Y;
        double r2 = x * x + y * y;

        if (r2 == 0.0)
        {
            return point;
        }

        double inv = 1.0 / r2;

        return new Point(
            x * inv,
            y * inv
        );
    }
}
