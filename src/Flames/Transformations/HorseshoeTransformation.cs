using Flames.Core;
using Flames.Models;

namespace Flames.Transformations;

/// <summary>
/// Трансформация Horseshoe
/// </summary>
public sealed class HorseshoeTransformation : ITransformation
{
    /// <inheritdoc/>
    public Point Transform(Point point)
    {
        double x = point.X;
        double y = point.Y;
        double r = Math.Sqrt(x * x + y * y);
        
        if (r == 0.0)
        {
            return new Point(0, 0);
        }

        double invR = 1.0 / r;
        double xPlusY = x + y;
        double xMinusY = x - y;

        double newX = invR * xMinusY * xPlusY;
        double newY = 2.0 * invR * x * y;

        return new Point(newX, newY);
    }
}
