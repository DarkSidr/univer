namespace TodoListPrototype.Services;

public interface IAnalyticsLogger
{
    void Log(string eventName, IReadOnlyDictionary<string, object?>? properties = null);
}
