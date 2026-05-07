using TodoListPrototype.Data;
using TodoListPrototype.Models;

namespace TodoListPrototype.Services;

public sealed class TodoService
{
    private readonly IStorage _storage;
    private readonly IAnalyticsLogger _analytics;
    private readonly TaskImportanceAnalyzer _importanceAnalyzer;
    private readonly TodoData _data;

    public TodoService(IStorage storage, IAnalyticsLogger analytics, TaskImportanceAnalyzer importanceAnalyzer)
    {
        _storage = storage;
        _analytics = analytics;
        _importanceAnalyzer = importanceAnalyzer;
        _data = storage.Load();

        if (_data.Categories.Count == 0)
        {
            _data.Categories.AddRange(Category.DefaultCategories);
        }

        UpdateMissingImportance();
    }

    public IReadOnlyList<Category> GetCategories()
    {
        return _data.Categories
            .OrderBy(category => category.Id)
            .ToList();
    }

    public IReadOnlyList<TaskItem> GetTasks(int? categoryId = null)
    {
        var tasks = _data.Tasks.AsEnumerable();

        if (categoryId.HasValue)
        {
            tasks = tasks.Where(task => task.CategoryId == categoryId.Value);
            _analytics.Log("category_filtered", new Dictionary<string, object?> { ["category_id"] = categoryId.Value });
        }
        else
        {
            _analytics.Log("tasks_listed");
        }

        return tasks
            .OrderBy(task => task.IsCompleted)
            .ThenBy(task => task.Deadline is null)
            .ThenBy(task => task.Deadline)
            .ThenBy(task => task.Id)
            .ToList();
    }

    public TaskItem? GetTaskById(int id)
    {
        return _data.Tasks.FirstOrDefault(task => task.Id == id);
    }

    public TaskItem AddTask(string title, string description, DateOnly? deadline, int categoryId)
    {
        ValidateTitle(title);
        ValidateCategory(categoryId);

        var nextId = _data.Tasks.Count == 0 ? 1 : _data.Tasks.Max(task => task.Id) + 1;
        var task = new TaskItem
        {
            Id = nextId,
            Title = title.Trim(),
            Description = description.Trim(),
            Deadline = deadline,
            CategoryId = categoryId,
            Importance = _importanceAnalyzer.Analyze(title, description, deadline),
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        _data.Tasks.Add(task);
        Save();
        _analytics.Log("task_created", TaskProperties(task));

        return task;
    }

    public bool UpdateTask(int id, string title, string description, DateOnly? deadline, int categoryId, bool isCompleted)
    {
        ValidateTitle(title);
        ValidateCategory(categoryId);

        var task = GetTaskById(id);

        if (task is null)
        {
            _analytics.Log("task_update_failed", new Dictionary<string, object?> { ["task_id"] = id });
            return false;
        }

        task.Title = title.Trim();
        task.Description = description.Trim();
        task.Deadline = deadline;
        task.CategoryId = categoryId;
        task.Importance = _importanceAnalyzer.Analyze(task);
        task.IsCompleted = isCompleted;
        Save();
        _analytics.Log("task_updated", TaskProperties(task));

        return true;
    }

    public bool DeleteTask(int id)
    {
        var task = GetTaskById(id);

        if (task is null)
        {
            _analytics.Log("task_delete_failed", new Dictionary<string, object?> { ["task_id"] = id });
            return false;
        }

        _data.Tasks.Remove(task);
        Save();
        _analytics.Log("task_deleted", TaskProperties(task));

        return true;
    }

    public bool MarkCompleted(int id)
    {
        var task = GetTaskById(id);

        if (task is null)
        {
            _analytics.Log("task_complete_failed", new Dictionary<string, object?> { ["task_id"] = id });
            return false;
        }

        task.IsCompleted = true;
        Save();
        _analytics.Log("task_completed", TaskProperties(task));

        return true;
    }

    public string GetCategoryName(int categoryId)
    {
        return _data.Categories.FirstOrDefault(category => category.Id == categoryId)?.Name ?? "Без категории";
    }

    private void Save()
    {
        _storage.Save(_data);
    }

    private void UpdateMissingImportance()
    {
        var changed = false;

        foreach (var task in _data.Tasks.Where(task => string.IsNullOrWhiteSpace(task.Importance)))
        {
            task.Importance = _importanceAnalyzer.Analyze(task);
            changed = true;
        }

        if (changed)
        {
            Save();
        }
    }

    private static Dictionary<string, object?> TaskProperties(TaskItem task)
    {
        return new Dictionary<string, object?>
        {
            ["task_id"] = task.Id,
            ["category_id"] = task.CategoryId,
            ["has_description"] = !string.IsNullOrWhiteSpace(task.Description),
            ["has_deadline"] = task.Deadline.HasValue,
            ["importance"] = task.Importance,
            ["is_completed"] = task.IsCompleted
        };
    }

    private void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            _analytics.Log("validation_failed", new Dictionary<string, object?> { ["field"] = "title" });
            throw new ArgumentException("Название задачи не может быть пустым.");
        }
    }

    private void ValidateCategory(int categoryId)
    {
        if (_data.Categories.All(category => category.Id != categoryId))
        {
            _analytics.Log("validation_failed", new Dictionary<string, object?> { ["field"] = "category_id" });
            throw new ArgumentException("Выбрана неизвестная категория.");
        }
    }
}
