using Flames.Models;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Flames.Core;

/// <summary>
/// Рендерер фрактального пламени
/// </summary>
public sealed class FlameRenderer
{
    private const int SkipIterations = 20;

    private readonly ILogger<FlameRenderer> _logger;
    private readonly FlameConfig _config;
    private readonly IReadOnlyList<FlameFunction> _functions;
    private readonly double[] _cumulativeWeights;
    private readonly double _totalWeight;

    private int Width => _config.Size.Width;
    private int Height => _config.Size.Height;

    /// <summary>
    /// Создает рендерер фрактального пламени
    /// </summary>
    public FlameRenderer(
        FlameConfig config,
        IReadOnlyList<FlameFunction> functions,
        ILogger<FlameRenderer> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _functions = functions ?? throw new ArgumentNullException(nameof(functions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_functions.Count == 0)
        {
            throw new ArgumentException("Должна быть указана хотя бы одна функция пламени", nameof(functions));
        }

        _cumulativeWeights = new double[_functions.Count];

        double totalWeight = 0;

        for (int i = 0; i < _functions.Count; i++)
        {
            var weight = Math.Max(0.0, _functions[i].Weight);
            totalWeight += weight;
            _cumulativeWeights[i] = totalWeight;
        }

        if (totalWeight <= 0)
        {
            throw new ArgumentException("Суммарный вес функций должен быть положительным", nameof(functions));
        }

        _totalWeight = totalWeight;
    }

    /// <summary>
    /// Генерирует изображение фрактального пламени
    /// </summary>
    public void Render()
    {
        _logger.LogInformation(
            "Начало генерации фрактального пламени. Размер: {Width}x{Height}, Итераций: {Iterations}, Потоков: {Threads}",
            Width,
            Height,
            _config.IterationCount,
            _config.Threads
        );

        var density = new double[Width, Height];
        var colorR = new double[Width, Height];
        var colorG = new double[Width, Height];
        var colorB = new double[Width, Height];

        var random = new Random(unchecked((int)_config.Seed));

        if (_config.Threads <= 1)
        {
            RenderSingleThreaded(density, colorR, colorG, colorB, random);
        }
        else
        {
            RenderMultiThreaded(density, colorR, colorG, colorB);
        }

        _logger.LogInformation("Генерация точек завершена. Применение log-density и создание изображения...");

        ApplyLogDensityAndNormalize(density, colorR, colorG, colorB);
        SaveImage(density, colorR, colorG, colorB);

        _logger.LogInformation("Изображение сохранено: {OutputPath}", _config.OutputPath);
    }

    /// <summary>
    /// Однопоточная генерация точек
    /// </summary>
    private void RenderSingleThreaded(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB,
        Random random)
    {
        var point = new Models.Point(
            random.NextDouble() * 2 - 1,
            random.NextDouble() * 2 - 1
        );

        int totalIterations = _config.IterationCount + SkipIterations;
        int lastLoggedPercent = -1;

        for (int i = 0; i < totalIterations; i++)
        {
            if (i % 1000 == 0 && i > 0)
            {
                int percent = (int)((double)i / totalIterations * 100);

                if (percent != lastLoggedPercent)
                {
                    _logger.LogInformation(
                        "Прогресс: {Percent}% ({Current}/{Total} итераций)",
                        percent,
                        i,
                        totalIterations
                    );

                    lastLoggedPercent = percent;
                }
            }

            int functionIndex = SelectFunction(random);
            point = _functions[functionIndex].Apply(point);

            if (i >= SkipIterations)
            {
                ApplySymmetry(point, functionIndex, density, colorR, colorG, colorB);
            }
        }
    }

    /// <summary>
    /// Многопоточная генерация точек
    /// </summary>
    private void RenderMultiThreaded(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        var localDensity = new double[_config.Threads][,];
        var localColorR = new double[_config.Threads][,];
        var localColorG = new double[_config.Threads][,];
        var localColorB = new double[_config.Threads][,];

        for (int t = 0; t < _config.Threads; t++)
        {
            localDensity[t] = new double[Width, Height];
            localColorR[t] = new double[Width, Height];
            localColorG[t] = new double[Width, Height];
            localColorB[t] = new double[Width, Height];
        }

        int iterationsPerThread = _config.IterationCount / _config.Threads;
        var tasks = new List<Task>(_config.Threads);

        for (int t = 0; t < _config.Threads; t++)
        {
            int threadId = t;
            int startIteration = threadId * iterationsPerThread;
            int endIteration = threadId == _config.Threads - 1
                ? _config.IterationCount
                : (threadId + 1) * iterationsPerThread;

            tasks.Add(Task.Run(() =>
            {
                var threadRandom = new Random(unchecked((int)(_config.Seed + threadId)));

                var point = new Models.Point(
                    threadRandom.NextDouble() * 2 - 1,
                    threadRandom.NextDouble() * 2 - 1
                );

                int iterations = (endIteration - startIteration) + SkipIterations;

                for (int i = 0; i < iterations; i++)
                {
                    int functionIndex = SelectFunction(threadRandom);
                    point = _functions[functionIndex].Apply(point);

                    if (i >= SkipIterations)
                    {
                        ApplySymmetry(
                            point,
                            functionIndex,
                            localDensity[threadId],
                            localColorR[threadId],
                            localColorG[threadId],
                            localColorB[threadId]
                        );
                    }
                }
            }));
        }

        Task.WaitAll([.. tasks]);

        for (int t = 0; t < _config.Threads; t++)
        {
            var d = localDensity[t];
            var r = localColorR[t];
            var g = localColorG[t];
            var b = localColorB[t];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    density[x, y] += d[x, y];
                    colorR[x, y] += r[x, y];
                    colorG[x, y] += g[x, y];
                    colorB[x, y] += b[x, y];
                }
            }
        }
    }

    /// <summary>
    /// Выбирает функцию согласно весам
    /// </summary>
    private int SelectFunction(Random random)
    {
        double value = random.NextDouble() * _totalWeight;

        for (int i = 0; i < _cumulativeWeights.Length; i++)
        {
            if (value <= _cumulativeWeights[i])
            {
                return i;
            }
        }

        return _cumulativeWeights.Length - 1;
    }

    /// <summary>
    /// Применяет симметрию к точке и обновляет буферы плотности и цвета
    /// </summary>
    private void ApplySymmetry(
        Models.Point point,
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

        int pixelX = (int)((point.X + 1) * 0.5 * Width);
        int pixelY = (int)((1 - point.Y) * 0.5 * Height);

        if (pixelX < 0 || pixelX >= Width || pixelY < 0 || pixelY >= Height)
        {
            return;
        }

        double r = Math.Sin(functionIndex * 0.5) * 0.5 + 0.5;
        double g = Math.Sin(functionIndex * 0.5 + 2.0) * 0.5 + 0.5;
        double b = Math.Sin(functionIndex * 0.5 + 4.0) * 0.5 + 0.5;

        double angleStep = 2.0 * Math.PI / _config.SymmetryLevel;

        for (int s = 0; s < _config.SymmetryLevel; s++)
        {
            double angle = s * angleStep;
            double cosA = Math.Cos(angle);
            double sinA = Math.Sin(angle);

            double symX = point.X * cosA - point.Y * sinA;
            double symY = point.X * sinA + point.Y * cosA;

            if (symX is < -1 or > 1 || symY is < -1 or > 1)
            {
                continue;
            }

            int symPixelX = (int)((symX + 1) * 0.5 * Width);
            int symPixelY = (int)((1 - symY) * 0.5 * Height);

            if (symPixelX < 0 || symPixelX >= Width || symPixelY < 0 || symPixelY >= Height)
            {
                continue;
            }

            density[symPixelX, symPixelY] += 1.0;
            colorR[symPixelX, symPixelY] += r;
            colorG[symPixelX, symPixelY] += g;
            colorB[symPixelX, symPixelY] += b;
        }
    }

    /// <summary>
    /// Применяет log-density и нормализует цвета
    /// </summary>
    private void ApplyLogDensityAndNormalize(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        double maxDensity = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
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

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (density[x, y] > 0)
                {
                    colorR[x, y] /= density[x, y];
                    colorG[x, y] /= density[x, y];
                    colorB[x, y] /= density[x, y];
                }
            }
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (density[x, y] > 0)
                {
                    double logDensity = Math.Log(density[x, y] + 1) / logMaxDensity;
                    density[x, y] = logDensity;
                }
            }
        }
    }

    /// <summary>
    /// Создает изображение и сохраняет его в PNG
    /// </summary>
    private void SaveImage(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        using var image = new Image<Rgb24>(Width, Height);

        double maxDensity = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (density[x, y] > maxDensity)
                {
                    maxDensity = density[x, y];
                }
            }
        }

        double maxR = 0, maxG = 0, maxB = 0;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (density[x, y] > 0)
                {
                    maxR = Math.Max(maxR, colorR[x, y]);
                    maxG = Math.Max(maxG, colorG[x, y]);
                    maxB = Math.Max(maxB, colorB[x, y]);
                }
            }
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                double d = maxDensity > 0 ? density[x, y] / maxDensity : 0;

                double r = maxR > 0 ? (colorR[x, y] / maxR) * d : 0;
                double g = maxG > 0 ? (colorG[x, y] / maxG) * d : 0;
                double b = maxB > 0 ? (colorB[x, y] / maxB) * d : 0;

                if (_config.IsGammaCorrectionEnabled)
                {
                    double gammaInv = 1.0 / _config.GammaCorrection;

                    r = Math.Pow(Math.Clamp(r, 0.0, 1.0), gammaInv);
                    g = Math.Pow(Math.Clamp(g, 0.0, 1.0), gammaInv);
                    b = Math.Pow(Math.Clamp(b, 0.0, 1.0), gammaInv);
                }

                byte rByte = (byte)(Math.Clamp(r, 0.0, 1.0) * 255.0);
                byte gByte = (byte)(Math.Clamp(g, 0.0, 1.0) * 255.0);
                byte bByte = (byte)(Math.Clamp(b, 0.0, 1.0) * 255.0);

                image[x, y] = new Rgb24(rByte, gByte, bByte);
            }
        }

        image.SaveAsPng(_config.OutputPath);
    }
}
