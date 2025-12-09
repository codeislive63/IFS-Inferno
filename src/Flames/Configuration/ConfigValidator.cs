using Flames.Models;
using Flames.Transformations;

namespace Flames.Configuration;

/// <summary>
/// Валидатор конфигурации генерации фрактального пламени
/// </summary>
public static class ConfigValidator
{
    /// <summary>
    /// Валидирует конфигурацию и возвращает список ошибок
    /// </summary>
    public static List<string> Validate(FlameConfig config)
    {
        var errors = new List<string>();

        void AddError(bool condition, string message)
        {
            if (condition)
            {
                errors.Add(message);
            }
        }

        AddError(config.Size.Width <= 0 || config.Size.Height <= 0,
            "Размер изображения должен быть положительным");

        AddError(config.IterationCount <= 0,
            "Количество итераций должно быть положительным");

        AddError(config.Threads <= 0,
            "Количество потоков должно быть положительным");

        AddError(string.IsNullOrWhiteSpace(config.OutputPath),
            "Путь для сохранения изображения не может быть пустым");

        AddError(config.Functions.Count == 0,
            "Должна быть указана хотя бы одна функция трансформации");

        foreach (var func in config.Functions)
        {
            AddError(string.IsNullOrWhiteSpace(func.Name),
                "Имя функции трансформации не может быть пустым");

            if (!string.IsNullOrWhiteSpace(func.Name))
            {
                AddError(TransformationFactory.Create(func.Name) is null,
                    $"Неизвестная функция трансформации: {func.Name}");
            }

            AddError(func.Weight < 0,
                $"Вес функции \"{func.Name}\" не может быть отрицательным");
        }

        AddError(config.AffineParams.Count == 0,
            "Должен быть указан хотя бы один набор аффинных параметров");

        AddError(config.AffineParams.Count != config.Functions.Count,
            $"Количество аффинных параметров ({config.AffineParams.Count}) должно совпадать с количеством функций ({config.Functions.Count})");

        AddError(config.GammaCorrection <= 0,
            "Значение гаммы должно быть положительным числом");

        AddError(config.SymmetryLevel < 1,
            "Уровень симметрии должен быть >= 1");

        return errors;
    }
}
