using System.Text.Json;

namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Экспортер данных о выполнении тестов в JSON формат.
/// </summary>
public static class JsonExporter
{
    /// <summary>
    /// Экспортирует данные о выполнении тестов в JSON файл.
    /// </summary>
    /// <param name="executions">Коллекция информации о выполнении тестов.</param>
    /// <param name="filePath">Путь к файлу для сохранения JSON.</param>
    public static void Export(IEnumerable<TestExecutionInfo> executions, string filePath)
    {
        var data = executions.OrderBy(e => e.StartTime).Select(e => new
        {
            testName = e.TestName,
            workerId = e.WorkerId,
            startTime = e.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            endTime = e.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            durationMs = e.Duration.TotalMilliseconds,
            status = e.Status
        }).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(filePath, json);
    }
}
