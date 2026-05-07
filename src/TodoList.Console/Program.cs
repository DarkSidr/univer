using TodoListPrototype.Data;
using TodoListPrototype.Presentation;
using TodoListPrototype.Services;

var dataPath = Path.Combine(AppContext.BaseDirectory, "tasks.json");
var analyticsPath = Path.Combine(AppContext.BaseDirectory, "analytics.log");

var storage = new JsonFileStorage(dataPath);
var analytics = new JsonLinesAnalyticsLogger(analyticsPath);
var importanceAnalyzer = new TaskImportanceAnalyzer();
analytics.Log("app_started");

var todoService = new TodoService(storage, analytics, importanceAnalyzer);
var searchService = new TaskSearchService(analytics);

if (args.Length >= 2 && args[0] == "--stress-test" && int.TryParse(args[1], out var count))
{
    var stressTest = new StressTestService(todoService, searchService, analytics);
    var result = stressTest.Run(count);

    Console.WriteLine($"Создано задач: {result.GeneratedTasks}");
    Console.WriteLine($"Время генерации: {result.GenerationMilliseconds:F3} мс");
    Console.WriteLine($"Поиск Contains: {result.SearchBenchmark.ContainsElapsed.TotalMilliseconds:F3} мс");
    Console.WriteLine($"Поиск Regex: {result.SearchBenchmark.RegexElapsed.TotalMilliseconds:F3} мс");
    Console.WriteLine($"Фильтрация категории: {result.FilterMetrics.ElapsedMilliseconds:F3} мс");
    return;
}

var menu = new ConsoleMenu(todoService, searchService, analytics);

menu.Run();
