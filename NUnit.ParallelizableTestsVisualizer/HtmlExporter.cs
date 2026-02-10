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
    /// <param name="assemblyName">Название сборки с тестами (опционально).</param>
    /// <param name="longRunningTestThresholdSeconds">Порог длительности теста в секундах для пометки как "долгий" (по умолчанию 120).</param>
    public static void Export(IEnumerable<TestExecutionInfo> executions, string filePath, string? assemblyName = null, double longRunningTestThresholdSeconds = HtmlConstants.LongRunningTestThresholdSeconds)
    {
        var executionList = executions.OrderBy(e => e.StartTime).ToList();

        if (!executionList.Any())
        {
            File.WriteAllText(filePath, "<html><body><h1>Нет данных о выполнении тестов</h1></body></html>");
            return;
        }

        var calculator = new TimelineCalculator(executionList);
        var builder = new HtmlTemplateBuilder(calculator, longRunningTestThresholdSeconds);

        BuildHtmlDocument(builder, executionList, calculator, assemblyName);

        File.WriteAllText(filePath, builder.Build());
    }

    /// <summary>
    /// Строит HTML документ с визуализацией.
    /// </summary>
    private static void BuildHtmlDocument(
        HtmlTemplateBuilder builder, 
        List<TestExecutionInfo> executions, 
        TimelineCalculator calculator,
        string? assemblyName)
    {
        builder.BeginDocument("Визуализация параллельного выполнения тестов", assemblyName);

        var workers = executions
            .Select(e => e.WorkerId)
            .Distinct()
            .OrderBy(w => ExtractWorkerNumber(w))
            .ToList();
        
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
        
        builder.AddTestListsSection(executions);
        
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

    /// <summary>
    /// Извлекает номер воркера из его идентификатора (часть после символа #).
    /// </summary>
    private static int ExtractWorkerNumber(string workerId)
    {
        var hashIndex = workerId.LastIndexOf('#');
        if (hashIndex >= 0 && hashIndex < workerId.Length - 1)
        {
            var numberPart = workerId.Substring(hashIndex + 1);
            if (int.TryParse(numberPart, out int workerNumber))
            {
                return workerNumber;
            }
        }
        return int.MaxValue; // Если не удалось извлечь номер, поместить в конец
    }
}
