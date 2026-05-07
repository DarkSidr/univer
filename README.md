# Todo List Prototype

Учебный проект по дисциплине "Современные технологии разработки программного обеспечения".

## Вариант

Вариант 1: персональный менеджер задач (Todo List) с категориями.

## Лабораторные работы

- [Лабораторная работа №1: анализ и исследование проблем](docs/Research.md)
- [Лабораторная работа №2: проектирование и управление требованиями](docs/Design.md)
- [Лабораторная работа №3: план проверки гипотез](docs/HypothesisCheckPlan.md)

## MVP

В рамках лабораторной работы №3 реализовано консольное приложение на C# и .NET 8.

Функции MVP:

- CRUD для задач;
- категории "Работа", "Учеба", "Личное";
- дедлайн и статус "Выполнено";
- сохранение и загрузка задач из JSON;
- логирование событий в `analytics.log`.

## Запуск

```bash
dotnet run --project src/TodoList.Console/TodoList.Console.csproj
```

## Сборка

```bash
dotnet build TodoListPrototype.sln
```

## Структура проекта

```text
.
├── docs/
│   ├── diagrams/
│   │   ├── as-is.mmd
│   │   ├── as-is.png
│   │   ├── to-be.mmd
│   │   └── to-be.png
│   ├── wireframes.md
│   ├── HypothesisCheckPlan.md
│   ├── Design.md
│   └── Research.md
├── src/
│   └── TodoList.Console/
├── .github/
│   └── workflows/
│       └── dotnet.yml
├── TodoListPrototype.sln
├── .gitignore
└── README.md
```

## Использование AI

При выполнении использовался AI-ассистент как аналог GitHub Copilot:

- для подготовки структуры проблемного интервью;
- для формулировки критериев конкурентного анализа;
- для уточнения JTBD и гипотез будущего прототипа;
- для генерации Acceptance Criteria, Use Case и backlog во второй лабораторной работе;
- для генерации CRUD-логики, JSON-сериализации, валидации и аналитических событий в MVP.
