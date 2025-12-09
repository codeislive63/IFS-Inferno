using Flames.Models;
using Flames.Transformations;

namespace Flames.Core;

/// <summary>
/// Функция пламени, объединяющая аффинное преобразование, вариацию и вес
/// </summary>
/// <remarks>
/// Создает функцию пламени из аффинного преобразования, вариации и веса
/// </remarks>
public sealed class FlameFunction(AffineParameters affine, ITransformation transformation, double weight)
{
    /// <summary>
    /// Аффинное преобразование, применяемое перед вариацией
    /// </summary>
    public AffineParameters Affine { get; } = affine;

    /// <summary>
    /// Нелинейная трансформация
    /// </summary>
    public ITransformation Transformation { get; } = transformation ?? throw new ArgumentNullException(nameof(transformation));

    /// <summary>
    /// Вес функции при выборе трансформации
    /// </summary>
    public double Weight { get; } = weight;

    /// <summary>
    /// Применяет функцию к точке
    /// </summary>
    public Point Apply(in Point point)
    {
        var (x, y) = Affine.Transform(point.X, point.Y);
        return Transformation.Transform(new Point(x, y));
    }
}
