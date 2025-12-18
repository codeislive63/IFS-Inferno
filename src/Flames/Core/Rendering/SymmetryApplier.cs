using Flames.Models;

namespace Flames.Core.Rendering;

/// <summary>
/// Применяет заданный уровень симметрии: вращает точку на несколько углов и добавляет вклад
/// в плотность и цветовые каналы соответствующих пикселей
/// </summary>
public sealed class SymmetryApplier
{
    private readonly int _width;
    private readonly int _height;
    private readonly (double Cos, double Sin)[] _rotations;

    /// <summary>
    /// Подготавливает параметры симметрии для заданного размера изображения и уровня симметрии,
    /// вычисляя набор углов поворота (cos/sin)
    /// </summary>
    public SymmetryApplier(int width, int height, int symmetryLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(symmetryLevel);

        _width = width;
        _height = height;
        _rotations = new (double Cos, double Sin)[symmetryLevel];
        
        double step = 2.0 * Math.PI / symmetryLevel;

        for (int s = 0; s < symmetryLevel; s++)
        {
            double angle = s * step;
            _rotations[s] = (Math.Cos(angle), Math.Sin(angle));
        }
    }

    /// <summary>
    /// Добавляет вклад точки и её симметричных копий в массивы плотности и цвета
    /// </summary>
    public void Apply(
        Point point,
        int functionIndex,
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        if (point.X < -1 || point.X > 1 || point.Y < -1 || point.Y > 1)
        {
            return;
        }

        int pixelX = (int)((point.X + 1) * 0.5 * _width);
        int pixelY = (int)((1 - point.Y) * 0.5 * _height);

        if (pixelX < 0 || pixelX >= _width || pixelY < 0 || pixelY >= _height)
        {
            return;
        }

        double r = Math.Sin(functionIndex * 0.5) * 0.5 + 0.5;
        double g = Math.Sin(functionIndex * 0.5 + 2.0) * 0.5 + 0.5;
        double b = Math.Sin(functionIndex * 0.5 + 4.0) * 0.5 + 0.5;

        for (int s = 0; s < _rotations.Length; s++)
        {
            var (cosA, sinA) = _rotations[s];

            double symX = point.X * cosA - point.Y * sinA;
            double symY = point.X * sinA + point.Y * cosA;

            if (symX is < -1 or > 1 || symY is < -1 or > 1)
            {
                continue;
            }

            int symPixelX = (int)((symX + 1) * 0.5 * _width);
            int symPixelY = (int)((1 - symY) * 0.5 * _height);

            if (symPixelX < 0 || symPixelX >= _width || symPixelY < 0 || symPixelY >= _height)
            {
                continue;
            }

            density[symPixelX, symPixelY] += 1.0;
            colorR[symPixelX, symPixelY] += r;
            colorG[symPixelX, symPixelY] += g;
            colorB[symPixelX, symPixelY] += b;
        }
    }
}
