using System.Collections.Concurrent;

namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Потокобезопасное хранилище для данных о выполнении тестов.
/// </summary>
public static class TestExecutionStorage
{
    private static readonly ConcurrentBag<TestExecutionInfo> _executions = new ConcurrentBag<TestExecutionInfo>();
    private static readonly ConcurrentDictionary<string, TestExecutionInfo> _pendingExecutions = new ConcurrentDictionary<string, TestExecutionInfo>();
    
    /// <summary>
    /// Добавляет информацию о начале выполнения теста в хранилище незавершенных тестов.
    /// </summary>
    /// <param name="testName">Имя теста.</param>
    /// <param name="info">Информация о выполнении теста.</param>
    public static void AddPendingExecution(string testName, TestExecutionInfo info)
    {
        _pendingExecutions[testName] = info;
    }
    
    /// <summary>
    /// Пытается получить информацию о незавершенном тесте из хранилища.
    /// </summary>
    /// <param name="testName">Имя теста.</param>
    /// <param name="info">Информация о выполнении теста (если найдена).</param>
    /// <returns>true, если тест найден в хранилище незавершенных тестов; иначе false.</returns>
    public static bool TryGetPendingExecution(string testName, out TestExecutionInfo info)
    {
        return _pendingExecutions.TryGetValue(testName, out info);
    }
    
    /// <summary>
    /// Помечает тест как завершенный, перемещая его из хранилища незавершенных в хранилище завершенных тестов.
    /// </summary>
    /// <param name="testName">Имя теста.</param>
    public static void CompletePendingExecution(string testName)
    {
        if (_pendingExecutions.TryRemove(testName, out var info))
        {
            _executions.Add(info);
        }
    }
    
    /// <summary>
    /// Добавляет информацию о выполнении теста в хранилище завершенных тестов.
    /// </summary>
    /// <param name="info">Информация о выполнении теста.</param>
    public static void AddExecution(TestExecutionInfo info)
    {
        _executions.Add(info);
    }
    
    /// <summary>
    /// Возвращает все завершенные тесты из хранилища.
    /// </summary>
    /// <returns>Коллекция информации о выполнении всех завершенных тестов.</returns>
    public static IEnumerable<TestExecutionInfo> GetExecutions()
    {
        return _executions;
    }
    
    /// <summary>
    /// Очищает хранилище, удаляя всю информацию о завершенных и незавершенных тестах.
    /// </summary>
    public static void Clear()
    {
        _executions.Clear();
        _pendingExecutions.Clear();
    }
}
