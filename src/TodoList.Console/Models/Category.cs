namespace TodoListPrototype.Models;

public sealed class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public static IReadOnlyList<Category> DefaultCategories { get; } =
    [
        new Category { Id = 1, Name = "Работа" },
        new Category { Id = 2, Name = "Учеба" },
        new Category { Id = 3, Name = "Личное" }
    ];
}
