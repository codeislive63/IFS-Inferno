namespace Flames.Core.Rendering;

/// <summary>
/// Выбирает индекс функции преобразования случайным образом с учетом её веса,
/// используя массив накопленных весов
/// </summary>
public sealed class WeightedFunctionSelector
{
    private readonly double[] _cumulativeWeights;
    private readonly double _totalWeight;

    public WeightedFunctionSelector(IReadOnlyList<FlameFunction> functions)
    {
        ArgumentNullException.ThrowIfNull(functions);

        if (functions.Count == 0)
        {
            throw new ArgumentException("Список функций пуст", nameof(functions));
        }

        _cumulativeWeights = new double[functions.Count];

        double total = 0;

        for (int i = 0; i < functions.Count; i++)
        {
            var w = Math.Max(0.0, functions[i].Weight);
            total += w;
            _cumulativeWeights[i] = total;
        }

        if (total <= 0)
        {
            throw new ArgumentException("Суммарный вес функций должен быть положительным", nameof(functions));
        }

        _totalWeight = total;
    }

    /// <summary>
    /// Возвращает индекс выбранной функции согласно её весу
    /// </summary>
    public int Select(Random random)
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
}
