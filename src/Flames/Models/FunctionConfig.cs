namespace Flames.Models;

/// <summary>
/// Конфигурация трансформационной функции
/// </summary>
public sealed record class FunctionConfig
{
    /// <summary>
    /// Имя функции трансформации
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Вес функции при выборе трансформации
    /// </summary>
    public double Weight { get; init; } = 1.0;

    public FunctionConfig() { }

    public FunctionConfig(string name, double weight)
    {
        Name = name;
        Weight = weight;
    }
}
