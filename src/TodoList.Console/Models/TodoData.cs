namespace TodoListPrototype.Models;

public sealed class TodoData
{
    public List<Category> Categories { get; set; } = Category.DefaultCategories
        .Select(category => new Category { Id = category.Id, Name = category.Name })
        .ToList();

    public List<TaskItem> Tasks { get; set; } = [];
}
