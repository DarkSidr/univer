using TodoListPrototype.Data;
using TodoListPrototype.Presentation;
using TodoListPrototype.Services;

var dataPath = Path.Combine(AppContext.BaseDirectory, "tasks.json");
var analyticsPath = Path.Combine(AppContext.BaseDirectory, "analytics.log");

var storage = new JsonFileStorage(dataPath);
var analytics = new JsonLinesAnalyticsLogger(analyticsPath);
analytics.Log("app_started");

var todoService = new TodoService(storage, analytics);
var menu = new ConsoleMenu(todoService);

menu.Run();
