using Flames.Configuration;
using Flames.Models;
using FluentAssertions;
using Xunit;

namespace Flames.Tests.Configuration;

/// <summary>
/// Тесты для валидатора конфигурации FlameConfig
/// </summary>
public class ConfigValidatorTests
{
    /// <summary>
    /// Должен возвращать пустой список ошибок при полностью валидной конфигурации
    /// </summary>
    [Fact]
    public void Validate_WithValidConfig_ShouldReturnNoErrors()
    {
        var config = new FlameConfig
        {
            Size = new Size(1920, 1080),
            IterationCount = 2500,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = 1.0 }],
            AffineParams = [new AffineParameters(0.5, 0, 0, 0, 0.5, 0)]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Некорректный размер изображения должен вызывать ошибку
    /// </summary>
    [Fact]
    public void Validate_WithInvalidSize_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(-1, 1080),
            IterationCount = 2500,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = 1.0 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Размер изображения"));
    }

    /// <summary>
    /// Неизвестная функция трансформации должна вызывать ошибку
    /// </summary>
    [Fact]
    public void Validate_WithInvalidFunction_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(1920, 1080),
            IterationCount = 2500,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "invalid", Weight = 1.0 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Неизвестная функция"));
    }

    /// <summary>
    /// Количество функций и количество наборов аффинных параметров должны совпадать
    /// </summary>
    [Fact]
    public void Validate_WithMismatchedCounts_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(1920, 1080),
            IterationCount = 2500,
            Threads = 1,
            OutputPath = "test.png",
            Functions =
            [
                new() { Name = "linear", Weight = 1.0 }
            ],
            AffineParams =
            [
                new AffineParameters(),
                new AffineParameters()
            ]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Количество аффинных параметров"));
    }

    /// <summary>
    /// Количество итераций должно быть положительным
    /// </summary>
    [Fact]
    public void Validate_WithZeroIterationCount_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 0,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("итераций"));
    }

    /// <summary>
    /// Количество потоков должно быть > 0
    /// </summary>
    [Fact]
    public void Validate_WithZeroThreads_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 0,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Количество потоков"));
    }

    /// <summary>
    /// Пустой путь вывода должен считаться ошибкой
    /// </summary>
    [Fact]
    public void Validate_WithEmptyOutputPath_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "",
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Путь"));
    }

    /// <summary>
    /// Должна быть хотя бы одна функция трансформации
    /// </summary>
    [Fact]
    public void Validate_WithNoFunctions_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("хотя бы одна функция"));
    }

    /// <summary>
    /// Имя функции не может быть пустым
    /// </summary>
    [Fact]
    public void Validate_WithEmptyFunctionName_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("Имя функции"));
    }

    /// <summary>
    /// Вес функции не должен быть отрицательным
    /// </summary>
    [Fact]
    public void Validate_WithNegativeWeight_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = -1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("не может быть отрицательным"));
    }

    /// <summary>
    /// Должен быть хотя бы один набор аффинных параметров
    /// </summary>
    [Fact]
    public void Validate_WithNoAffineParams_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = []
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("хотя бы один набор"));
    }

    /// <summary>
    /// Гамма должна быть > 0
    /// </summary>
    [Fact]
    public void Validate_WithNonPositiveGamma_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            GammaCorrection = 0,
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("гамм", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Уровень симметрии должен быть ≥ 1
    /// </summary>
    [Fact]
    public void Validate_WithInvalidSymmetryLevel_ShouldReturnError()
    {
        var config = new FlameConfig
        {
            Size = new Size(100, 100),
            IterationCount = 10,
            Threads = 1,
            OutputPath = "test.png",
            SymmetryLevel = 0,
            Functions = [new() { Name = "linear", Weight = 1 }],
            AffineParams = [new AffineParameters()]
        };

        var errors = ConfigValidator.Validate(config);

        errors.Should().Contain(e => e.Contains("симметрии"));
    }
}
