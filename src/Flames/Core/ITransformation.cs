using Flames.Models;

namespace Flames.Core;

/// <summary>
/// Интерфейс для трансформационных функций
/// </summary>
public interface ITransformation
{
    /// <summary>
    /// Применяет трансформацию к точке
    /// </summary>
    Point Transform(Point point);
}
