using System.Text.Encodings.Web;
using System.Text.Json;

namespace TodoListPrototype.Services;

public sealed class JsonLinesAnalyticsLogger : IAnalyticsLogger
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonLinesAnalyticsLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string eventName, IReadOnlyDictionary<string, object?>? properties = null)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var entry = new Dictionary<string, object?>
        {
            ["event"] = eventName,
            ["user"] = "local_user",
            ["time"] = DateTimeOffset.Now.ToString("O")
        };

        if (properties is not null)
        {
            foreach (var property in properties)
            {
                entry[property.Key] = property.Value;
            }
        }

        File.AppendAllText(_filePath, JsonSerializer.Serialize(entry, _options) + Environment.NewLine);
    }
}
