using Flames.Core;

namespace Flames.Transformations;

/// <summary>
/// Фабрика для создания трансформаций
/// </summary>
public static class TransformationFactory
{
    private static readonly IReadOnlyDictionary<string, Func<ITransformation>> Transformations =
        new Dictionary<string, Func<ITransformation>>(StringComparer.OrdinalIgnoreCase)
        {
            { "linear", () => new LinearTransformation() },
            { "swirl", () => new SwirlTransformation() },
            { "horseshoe", () => new HorseshoeTransformation() },
            { "disc", () => new DiscTransformation() },
            { "polar", () => new PolarTransformation() },
            { "spherical", () => new SphericalTransformation() }
        };

    /// <summary>
    /// Создает трансформацию по имени
    /// </summary>
    public static ITransformation? Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        name = name.Trim();

        return Transformations.TryGetValue(name, out var factory)
            ? factory()
            : null;
    }

    /// <summary>
    /// Возвращает список доступных трансформаций
    /// </summary>
    public static IEnumerable<string> GetAvailableTransformations()
    {
        return Transformations.Keys;
    }
}
