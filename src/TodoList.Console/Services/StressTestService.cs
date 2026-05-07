using System.Diagnostics;
using TodoListPrototype.Models;

namespace TodoListPrototype.Services;

public sealed class StressTestService
{
    private readonly TodoService _todoService;
    private readonly TaskSearchService _searchService;
    private readonly IAnalyticsLogger _analytics;

    public StressTestService(TodoService todoService, TaskSearchService searchService, IAnalyticsLogger analytics)
    {
        _todoService = todoService;
        _searchService = searchService;
        _analytics = analytics;
    }

    public StressTestResult Run(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Количество задач должно быть больше нуля.");
        }

        var stopwatch = Stopwatch.StartNew();

        for (var i = 1; i <= count; i++)
        {
            var categoryId = i % 3 + 1;
            var deadline = DateOnly.FromDateTime(DateTime.Today.AddDays(i % 30));
            _todoService.AddTask(
                $"Нагрузочная задача {i}",
                $"Автоматически созданная задача для стресс-теста. Пакет {i % 10}.",
                deadline,
                categoryId);
        }

        stopwatch.Stop();
        var allTasks = _todoService.GetTasks();
        var searchBenchmark = _searchService.Benchmark(allTasks, "Нагрузочная", 100);
        var filterMetrics = MeasureFilter(allTasks, categoryId: 2);

        var result = new StressTestResult(count, stopwatch.Elapsed.TotalMilliseconds, searchBenchmark, filterMetrics);
        _analytics.Log("stress_test_completed", new Dictionary<string, object?>
        {
            ["generated_tasks"] = result.GeneratedTasks,
            ["generation_ms"] = result.GenerationMilliseconds,
            ["contains_ms"] = result.SearchBenchmark.ContainsElapsed.TotalMilliseconds,
            ["regex_ms"] = result.SearchBenchmark.RegexElapsed.TotalMilliseconds,
            ["filter_ms"] = result.FilterMetrics.ElapsedMilliseconds
        });

        return result;
    }

    private static OperationMetrics MeasureFilter(IEnumerable<TaskItem> tasks, int categoryId)
    {
        var taskList = tasks.ToList();
        var stopwatch = Stopwatch.StartNew();
        var filtered = taskList.Where(task => task.CategoryId == categoryId).ToList();
        stopwatch.Stop();

        return new OperationMetrics("category_filter", filtered.Count, stopwatch.Elapsed.TotalMilliseconds);
    }
}
