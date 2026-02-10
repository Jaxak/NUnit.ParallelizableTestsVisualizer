using System.Globalization;
using System.Text;

namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Строит HTML разметку для визуализации тестов.
/// </summary>
internal class HtmlTemplateBuilder
{
    private readonly StringBuilder _html = new();
    private readonly TimelineCalculator _calculator;
    private readonly double _longRunningTestThresholdSeconds;

    public HtmlTemplateBuilder(TimelineCalculator calculator, double longRunningTestThresholdSeconds = HtmlConstants.LongRunningTestThresholdSeconds)
    {
        _calculator = calculator;
        _longRunningTestThresholdSeconds = longRunningTestThresholdSeconds;
    }

    /// <summary>
    /// Начинает построение HTML документа.
    /// </summary>
    public void BeginDocument(string title, string? assemblyName = null)
    {
        _html.AppendLine("<!DOCTYPE html>");
        _html.AppendLine("<html>");
        _html.AppendLine("<head>");
        _html.AppendLine("    <meta charset='utf-8'>");
        _html.AppendLine($"    <title>{EscapeHtml(title)}</title>");
        _html.AppendLine("    <style>");
        _html.AppendLine(HtmlConstants.GetCssStyles());
        _html.AppendLine("    </style>");
        _html.AppendLine("</head>");
        _html.AppendLine("<body>");
        
        if (!string.IsNullOrWhiteSpace(assemblyName))
        {
            _html.AppendLine($"    <h1>{EscapeHtml(title)}<br><span style='color: #666; font-weight: normal; font-size: 0.7em;'>для {EscapeHtml(assemblyName)}</span></h1>");
        }
        else
        {
            _html.AppendLine($"    <h1>{EscapeHtml(title)}</h1>");
        }
    }

    /// <summary>
    /// Добавляет секцию статистики.
    /// </summary>
    public void AddStatistics(int totalTests, int workerCount, double totalDurationSeconds, DateTime startTime, DateTime endTime)
    {
        _html.AppendLine("    <div class='stats'>");
        _html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Всего тестов:</span> {totalTests}</div>");
        _html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Потоков:</span> {workerCount}</div>");
        _html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Общее время:</span> {totalDurationSeconds.ToString("F2", CultureInfo.InvariantCulture)} сек</div>");
        _html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Начало:</span> {startTime:HH:mm:ss.fff}</div>");
        _html.AppendLine($"        <div class='stats-item'><span class='stats-label'>Окончание:</span> {endTime:HH:mm:ss.fff}</div>");
        _html.AppendLine("    </div>");
    }

    /// <summary>
    /// Добавляет элементы управления масштабом.
    /// </summary>
    public void AddZoomControls()
    {
        _html.AppendLine("    <div class='zoom-control'>");
        _html.AppendLine("        <label for='zoom-slider'>Масштаб:</label>");
        _html.AppendLine("        <input type='range' id='zoom-slider' class='zoom-slider' min='0.1' max='100' step='0.1' value='1' />");
        _html.AppendLine("        <input type='number' id='zoom-input' class='zoom-input' min='0.1' max='100' step='0.1' value='1.00' />");
        _html.AppendLine("        <span style='color: #666;'>×</span>");
        _html.AppendLine("        <div class='zoom-buttons'>");
        _html.AppendLine("            <button id='zoom-out' class='zoom-button'>−</button>");
        _html.AppendLine("            <button id='zoom-reset' class='zoom-button'>Сброс</button>");
        _html.AppendLine("            <button id='zoom-in' class='zoom-button'>+</button>");
        _html.AppendLine("        </div>");
        _html.AppendLine("        <span style='color: #666; font-size: 12px; margin-left: 10px;'>(Ctrl+колесо мыши для масштабирования)</span>");
        _html.AppendLine("    </div>");
    }

    /// <summary>
    /// Начинает секцию временной шкалы.
    /// </summary>
    public void BeginTimeline()
    {
        _html.AppendLine("    <div class='timeline-container'>");
    }

    /// <summary>
    /// Добавляет временную ось с маркерами.
    /// </summary>
    public void AddTimeAxis()
    {
        _html.AppendLine($"        <div class='time-axis' style='width: {_calculator.TimelineWidthPx}px;' data-original-width='{_calculator.TimelineWidthPx}'>");
        
        foreach (var (time, position) in _calculator.GetTimeMarkers())
        {
            _html.AppendLine($"            <div class='time-marker' style='left: {position}px;' data-original-left='{position}'>{time:HH:mm:ss.fff}</div>");
        }
        
        _html.AppendLine("        </div>");
        _html.AppendLine("        <div class='timeline'>");
    }

    /// <summary>
    /// Добавляет ряд для воркера с тестами.
    /// </summary>
    public void AddWorkerRow(int rowNumber, string workerId, List<TestExecutionInfo> tests)
    {
        _html.AppendLine($"            <div class='worker-row' style='width: {_calculator.TimelineWidthPx}px;'>");
        _html.AppendLine($"                <div class='worker-label'>{rowNumber}</div>");

        foreach (var test in tests)
        {
            AddTestBlock(test);
        }

        _html.AppendLine("            </div>");
    }

    /// <summary>
    /// Добавляет блок теста.
    /// </summary>
    private void AddTestBlock(TestExecutionInfo test)
    {
        var leftPx = _calculator.GetPositionPx(test.StartTime);
        var cleanWidthPx = test.Duration.TotalMilliseconds * HtmlConstants.PixelsPerMillisecond; // Чистая ширина без минимума
        var widthPx = _calculator.GetWidthPx(test.Duration); // С учетом минимума

        var statusClass = GetStatusClass(test);
        var testShortName = GetShortTestName(test.TestName);
        var statusEmoji = GetStatusEmoji(test);
        
        // Определяем, нужно ли отображать текст внутри блока
        // Текст показываем только если при максимальном масштабе блок будет достаточно широким
        var shouldDisplayText = (widthPx * HtmlConstants.MaxZoom) >= HtmlConstants.MinWidthForTextDisplay;
        
        var blockContent = shouldDisplayText
            ? (string.IsNullOrEmpty(statusEmoji) ? EscapeHtml(testShortName) : $"<span style='margin-right: 4px;'>{statusEmoji}</span>{EscapeHtml(testShortName)}")
            : string.Empty;

        _html.AppendLine($"                <div class='test-block {statusClass}' ");
        _html.AppendLine($"                     style='left: {leftPx.ToString("F2", CultureInfo.InvariantCulture)}px; width: {widthPx.ToString("F2", CultureInfo.InvariantCulture)}px;'");
        _html.AppendLine($"                     data-original-left='{leftPx.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-original-width='{widthPx.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-clean-width='{cleanWidthPx.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-test-name='{EscapeHtml(test.TestName)}'");
        _html.AppendLine($"                     data-worker='{test.WorkerId}'");
        _html.AppendLine($"                     data-start='{test.StartTime:HH:mm:ss.fff}'");
        _html.AppendLine($"                     data-end='{test.EndTime:HH:mm:ss.fff}'");
        _html.AppendLine($"                     data-duration='{test.Duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-status='{test.Status}'>");
        _html.AppendLine($"                    {blockContent}");
        _html.AppendLine("                </div>");
    }

    /// <summary>
    /// Определяет CSS класс для статуса теста.
    /// </summary>
    private string GetStatusClass(TestExecutionInfo test)
    {
        if (test.Duration.TotalSeconds > _longRunningTestThresholdSeconds)
        {
            return "long-running";
        }
        
        return test.Status.ToLower();
    }

    /// <summary>
    /// Возвращает эмодзи для статуса теста.
    /// </summary>
    private static string GetStatusEmoji(TestExecutionInfo test)
    {
        // Эмодзи на основе статуса теста
        return test.Status.ToLower() switch
        {
            "passed" => "✅",
            "failed" => "❌",
            "skipped" => "⏭️",
            "inconclusive" => "❓",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Завершает секцию временной шкалы.
    /// </summary>
    public void EndTimeline()
    {
        _html.AppendLine("        </div>");
        _html.AppendLine("    </div>");
    }

    /// <summary>
    /// Добавляет блоки со списками тестов.
    /// </summary>
    public void AddTestListsSection(List<TestExecutionInfo> executions)
    {
        var longRunningTests = executions
            .Where(t => t.Duration.TotalSeconds > _longRunningTestThresholdSeconds)
            .OrderByDescending(t => t.Duration)
            .ToList();

        var failedTests = executions
            .Where(t => t.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.TestName)
            .ToList();

        // Группируем тесты по имени и находим те, у которых было несколько запусков
        var retriedTests = executions
            .GroupBy(t => t.TestName)
            .Where(g => g.Any(t => t.RetryCount > 0))
            .Select(g => new
            {
                TestName = g.Key,
                RetryCount = g.Max(t => t.RetryCount),
                TotalExecutions = g.Count(),
                LastExecution = g.OrderByDescending(t => t.StartTime).First()
            })
            .OrderByDescending(t => t.RetryCount)
            .ThenBy(t => t.TestName)
            .ToList();

        _html.AppendLine("    <div class='test-lists-container'>");
        
        AddLongRunningTestsList(longRunningTests);
        AddFailedTestsList(failedTests);
        AddRetriedTestsList(retriedTests);
        
        _html.AppendLine("    </div>");
    }

    /// <summary>
    /// Добавляет блок со списком долгих тестов.
    /// </summary>
    private void AddLongRunningTestsList(List<TestExecutionInfo> tests)
    {
        _html.AppendLine("        <div class='test-list-block long-running'>");
        _html.AppendLine($"            <h2>⚠️ Долгие тесты ({tests.Count})</h2>");
        
        if (tests.Any())
        {
            _html.AppendLine("            <ul class='test-list'>");
            foreach (var test in tests)
            {
                var durationSeconds = test.Duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture);
                _html.AppendLine("                <li class='long-running'>");
                _html.AppendLine($"                    <span class='test-name'>{EscapeHtml(test.TestName)}</span>");
                _html.AppendLine($"                    <span class='test-duration'>({durationSeconds} сек)</span>");
                _html.AppendLine($"                    <span class='test-worker'>Worker: {EscapeHtml(test.WorkerId)}</span>");
                _html.AppendLine("                </li>");
            }
            _html.AppendLine("            </ul>");
        }
        else
        {
            _html.AppendLine("            <div class='empty-message'>Долгих тестов не обнаружено</div>");
        }
        
        _html.AppendLine("        </div>");
    }

    /// <summary>
    /// Добавляет блок со списком упавших тестов.
    /// </summary>
    private void AddFailedTestsList(List<TestExecutionInfo> tests)
    {
        _html.AppendLine("        <div class='test-list-block failed'>");
        _html.AppendLine($"            <h2>❌ Упавшие тесты ({tests.Count})</h2>");
        
        if (tests.Any())
        {
            _html.AppendLine("            <ul class='test-list'>");
            foreach (var test in tests)
            {
                var durationMs = test.Duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture);
                _html.AppendLine("                <li class='failed'>");
                _html.AppendLine($"                    <span class='test-name'>{EscapeHtml(test.TestName)}</span>");
                _html.AppendLine($"                    <span class='test-duration'>({durationMs} мс)</span>");
                _html.AppendLine($"                    <span class='test-worker'>Worker: {EscapeHtml(test.WorkerId)}</span>");
                _html.AppendLine("                </li>");
            }
            _html.AppendLine("            </ul>");
        }
        else
        {
            _html.AppendLine("            <div class='empty-message'>Упавших тестов не обнаружено</div>");
        }
        
        _html.AppendLine("        </div>");
    }

    /// <summary>
    /// Добавляет блок со списком тестов с перезапусками (Retry).
    /// </summary>
    private void AddRetriedTestsList(dynamic retriedTests)
    {
        var testsList = ((IEnumerable<dynamic>)retriedTests).ToList();
        
        _html.AppendLine("        <div class='test-list-block retried'>");
        _html.AppendLine($"            <h2>🔄 Тесты с перезапусками ({testsList.Count})</h2>");
        
        if (testsList.Any())
        {
            _html.AppendLine("            <ul class='test-list'>");
            foreach (var test in testsList)
            {
                var lastExecution = (TestExecutionInfo)test.LastExecution;
                var durationMs = lastExecution.Duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture);
                var retryCount = (int)test.RetryCount;
                var totalExecutions = (int)test.TotalExecutions;
                
                _html.AppendLine("                <li class='retried'>");
                _html.AppendLine($"                    <span class='test-name'>{EscapeHtml((string)test.TestName)}</span>");
                _html.AppendLine($"                    <span class='test-retry-count'>Перезапусков: {retryCount} (всего запусков: {totalExecutions})</span>");
                _html.AppendLine($"                    <span class='test-duration'>({durationMs} мс последний)</span>");
                _html.AppendLine($"                    <span class='test-worker'>Worker: {EscapeHtml(lastExecution.WorkerId)}</span>");
                _html.AppendLine("                </li>");
            }
            _html.AppendLine("            </ul>");
        }
        else
        {
            _html.AppendLine("            <div class='empty-message'>Тестов с перезапусками не обнаружено</div>");
        }
        
        _html.AppendLine("        </div>");
    }

    /// <summary>
    /// Добавляет всплывающую подсказку и JavaScript.
    /// </summary>
    public void AddTooltipAndScript()
    {
        _html.AppendLine("    <div id='tooltip' class='tooltip'></div>");
        _html.AppendLine("    <script>");
        _html.AppendLine(HtmlConstants.GetJavaScript());
        _html.AppendLine("    </script>");
    }

    /// <summary>
    /// Завершает HTML документ.
    /// </summary>
    public void EndDocument()
    {
        _html.AppendLine("</body>");
        _html.AppendLine("</html>");
    }

    /// <summary>
    /// Возвращает построенный HTML.
    /// </summary>
    public string Build() => _html.ToString();

    /// <summary>
    /// Извлекает короткое имя теста из полного имени (последняя часть после точки).
    /// </summary>
    private static string GetShortTestName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 0 ? parts[^1] : fullName;
    }

    /// <summary>
    /// Экранирует специальные HTML символы в тексте.
    /// </summary>
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
