namespace TodoListPrototype.Services;

public sealed record StressTestResult(
    int GeneratedTasks,
    double GenerationMilliseconds,
    SearchBenchmarkResult SearchBenchmark,
    OperationMetrics FilterMetrics);
