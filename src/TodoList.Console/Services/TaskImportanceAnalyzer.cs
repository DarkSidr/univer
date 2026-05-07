using TodoListPrototype.Models;

namespace TodoListPrototype.Services;

public sealed class TaskImportanceAnalyzer
{
    private static readonly string[] HighImportanceKeywords =
    [
        "срочно",
        "важно",
        "экзамен",
        "дедлайн",
        "отчет",
        "защита",
        "проект",
        "оплата"
    ];

    public string Analyze(string title, string description, DateOnly? deadline)
    {
        var text = $"{title} {description}".ToLowerInvariant();
        var score = 0;

        if (HighImportanceKeywords.Any(text.Contains))
        {
            score += 2;
        }

        if (deadline.HasValue)
        {
            var daysLeft = deadline.Value.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

            if (daysLeft <= 1)
            {
                score += 3;
            }
            else if (daysLeft <= 3)
            {
                score += 2;
            }
            else if (daysLeft <= 7)
            {
                score += 1;
            }
        }

        return score switch
        {
            >= 4 => "Высокая",
            >= 2 => "Средняя",
            _ => "Обычная"
        };
    }

    public string Analyze(TaskItem task)
    {
        return Analyze(task.Title, task.Description, task.Deadline);
    }
}
