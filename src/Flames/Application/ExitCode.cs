namespace Flames.Application;

public enum ExitCode
{
    Success = 0,
    UserError = 1,     // неверные аргументы/валидация/файл конфига
    ConfigError = 2,   // ошибка чтения/парсинга конфига
    RuntimeError = 10  // непредвиденная ошибка выполнения
}
