using TodoListPrototype.Models;

namespace TodoListPrototype.Data;

public interface IStorage
{
    TodoData Load();

    void Save(TodoData data);
}
