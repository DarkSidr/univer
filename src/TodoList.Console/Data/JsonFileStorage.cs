using System.Text.Encodings.Web;
using System.Text.Json;
using TodoListPrototype.Models;

namespace TodoListPrototype.Data;

public sealed class JsonFileStorage : IStorage
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonFileStorage(string filePath)
    {
        _filePath = filePath;
    }

    public TodoData Load()
    {
        if (!File.Exists(_filePath))
        {
            return new TodoData();
        }

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new TodoData();
        }

        return JsonSerializer.Deserialize<TodoData>(json, _options) ?? new TodoData();
    }

    public void Save(TodoData data)
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(_filePath, json);
    }
}
