namespace TodoListPrototype.Services;

public sealed record OperationMetrics(string Operation, int ItemsCount, double ElapsedMilliseconds);
