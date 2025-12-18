using Flames.Models;
using Microsoft.Extensions.Logging;

namespace Flames.Core.Rendering;

/// <summary>
/// Генерирует точки фрактального пламени, выбирая функции преобразования по весам
/// </summary>
public sealed class FlamePointGenerator(
    FlameConfig config,
    IReadOnlyList<FlameFunction> functions,
    WeightedFunctionSelector selector,
    SymmetryApplier symmetry,
    ILogger logger)
{
    private const int SkipIterations = 20;

    private readonly IReadOnlyList<FlameFunction> _functions = functions ?? throw new ArgumentNullException(nameof(functions));
    private readonly FlameConfig _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly WeightedFunctionSelector _selector = selector ?? throw new ArgumentNullException(nameof(selector));
    private readonly SymmetryApplier _symmetry = symmetry ?? throw new ArgumentNullException(nameof(symmetry));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Запускает генерацию фрактального пламени и заполняет переданные буферы плотности и цветовых каналов.
    /// В зависимости от значения <see cref="_config.Threads"/> выбирает однопоточный или многопоточный режим.
    /// </summary>
    public void Generate(
        double[,] density, 
        double[,] colorR, 
        double[,] colorG, 
        double[,] colorB)
    {
        if (_config.Threads <= 1)
        {
            RenderSingleThreaded(density, colorR, colorG, colorB);
        }
        else
        {
            RenderMultiThreaded(density, colorR, colorG, colorB);
        }
    }

    /// <summary>
    /// Выполняет генерацию точек в одном потоке и записывает накопления в общие буферы
    /// </summary>
    private void RenderSingleThreaded(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        var random = new Random(unchecked((int)_config.Seed));
        var point = new Point(random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1);

        int totalIterations = _config.IterationCount + SkipIterations;
        int lastLoggedPercent = -1;

        for (int i = 0; i < totalIterations; i++)
        {
            if (i % 1000 == 0 && i > 0)
            {
                int percent = (int)((double)i / totalIterations * 100);
                if (percent != lastLoggedPercent)
                {
                    _logger.LogInformation("Прогресс: {Percent}% ({Current}/{Total} итераций)", percent, i, totalIterations);
                    lastLoggedPercent = percent;
                }
            }

            int functionIndex = _selector.Select(random);
            point = _functions[functionIndex].Apply(point);

            if (i >= SkipIterations)
            {
                _symmetry.Apply(point, functionIndex, density, colorR, colorG, colorB);
            }
        }
    }

    /// <summary>
    /// Выполняет генерацию точек в нескольких потоках, накапливая данные в локальные буферы,
    /// а затем суммирует их в итоговые массивы
    /// </summary>
    private void RenderMultiThreaded(
        double[,] density,
        double[,] colorR,
        double[,] colorG,
        double[,] colorB)
    {
        int threads = _config.Threads;

        var localDensity = new double[threads][,];
        var localColorR = new double[threads][,];
        var localColorG = new double[threads][,];
        var localColorB = new double[threads][,];

        int width = density.GetLength(0);
        int height = density.GetLength(1);

        for (int t = 0; t < threads; t++)
        {
            localDensity[t] = new double[width, height];
            localColorR[t] = new double[width, height];
            localColorG[t] = new double[width, height];
            localColorB[t] = new double[width, height];
        }

        int iterationsPerThread = _config.IterationCount / threads;
        var tasks = new List<Task>(threads);

        for (int t = 0; t < threads; t++)
        {
            int threadId = t;
            int startIteration = threadId * iterationsPerThread;
            int endIteration = threadId == threads - 1
                ? _config.IterationCount
                : (threadId + 1) * iterationsPerThread;

            tasks.Add(Task.Run(() =>
            {
                var threadRandom = new Random(unchecked((int)(_config.Seed + threadId)));
                var point = new Point(threadRandom.NextDouble() * 2 - 1, threadRandom.NextDouble() * 2 - 1);

                int iterations = (endIteration - startIteration) + SkipIterations;

                for (int i = 0; i < iterations; i++)
                {
                    int functionIndex = _selector.Select(threadRandom);
                    point = _functions[functionIndex].Apply(point);

                    if (i >= SkipIterations)
                    {
                        _symmetry.Apply(
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

        for (int t = 0; t < threads; t++)
        {
            var d = localDensity[t];
            var r = localColorR[t];
            var g = localColorG[t];
            var b = localColorB[t];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    density[x, y] += d[x, y];
                    colorR[x, y] += r[x, y];
                    colorG[x, y] += g[x, y];
                    colorB[x, y] += b[x, y];
                }
            }
        }
    }
}
