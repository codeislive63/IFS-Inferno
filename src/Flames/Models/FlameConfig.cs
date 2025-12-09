namespace Flames.Models;

/// <summary>
/// Конфигурация генерации фрактального пламени
/// </summary>
public sealed record class FlameConfig
{
    /// <summary>
    /// Размер итогового изображения (по умолчанию 1920 X 1080)
    /// </summary>
    public Size Size { get; init; } = new(1920, 1080);

    /// <summary>
    /// Количество итераций генерации
    /// </summary>
    public int IterationCount { get; init; } = 2500;

    /// <summary>
    /// Путь для сохранения итогового изображения
    /// </summary>
    public string OutputPath { get; init; } = "result.png";

    /// <summary>
    /// Количество потоков рендера
    /// </summary>
    public int Threads { get; init; } = 1;

    /// <summary>
    /// Начальное значение генератора случайных чисел
    /// </summary>
    public long Seed { get; init; } = 5;

    /// <summary>
    /// Конфигурации трансформационных функций
    /// </summary>
    public IReadOnlyList<FunctionConfig> Functions { get; init; } = [];

    /// <summary>
    /// Параметры аффинных преобразований
    /// </summary>
    public IReadOnlyList<AffineParameters> AffineParams { get; init; } = [];

    /// <summary>
    /// Включает гамма-коррекцию яркости
    /// </summary>
    public bool IsGammaCorrectionEnabled { get; init; } = false;

    /// <summary>
    /// Значение гаммы для коррекции яркости
    /// </summary>
    public double GammaCorrection { get; init; } = 2.2;

    /// <summary>
    /// Уровень симметрии для дублирования точек
    /// </summary>
    public int SymmetryLevel { get; init; } = 1;

    public FlameConfig() { }

    public FlameConfig(
        Size size,
        int iterationCount,
        string outputPath,
        int threads,
        long seed,
        IReadOnlyList<FunctionConfig> functions,
        IReadOnlyList<AffineParameters> affineParams,
        bool gammaCorrection,
        double gamma,
        int symmetryLevel)
    {
        Size = size;
        IterationCount = iterationCount;
        OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        Threads = threads;
        Seed = seed;
        Functions = functions ?? throw new ArgumentNullException(nameof(functions));
        AffineParams = affineParams ?? throw new ArgumentNullException(nameof(affineParams));
        IsGammaCorrectionEnabled = gammaCorrection;
        GammaCorrection = gamma;
        SymmetryLevel = symmetryLevel;
    }
}
