namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Экспортер данных о выполнении тестов в HTML формат с интерактивной визуализацией.
/// </summary>
public static class HtmlExporter
{
    /// <summary>
    /// Экспортирует данные о выполнении тестов в HTML файл с временной диаграммой.
    /// </summary>
    /// <param name="executions">Коллекция информации о выполнении тестов.</param>
    /// <param name="filePath">Путь к файлу для сохранения HTML.</param>
    public static void Export(IEnumerable<TestExecutionInfo> executions, string filePath)
    {
        var executionList = executions.OrderBy(e => e.StartTime).ToList();

        if (!executionList.Any())
        {
            File.WriteAllText(filePath, "<html><body><h1>Нет данных о выполнении тестов</h1></body></html>");
            return;
        }

        var calculator = new TimelineCalculator(executionList);
        var builder = new HtmlTemplateBuilder(calculator);

        BuildHtmlDocument(builder, executionList, calculator);

        File.WriteAllText(filePath, builder.Build());
    }

    /// <summary>
    /// Строит HTML документ с визуализацией.
    /// </summary>
    private static void BuildHtmlDocument(
        HtmlTemplateBuilder builder, 
        List<TestExecutionInfo> executions, 
        TimelineCalculator calculator)
    {
        builder.BeginDocument("Визуализация параллельного выполнения тестов");

        var workers = executions.Select(e => e.WorkerId).Distinct().OrderBy(w => w).ToList();
        var totalDurationSeconds = calculator.TotalDurationMs / 1000;

        builder.AddStatistics(
            executions.Count, 
            workers.Count, 
            totalDurationSeconds, 
            calculator.MinTime, 
            calculator.MaxTime);

        builder.AddZoomControls();

        builder.BeginTimeline();
        builder.AddTimeAxis();

        AddWorkerRows(builder, executions, workers);

        builder.EndTimeline();
        builder.AddTooltipAndScript();
        builder.EndDocument();
    }

    /// <summary>
    /// Добавляет ряды воркеров с тестами.
    /// </summary>
    private static void AddWorkerRows(
        HtmlTemplateBuilder builder, 
        List<TestExecutionInfo> executions, 
        List<string> workers)
    {
        var rowNumber = 1;
        foreach (var workerId in workers)
        {
            var workerTests = executions
                .Where(e => e.WorkerId == workerId)
                .OrderBy(e => e.StartTime)
                .ToList();

            builder.AddWorkerRow(rowNumber, workerId, workerTests);
            rowNumber++;
        }
    }
}
