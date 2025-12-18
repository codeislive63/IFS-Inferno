namespace Flames.Core.Rendering;

/// <summary>
/// Постобработка log-density и нормализация цвета
/// </summary>
public static class DensityPostProcessor
{
    /// <summary>
    /// Применяет log-density и нормализует цвета
    /// </summary>
    public static void ApplyLogDensityAndNormalize(
        int width,
        int height,
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        double maxDensity = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (density[x, y] > maxDensity)
                {
                    maxDensity = density[x, y];
                }
            }
        }

        if (maxDensity == 0)
        {
            return;
        }

        double logMaxDensity = Math.Log(maxDensity + 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (density[x, y] > 0)
                {
                    colorR[x, y] /= density[x, y];
                    colorG[x, y] /= density[x, y];
                    colorB[x, y] /= density[x, y];
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (density[x, y] > 0)
                {
                    density[x, y] = Math.Log(density[x, y] + 1) / logMaxDensity;
                }
            }
        }
    }
}
