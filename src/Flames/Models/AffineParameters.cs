namespace Flames.Models;

/// <summary>
/// Параметры аффинного преобразования
/// </summary>
public readonly record struct AffineParameters
{
    public double A { get; init; }

    public double B { get; init; }

    public double C { get; init; }

    public double D { get; init; }

    public double E { get; init; }

    public double F { get; init; }

    /// <summary>
    /// Создает параметры аффинного преобразования
    /// </summary>
    public AffineParameters(
        double a,
        double b,
        double c,
        double d,
        double e,
        double f)
    {
        A = a;
        B = b;
        C = c;
        D = d;
        E = e;
        F = f;
    }

    /// <summary>
    /// Применяет аффинное преобразование к координатам точки
    /// </summary>
    public (double x, double y) Transform(double x, double y)
    {
        var newX = A * x + B * y + C;
        var newY = D * x + E * y + F;

        return (newX, newY);
    }
}
