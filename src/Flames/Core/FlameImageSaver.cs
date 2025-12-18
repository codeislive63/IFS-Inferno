using Flames.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Flames.Core;

public static class FlameImageSaver
{
    /// <summary>
    /// Создает изображение и сохраняет его в PNG
    /// </summary>
    public static void SaveAsPng(
        FlameConfig config,
        int width,
        int height,
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        using var image = new Image<Rgb24>(width, height);

        double maxR = 0, maxG = 0, maxB = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (density[x, y] <= 0)
                {
                    continue;
                }

                maxR = Math.Max(maxR, colorR[x, y]);
                maxG = Math.Max(maxG, colorG[x, y]);
                maxB = Math.Max(maxB, colorB[x, y]);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                double d = Math.Clamp(density[x, y], 0.0, 1.0);

                double r = maxR > 0 ? (colorR[x, y] / maxR) * d : 0;
                double g = maxG > 0 ? (colorG[x, y] / maxG) * d : 0;
                double b = maxB > 0 ? (colorB[x, y] / maxB) * d : 0;

                if (config.IsGammaCorrectionEnabled)
                {
                    double gammaInv = 1.0 / config.GammaCorrection;
                    r = Math.Pow(Math.Clamp(r, 0.0, 1.0), gammaInv);
                    g = Math.Pow(Math.Clamp(g, 0.0, 1.0), gammaInv);
                    b = Math.Pow(Math.Clamp(b, 0.0, 1.0), gammaInv);
                }

                image[x, y] = new Rgb24(
                    (byte)(Math.Clamp(r, 0.0, 1.0) * 255.0),
                    (byte)(Math.Clamp(g, 0.0, 1.0) * 255.0),
                    (byte)(Math.Clamp(b, 0.0, 1.0) * 255.0)
                );
            }
        }

        image.SaveAsPng(config.OutputPath);
    }
}
