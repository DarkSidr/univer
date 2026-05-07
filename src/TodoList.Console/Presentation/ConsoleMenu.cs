using TodoListPrototype.Models;
using TodoListPrototype.Services;

namespace TodoListPrototype.Presentation;

public sealed class ConsoleMenu
{
    private readonly TodoService _todoService;
    private readonly TaskSearchService _searchService;
    private readonly IAnalyticsLogger _analytics;

    public ConsoleMenu(TodoService todoService, TaskSearchService searchService, IAnalyticsLogger analytics)
    {
        _todoService = todoService;
        _searchService = searchService;
        _analytics = analytics;
    }

    public void Run()
    {
        while (true)
        {
            PrintHeader();
            System.Console.WriteLine("1. Показать все задачи");
            System.Console.WriteLine("2. Показать задачи по категории");
            System.Console.WriteLine("3. Добавить задачу");
            System.Console.WriteLine("4. Редактировать задачу");
            System.Console.WriteLine("5. Отметить задачу выполненной");
            System.Console.WriteLine("6. Удалить задачу");
            System.Console.WriteLine("7. Найти задачу");
            System.Console.WriteLine("8. A/B тест поиска");
            System.Console.WriteLine("0. Выход");
            System.Console.Write("Выберите пункт: ");

            var choice = System.Console.ReadLine();
            System.Console.Clear();

            switch (choice)
            {
                case "1":
                    ShowTasks();
                    break;
                case "2":
                    ShowTasksByCategory();
                    break;
                case "3":
                    AddTask();
                    break;
                case "4":
                    EditTask();
                    break;
                case "5":
                    MarkCompleted();
                    break;
                case "6":
                    DeleteTask();
                    break;
                case "7":
                    SearchTasks();
                    break;
                case "8":
                    RunSearchBenchmark();
                    break;
                case "0":
                    return;
                default:
                    WriteError("Неизвестный пункт меню.");
                    break;
            }

            Pause();
        }
    }

    private static void PrintHeader()
    {
        System.Console.WriteLine("Персональный менеджер задач");
        System.Console.WriteLine(new string('-', 32));
    }

    private void ShowTasks(int? categoryId = null)
    {
        var tasks = _todoService.GetTasks(categoryId);

        if (tasks.Count == 0)
        {
            System.Console.WriteLine("Задач пока нет.");
            return;
        }

        foreach (var task in tasks)
        {
            PrintTask(task);
        }
    }

    private void ShowTasksByCategory()
    {
        var categoryId = ReadCategoryId();
        ShowTasks(categoryId);
    }

    private void AddTask()
    {
        try
        {
            var title = ReadRequiredString("Название: ");
            var description = ReadOptionalString("Описание: ");
            var deadline = ReadOptionalDate("Дедлайн (yyyy-MM-dd, пусто если нет): ");
            var categoryId = ReadCategoryId();

            var task = _todoService.AddTask(title, description, deadline, categoryId);
            System.Console.WriteLine($"Задача #{task.Id} добавлена.");
        }
        catch (ArgumentException exception)
        {
            WriteError(exception.Message);
        }
    }

    private void EditTask()
    {
        var id = ReadInt("ID задачи: ");
        var currentTask = _todoService.GetTaskById(id);

        if (currentTask is null)
        {
            WriteError("Задача не найдена.");
            return;
        }

        try
        {
            System.Console.WriteLine("Оставьте поле пустым, чтобы сохранить текущее значение.");

            var title = ReadStringWithDefault($"Название ({currentTask.Title}): ", currentTask.Title);
            var description = ReadStringWithDefault($"Описание ({DisplayText(currentTask.Description)}): ", currentTask.Description);
            var deadline = ReadDateWithDefault(currentTask.Deadline);
            var categoryId = ReadCategoryIdWithDefault(currentTask.CategoryId);
            var isCompleted = ReadBoolWithDefault(currentTask.IsCompleted);

            var updated = _todoService.UpdateTask(id, title, description, deadline, categoryId, isCompleted);
            System.Console.WriteLine(updated ? "Задача обновлена." : "Задача не найдена.");
        }
        catch (ArgumentException exception)
        {
            WriteError(exception.Message);
        }
    }

    private void MarkCompleted()
    {
        var id = ReadInt("ID задачи: ");
        var updated = _todoService.MarkCompleted(id);
        System.Console.WriteLine(updated ? "Задача отмечена как выполненная." : "Задача не найдена.");
    }

    private void DeleteTask()
    {
        var id = ReadInt("ID задачи: ");
        var deleted = _todoService.DeleteTask(id);
        System.Console.WriteLine(deleted ? "Задача удалена." : "Задача не найдена.");
    }

    private void SearchTasks()
    {
        var query = ReadRequiredString("Поисковый запрос: ");
        var mode = ReadSearchMode();
        var tasks = _todoService.GetTasks();
        var results = _searchService.Search(tasks, query, mode);

        _analytics.Log("task_search_used", new Dictionary<string, object?>
        {
            ["mode"] = mode.ToString(),
            ["query_length"] = query.Length,
            ["results_count"] = results.Count
        });

        System.Console.WriteLine($"Найдено задач: {results.Count}");
        System.Console.WriteLine();

        foreach (var task in results)
        {
            PrintTask(task);
        }
    }

    private void RunSearchBenchmark()
    {
        var query = ReadRequiredString("Запрос для A/B теста: ");
        var tasks = _todoService.GetTasks();
        var result = _searchService.Benchmark(tasks, query);

        _analytics.Log("search_ab_test_completed", new Dictionary<string, object?>
        {
            ["contains_ms"] = result.ContainsElapsed.TotalMilliseconds,
            ["regex_ms"] = result.RegexElapsed.TotalMilliseconds,
            ["winner"] = result.Winner.ToString()
        });

        System.Console.WriteLine("Результат A/B теста поиска:");
        System.Console.WriteLine($"A Contains: {result.ContainsElapsed.TotalMilliseconds:F3} мс");
        System.Console.WriteLine($"B Regex:    {result.RegexElapsed.TotalMilliseconds:F3} мс");
        System.Console.WriteLine($"Победитель: {DisplaySearchMode(result.Winner)}");
    }

    private void PrintTask(TaskItem task)
    {
        var status = task.IsCompleted ? "Выполнено" : "Активно";
        var deadline = task.Deadline?.ToString("yyyy-MM-dd") ?? "нет";
        var category = _todoService.GetCategoryName(task.CategoryId);

        System.Console.WriteLine($"#{task.Id} [{status}] {task.Title}");
        System.Console.WriteLine($"Категория: {category}; дедлайн: {deadline}");
        System.Console.WriteLine($"Важность: {task.Importance}");

        if (!string.IsNullOrWhiteSpace(task.Description))
        {
            System.Console.WriteLine($"Описание: {task.Description}");
        }

        System.Console.WriteLine();
    }

    private int ReadCategoryId()
    {
        System.Console.WriteLine("Категории:");

        foreach (var category in _todoService.GetCategories())
        {
            System.Console.WriteLine($"{category.Id}. {category.Name}");
        }

        return ReadInt("Выберите категорию: ");
    }

    private int ReadCategoryIdWithDefault(int currentCategoryId)
    {
        System.Console.WriteLine("Категории:");

        foreach (var category in _todoService.GetCategories())
        {
            System.Console.WriteLine($"{category.Id}. {category.Name}");
        }

        System.Console.Write($"Категория ({currentCategoryId}): ");
        var input = System.Console.ReadLine();

        return string.IsNullOrWhiteSpace(input) ? currentCategoryId : ParseInt(input);
    }

    private static SearchMode ReadSearchMode()
    {
        while (true)
        {
            System.Console.WriteLine("Режим поиска:");
            System.Console.WriteLine("1. Version A: Contains");
            System.Console.WriteLine("2. Version B: Regex");
            System.Console.Write("Выберите режим: ");

            var input = System.Console.ReadLine();

            if (input == "1")
            {
                return SearchMode.Contains;
            }

            if (input == "2")
            {
                return SearchMode.Regex;
            }

            WriteError("Введите 1 или 2.");
        }
    }

    private static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            WriteError("Поле обязательно для заполнения.");
        }
    }

    private static string ReadOptionalString(string prompt)
    {
        System.Console.Write(prompt);
        return System.Console.ReadLine() ?? string.Empty;
    }

    private static string ReadStringWithDefault(string prompt, string currentValue)
    {
        System.Console.Write(prompt);
        var input = System.Console.ReadLine();

        return string.IsNullOrWhiteSpace(input) ? currentValue : input;
    }

    private static DateOnly? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (DateOnly.TryParse(input, out var date))
            {
                return date;
            }

            WriteError("Введите дату в формате yyyy-MM-dd.");
        }
    }

    private static DateOnly? ReadDateWithDefault(DateOnly? currentDeadline)
    {
        var currentText = currentDeadline?.ToString("yyyy-MM-dd") ?? "нет";

        while (true)
        {
            System.Console.Write($"Дедлайн ({currentText}, '-' чтобы очистить): ");
            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return currentDeadline;
            }

            if (input.Trim() == "-")
            {
                return null;
            }

            if (DateOnly.TryParse(input, out var date))
            {
                return date;
            }

            WriteError("Введите дату в формате yyyy-MM-dd.");
        }
    }

    private static bool ReadBoolWithDefault(bool currentValue)
    {
        while (true)
        {
            var currentText = currentValue ? "да" : "нет";
            System.Console.Write($"Выполнено ({currentText}, да/нет): ");
            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return currentValue;
            }

            var normalized = input.Trim().ToLowerInvariant();

            if (normalized is "да" or "д" or "yes" or "y")
            {
                return true;
            }

            if (normalized is "нет" or "н" or "no" or "n")
            {
                return false;
            }

            WriteError("Введите да или нет.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            System.Console.Write(prompt);
            var input = System.Console.ReadLine();

            if (int.TryParse(input, out var value))
            {
                return value;
            }

            WriteError("Введите целое число.");
        }
    }

    private static int ParseInt(string input)
    {
        if (!int.TryParse(input, out var value))
        {
            throw new ArgumentException("Введите целое число.");
        }

        return value;
    }

    private static string DisplayText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "пусто" : value;
    }

    private static string DisplaySearchMode(SearchMode mode)
    {
        return mode == SearchMode.Contains ? "Version A: Contains" : "Version B: Regex";
    }

    private static void WriteError(string message)
    {
        var oldColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(message);
        System.Console.ForegroundColor = oldColor;
    }

    private static void Pause()
    {
        System.Console.WriteLine();
        System.Console.Write("Нажмите Enter для продолжения...");
        System.Console.ReadLine();
        System.Console.Clear();
    }
}
