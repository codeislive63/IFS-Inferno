using Flames.Core.Rendering;
using Flames.Models;
using Microsoft.Extensions.Logging;

namespace Flames.Core;

/// <summary>
/// Рендерер фрактального пламени
/// </summary>
public sealed class FlameRenderer
{
    private readonly ILogger<FlameRenderer> _logger;
    private readonly IReadOnlyList<FlameFunction> _functions;
    private readonly FlameConfig _config;

    private int Width => _config.Size.Width;
    private int Height => _config.Size.Height;

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

        if (_config.SymmetryLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(config), "SymmetryLevel должен быть > 0");
        }
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

        var selector = new WeightedFunctionSelector(_functions);
        var symmetry = new SymmetryApplier(Width, Height, _config.SymmetryLevel);
        var generator = new FlamePointGenerator(_config, _functions, selector, symmetry, _logger);

        generator.Generate(density, colorR, colorG, colorB);

        _logger.LogInformation("Генерация точек завершена. Применение log-density и создание изображения...");

        DensityPostProcessor.ApplyLogDensityAndNormalize(Width, Height, density, colorR, colorG, colorB);
        FlameImageSaver.SaveAsPng(_config, Width, Height, density, colorR, colorG, colorB);

        _logger.LogInformation("Изображение сохранено: {OutputPath}", _config.OutputPath);
    }
}
