using TodoListPrototype.Models;
using TodoListPrototype.Services;

namespace TodoListPrototype.Presentation;

public sealed class ConsoleMenu
{
    private readonly TodoService _todoService;

    public ConsoleMenu(TodoService todoService)
    {
        _todoService = todoService;
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

    private void PrintTask(TaskItem task)
    {
        var status = task.IsCompleted ? "Выполнено" : "Активно";
        var deadline = task.Deadline?.ToString("yyyy-MM-dd") ?? "нет";
        var category = _todoService.GetCategoryName(task.CategoryId);

        System.Console.WriteLine($"#{task.Id} [{status}] {task.Title}");
        System.Console.WriteLine($"Категория: {category}; дедлайн: {deadline}");

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
