using System.Diagnostics;
using System.Text.RegularExpressions;
using TodoListPrototype.Models;

namespace TodoListPrototype.Services;

public sealed class TaskSearchService
{
    public IReadOnlyList<TaskItem> Search(IEnumerable<TaskItem> tasks, string query, SearchMode mode)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        return mode switch
        {
            SearchMode.Regex => SearchByRegex(tasks, query),
            _ => SearchByContains(tasks, query)
        };
    }

    public SearchBenchmarkResult Benchmark(IEnumerable<TaskItem> tasks, string query, int iterations = 1_000)
    {
        var taskList = tasks.ToList();
        var containsElapsed = Measure(() => SearchByContains(taskList, query), iterations);
        var regexElapsed = Measure(() => SearchByRegex(taskList, query), iterations);

        return new SearchBenchmarkResult(containsElapsed, regexElapsed);
    }

    private static IReadOnlyList<TaskItem> SearchByContains(IEnumerable<TaskItem> tasks, string query)
    {
        return tasks
            .Where(task =>
                task.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                task.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyList<TaskItem> SearchByRegex(IEnumerable<TaskItem> tasks, string query)
    {
        var regex = new Regex(Regex.Escape(query), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return tasks
            .Where(task => regex.IsMatch(task.Title) || regex.IsMatch(task.Description))
            .ToList();
    }

    private static TimeSpan Measure(Action action, int iterations)
    {
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < iterations; i++)
        {
            action();
        }

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}

public sealed record SearchBenchmarkResult(TimeSpan ContainsElapsed, TimeSpan RegexElapsed)
{
    public SearchMode Winner => ContainsElapsed <= RegexElapsed ? SearchMode.Contains : SearchMode.Regex;
}
