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

    public HtmlTemplateBuilder(TimelineCalculator calculator)
    {
        _calculator = calculator;
    }

    /// <summary>
    /// Начинает построение HTML документа.
    /// </summary>
    public void BeginDocument(string title)
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
        _html.AppendLine($"    <h1>{EscapeHtml(title)}</h1>");
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
        _html.AppendLine("        <input type='range' id='zoom-slider' class='zoom-slider' min='0.1' max='20' step='0.1' value='1' />");
        _html.AppendLine("        <input type='number' id='zoom-input' class='zoom-input' min='0.1' max='20' step='0.1' value='1.00' />");
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

        for (int i = 0; i < tests.Count; i++)
        {
            AddTestBlock(tests[i], i > 0 ? tests[i - 1] : null);
        }

        _html.AppendLine("            </div>");
    }

    /// <summary>
    /// Добавляет блок теста.
    /// </summary>
    private void AddTestBlock(TestExecutionInfo test, TestExecutionInfo? previousTest)
    {
        var leftPx = _calculator.GetPositionPx(test.StartTime);
        var widthPx = _calculator.GetWidthPx(test.Duration);

        // Добавляем отступ, если тесты идут вплотную
        if (previousTest != null)
        {
            var prevEndOffset = _calculator.GetPositionPx(previousTest.EndTime);
            var currentStartOffset = leftPx;

            if (Math.Abs(currentStartOffset - prevEndOffset) < HtmlConstants.TestGapThresholdMs)
            {
                leftPx += HtmlConstants.MinTestGapPixels;
                widthPx = Math.Max(HtmlConstants.MinTestBlockWidth, widthPx - HtmlConstants.MinTestGapPixels);
            }
        }

        var statusClass = GetStatusClass(test);
        var testShortName = GetShortTestName(test.TestName);

        _html.AppendLine($"                <div class='test-block {statusClass}' ");
        _html.AppendLine($"                     style='left: {leftPx.ToString("F2", CultureInfo.InvariantCulture)}px; width: {widthPx.ToString("F2", CultureInfo.InvariantCulture)}px;'");
        _html.AppendLine($"                     data-original-left='{leftPx.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-original-width='{widthPx.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-test-name='{EscapeHtml(test.TestName)}'");
        _html.AppendLine($"                     data-worker='{test.WorkerId}'");
        _html.AppendLine($"                     data-start='{test.StartTime:HH:mm:ss.fff}'");
        _html.AppendLine($"                     data-end='{test.EndTime:HH:mm:ss.fff}'");
        _html.AppendLine($"                     data-duration='{test.Duration.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture)}'");
        _html.AppendLine($"                     data-status='{test.Status}'>");
        _html.AppendLine($"                    {EscapeHtml(testShortName)}");
        _html.AppendLine("                </div>");
    }

    /// <summary>
    /// Определяет CSS класс для статуса теста.
    /// </summary>
    private static string GetStatusClass(TestExecutionInfo test)
    {
        if (test.Duration.TotalSeconds > HtmlConstants.LongRunningTestThresholdSeconds)
        {
            return "long-running";
        }
        
        return test.Status.ToLower();
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
