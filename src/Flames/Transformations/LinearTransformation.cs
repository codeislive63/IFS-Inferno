using Flames.Core;
using Flames.Models;
using System.Runtime.CompilerServices;

namespace Flames.Transformations;

/// <summary>
/// Линейная трансформация
/// </summary>
public sealed class LinearTransformation : ITransformation
{
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point Transform(Point point)
    {
        return point;
    }
}
