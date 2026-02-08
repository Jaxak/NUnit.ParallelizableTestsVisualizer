using System.Globalization;
using System.Text;

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

        var minTime = executionList.Min(e => e.StartTime);
        var maxTime = executionList.Max(e => e.EndTime);
        var totalDuration = (maxTime - minTime).TotalMilliseconds;
            
        var workers = executionList.Select(e => e.WorkerId).Distinct().OrderBy(w => w).ToList();

        // Масштаб: 10 миллисекунд = 1 пиксель
        const double pixelsPerMs = 0.1;
        var timelineWidth = (int)(totalDuration * pixelsPerMs);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='utf-8'>");
        html.AppendLine("    <title>Визуализация параллельного выполнения тестов</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }");
        html.AppendLine("        h1 { color: #333; }");
        html.AppendLine("        .timeline-container { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow-x: auto; }");
        html.AppendLine("        .timeline { position: relative; margin-top: 20px; min-width: max-content; }");
        html.AppendLine("        .worker-row { position: relative; height: 60px; margin-bottom: 10px; border-left: 2px solid #333; }");
        html.AppendLine("        .worker-label { position: absolute; left: -40px; top: 20px; width: 30px; text-align: right; font-weight: bold; color: #666; }");
        html.AppendLine("        .test-block { position: absolute; height: 40px; top: 10px; border-radius: 4px; cursor: pointer; transition: transform 0.2s; display: flex; align-items: center; padding: 0 8px; font-size: 12px; color: white; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; border: 1px solid black; box-sizing: border-box; }");
        html.AppendLine("        .test-block:hover { transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); z-index: 10; }");
        html.AppendLine("        .test-block.passed { background: #4CAF50; }");
        html.AppendLine("        .test-block.failed { background: #f44336; }");
        html.AppendLine("        .test-block.skipped { background: #FF9800; }");
        html.AppendLine("        .test-block.inconclusive { background: #9E9E9E; }");
        html.AppendLine("        .test-block.long-running { background: #FFD700; color: #333; }");
        html.AppendLine("        .tooltip { position: fixed; display: none; background: #333; color: white; padding: 12px; border-radius: 4px; font-size: 14px; z-index: 1000; pointer-events: none; max-width: 400px; box-shadow: 0 4px 8px rgba(0,0,0,0.3); }");
        html.AppendLine("        .tooltip-visible { display: block; }");
        html.AppendLine("        .time-axis { height: 30px; border-bottom: 2px solid #333; position: relative; margin-bottom: 20px; margin-left: 0; }");
        html.AppendLine("        .time-marker { position: absolute; font-size: 11px; color: #666; top: 5px; }");
        html.AppendLine("        .stats { margin-bottom: 20px; padding: 15px; background: #e3f2fd; border-radius: 4px; }");
        html.AppendLine("        .stats-item { display: inline-block; margin-right: 30px; }");
        html.AppendLine("        .stats-label { font-weight: bold; color: #1976d2; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <h1>Визуализация параллельного выполнения тестов</h1>");
            
        // Статистика
        html.AppendLine("    <div class='stats'>");
        html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Всего тестов:</span> {executionList.Count}</div>");
        html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Потоков:</span> {workers.Count}</div>");
        html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Общее время:</span> {(totalDuration / 1000).ToString("F2", CultureInfo.InvariantCulture)} сек</div>");
        html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Начало:</span> {minTime:HH:mm:ss.fff}</div>");
        html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Окончание:</span> {maxTime:HH:mm:ss.fff}</div>");
        html.AppendLine("    </div>");
            
        html.AppendLine("    <div class='timeline-container'>");
            
        // Временная ось
        html.AppendLine($"        <div class='time-axis' style='width: {timelineWidth}px;'>");
        for (int i = 0; i <= 10; i++)
        {
            var timePoint = minTime.AddMilliseconds(totalDuration * i / 10);
            var leftPx = (int)(totalDuration * i / 10 * pixelsPerMs);
            html.AppendLine($"            <div class='time-marker' style='left: {leftPx}px;'>{timePoint:HH:mm:ss.fff}</div>");
        }
        html.AppendLine("        </div>");
            
        html.AppendLine("        <div class='timeline'>");
            
        var rowNumber = 1;
        foreach (var workerId in workers)
        {
            html.AppendLine($"            <div class='worker-row' style='width: {timelineWidth}px;'>");
            html.AppendLine($"                <div class='worker-label'>{rowNumber}</div>");
                
            // Сортируем тесты по времени начала для каждого воркера
            var workerTests = executionList
                .Where(e => e.WorkerId == workerId)
                .OrderBy(e => e.StartTime)
                .ToList();
                
            for (int i = 0; i < workerTests.Count; i++)
            {
                var test = workerTests[i];
                var startOffset = (test.StartTime - minTime).TotalMilliseconds;
                var leftPx = startOffset * pixelsPerMs;
                var widthPx = Math.Max(2, test.Duration.TotalMilliseconds * pixelsPerMs);
                
                // Проверяем, нужен ли отступ от предыдущего теста
                if (i > 0)
                {
                    var prevTest = workerTests[i - 1];
                    var prevEndOffset = (prevTest.EndTime - minTime).TotalMilliseconds;
                    var currentStartOffset = (test.StartTime - minTime).TotalMilliseconds;
                    
                    // Если тесты идут вплотную (разница меньше 0.1 мс), добавляем 1px отступ
                    if (Math.Abs(currentStartOffset - prevEndOffset) < 0.1)
                    {
                        leftPx += 1;
                        widthPx = Math.Max(2, widthPx - 1);
                    }
                }
                    
                var statusClass = test.Status.ToLower();
                
                // Проверка на длительные тесты (более 2 минут)
                if (test.Duration.TotalSeconds > 120)
                {
                    statusClass = "long-running";
                }
                
                var testShortName = GetShortTestName(test.TestName);
                    
                html.AppendLine($"                <div class='test-block {statusClass}' ");
                html.AppendLine($"                     style='left: {leftPx.ToString("F2", CultureInfo.InvariantCulture)}px; width: {widthPx.ToString("F2", CultureInfo.InvariantCulture)}px;'");
                html.AppendLine($"                     data-test-name='{EscapeHtml(test.TestName)}'");
                html.AppendLine($"                     data-worker='{test.WorkerId}'");
                html.AppendLine($"                     data-start='{test.StartTime:HH:mm:ss.fff}'");
                html.AppendLine($"                     data-end='{test.EndTime:HH:mm:ss.fff}'");
                html.AppendLine($"                     data-duration='{test.Duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture)}'");
                html.AppendLine($"                     data-status='{test.Status}'>");
                html.AppendLine($"                    {EscapeHtml(testShortName)}");
                html.AppendLine("                </div>");
            }
                
            html.AppendLine("            </div>");
            rowNumber++;
        }
            
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
            
        html.AppendLine("    <div id='tooltip' class='tooltip'></div>");
            
        html.AppendLine("    <script>");
        html.AppendLine("        const tooltip = document.getElementById('tooltip');");
        html.AppendLine("        const testBlocks = document.querySelectorAll('.test-block');");
        html.AppendLine("        ");
        html.AppendLine("        testBlocks.forEach(block => {");
        html.AppendLine("            block.addEventListener('mouseenter', (e) => {");
        html.AppendLine("                const testName = e.target.dataset.testName;");
        html.AppendLine("                const worker = e.target.dataset.worker;");
        html.AppendLine("                const start = e.target.dataset.start;");
        html.AppendLine("                const end = e.target.dataset.end;");
        html.AppendLine("                const duration = e.target.dataset.duration;");
        html.AppendLine("                const status = e.target.dataset.status;");
        html.AppendLine("                ");
        html.AppendLine("                tooltip.innerHTML = `");
        html.AppendLine("                    <strong>Тест:</strong> ${testName}<br>");
        html.AppendLine("                    <strong>Worker:</strong> ${worker}<br>");
        html.AppendLine("                    <strong>Начало:</strong> ${start}<br>");
        html.AppendLine("                    <strong>Окончание:</strong> ${end}<br>");
        html.AppendLine("                    <strong>Длительность:</strong> ${duration} мс<br>");
        html.AppendLine("                    <strong>Статус:</strong> ${status}");
        html.AppendLine("                `;");
        html.AppendLine("                ");
        html.AppendLine("                tooltip.classList.add('tooltip-visible');");
        html.AppendLine("            });");
        html.AppendLine("            ");
        html.AppendLine("            block.addEventListener('mousemove', (e) => {");
        html.AppendLine("                tooltip.style.left = (e.clientX + 15) + 'px';");
        html.AppendLine("                tooltip.style.top = (e.clientY + 15) + 'px';");
        html.AppendLine("            });");
        html.AppendLine("            ");
        html.AppendLine("            block.addEventListener('mouseleave', () => {");
        html.AppendLine("                tooltip.classList.remove('tooltip-visible');");
        html.AppendLine("            });");
        html.AppendLine("        });");
        html.AppendLine("    </script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
            
        File.WriteAllText(filePath, html.ToString());
    }

    /// <summary>
    /// Извлекает короткое имя теста из полного имени (последняя часть после точки).
    /// </summary>
    /// <param name="fullName">Полное имя теста.</param>
    /// <returns>Короткое имя теста.</returns>
    private static string GetShortTestName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 0 ? parts[parts.Length - 1] : fullName;
    }

    /// <summary>
    /// Экранирует специальные HTML символы в тексте.
    /// </summary>
    /// <param name="text">Исходный текст.</param>
    /// <returns>Текст с экранированными HTML символами.</returns>
    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
