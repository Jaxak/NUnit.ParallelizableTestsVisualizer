namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Содержит информацию о выполнении одного теста.
/// </summary>
public class TestExecutionInfo
{
    /// <summary>
    /// Полное имя теста.
    /// </summary>
    public string TestName { get; set; }
    
    /// <summary>
    /// Идентификатор потока (Worker), на котором выполнялся тест.
    /// </summary>
    public string WorkerId { get; set; }
    
    /// <summary>
    /// Время начала выполнения теста.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Время окончания выполнения теста.
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Длительность выполнения теста (вычисляется как разница между EndTime и StartTime).
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;
    
    /// <summary>
    /// Статус выполнения теста (Passed, Failed, Skipped, Inconclusive).
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Количество перезапусков теста (0 = запущен один раз без повторов).
    /// </summary>
    public int RetryCount { get; set; }
}
