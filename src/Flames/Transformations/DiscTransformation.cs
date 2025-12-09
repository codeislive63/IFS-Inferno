using Flames.Core;
using Flames.Models;

namespace Flames.Transformations;

/// <summary>
/// Трансформация Disc
/// </summary>
public sealed class DiscTransformation : ITransformation
{
    private const double OneOverPi = 1.0 / Math.PI;

    /// <inheritdoc/>
    public Point Transform(Point point)
    {
        var x = point.X;
        var y = point.Y;

        var r = Math.Sqrt(x * x + y * y);
        
        if (r == 0)
        {
            return point;
        }

        var theta = Math.Atan2(y, x);
        var angle = Math.PI * r;
        var sin = Math.Sin(angle);
        var cos = Math.Cos(angle);
        var factor = theta * OneOverPi;

        return new Point(
            factor * sin,
            factor * cos
        );
    }
}
