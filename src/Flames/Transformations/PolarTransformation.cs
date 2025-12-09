using Flames.Core;
using Flames.Models;

namespace Flames.Transformations;

/// <summary>
/// Трансформация Polar
/// </summary>
public sealed class PolarTransformation : ITransformation
{
    /// <inheritdoc/>
    public Point Transform(Point point)
    {
        double x = point.X;
        double y = point.Y;
        double r = Math.Sqrt(x * x + y * y);
        double theta = Math.Atan2(y, x);

        return new Point(
            theta / Math.PI,
            r - 1.0
        );
    }
}
