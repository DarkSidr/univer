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
var searchService = new TaskSearchService();
var menu = new ConsoleMenu(todoService, searchService, analytics);

menu.Run();
