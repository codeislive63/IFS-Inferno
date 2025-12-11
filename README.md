# Итоговая сводка проекта

### Реализовано:
- Однопоточный и многопоточный режимы работы
- Цветной алгоритм генерации
- 6 трансформаций: linear, swirl, horseshoe, disc, polar, spherical
- Поддержка консольного ввода, JSON конфигурации и параметров по умолчанию
- Сохранение изображений в формате PNG, RGB, 8 бит на канал
- Логирование прогресса работы

### Бонусные задания выполнены
- **5+ трансформаций** (реализовано 6)
- **Гамма-коррекция**
- **Симметрия**


# Команды для тестирования проекта

### Тестирование

### Пример 1: Простой запуск с параметрами по умолчанию
```bash
dotnet run --project src/Flames/Flames.csproj
```

### Пример 2: Запуск с параметрами командной строки
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 800 -h 600 -i 1000 -f "swirl:1.0,linear:0.5" -ap "0.5,0,0,0,0.5,0/0.3,0,0.3,0,0.3,0" -t 1 -o test1.png
```

### Пример 3: Запуск с несколькими трансформациями
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 5000 -f "swirl:1.0,horseshoe:0.8,disc:0.6" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5/0.4,0.1,-0.2,-0.1,0.4,0.2" -t 4 -o test2.png
```

### Пример 4: Запуск с гамма-коррекцией и симметрией
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 5000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 4 -g --gamma 2.2 -s 3 -o test3.png
```

### Пример 5: Запуск с JSON конфигурацией
```bash
dotnet run --project src/Flames/Flames.csproj -- --config config.json
```

### Пример 6: Запуск с примером конфигурации (5 трансформаций)
```bash
dotnet run --project src/Flames/Flames.csproj -- --config example_config.json
```

### Пример 7: Запуск в режиме многопоточности (1 поток)
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 1 -o test_single_thread.png
```

### Пример 8: Запуск в режиме многопоточности (4 потока)
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 4 -o test_multi_thread.png
```

### Пример 9: Тестирование всех трансформаций
```bash
dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 5000 -f "linear:1.0,swirl:1.0,horseshoe:0.8,disc:0.6,polar:0.7,spherical:0.5" -ap "0.5,0,0,0,0.5,0/0.4,0.1,0,0,0.4,0.1/0.3,0,0.5,0,0.3,0.5/0.4,0.1,-0.2,-0.1,0.4,0.2/0.35,0.05,0.3,-0.05,0.35,-0.3/0.45,-0.1,-0.4,0.1,0.45,0.4" -t 4 -o test_all_transforms.png
```

## Тестирование производительности

### Сравнение однопоточного и многопоточного режимов
```bash
# 1 поток
Measure-Command { dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 1 -o perf1.png }

# 2 потока
Measure-Command { dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 2 -o perf2.png }

# 4 потока
Measure-Command { dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 4 -o perf4.png }

# 8 потоков
Measure-Command { dotnet run --project src/Flames/Flames.csproj -- -w 1920 -h 1080 -i 10000 -f "swirl:1.0,horseshoe:0.8" -ap "0.5,0,0,0,0.5,0/0.3,0,0.5,0,0.3,0.5" -t 8 -o perf8.png }
```

## Пример JSON-конфигурации

```json
{
  "size": {
    "width": 1920,
    "height": 1080
  },
  "iterationCount": 5000,
  "outputPath": "example_flame.png",
  "threads": 4,
  "seed": 42,
  "functions": [
    { "name": "swirl", "weight": 1.0 },
    { "name": "horseshoe", "weight": 0.8 },
    { "name": "disc", "weight": 0.6 },
    { "name": "polar", "weight": 0.7 },
    { "name": "spherical", "weight": 0.5 }
  ],
  "affineParams": [
    { "a": 0.5, "b": 0.0, "c": 0.0, "d": 0.0, "e": 0.5, "f": 0.0 },
    { "a": 0.3, "b": 0.0, "c": 0.5, "d": 0.0, "e": 0.3, "f": 0.5 },
    { "a": 0.4, "b": 0.1, "c": -0.2, "d": -0.1, "e": 0.4, "f": 0.2 },
    { "a": 0.35, "b": 0.05, "c": 0.3, "d": -0.05, "e": 0.35, "f": -0.3 },
    { "a": 0.45, "b": -0.1, "c": -0.4, "d": 0.1, "e": 0.45, "f": 0.4 }
  ],
  "isGammaCorrectionEnabled": true,
  "gammaCorrection": 2.2,
  "symmetryLevel": 3
}
```

## Отчёт о покрытии кода
<img width="2283" height="753" alt="Снимок экрана 2025-12-09 172026" src="https://github.com/user-attachments/assets/f4d442c9-60e8-41c4-9ae4-e8f445239386" />
