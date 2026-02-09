namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Вычисляет параметры временной шкалы для визуализации.
/// </summary>
internal class TimelineCalculator
{
    public DateTime MinTime { get; }
    public DateTime MaxTime { get; }
    public double TotalDurationMs { get; }
    public int TimelineWidthPx { get; }

    public TimelineCalculator(IEnumerable<TestExecutionInfo> executions)
    {
        var executionList = executions.ToList();
        
        MinTime = executionList.Min(e => e.StartTime);
        MaxTime = executionList.Max(e => e.EndTime);
        TotalDurationMs = (MaxTime - MinTime).TotalMilliseconds;
        TimelineWidthPx = (int)(TotalDurationMs * HtmlConstants.PixelsPerMillisecond);
    }

    /// <summary>
    /// Вычисляет позицию в пикселях для заданного времени.
    /// </summary>
    public double GetPositionPx(DateTime time)
    {
        var offset = (time - MinTime).TotalMilliseconds;
        return offset * HtmlConstants.PixelsPerMillisecond;
    }

    /// <summary>
    /// Вычисляет ширину в пикселях для заданной длительности.
    /// </summary>
    public double GetWidthPx(TimeSpan duration)
    {
        return Math.Max(HtmlConstants.MinTestBlockWidth, duration.TotalMilliseconds * HtmlConstants.PixelsPerMillisecond);
    }

    /// <summary>
    /// Генерирует временные маркеры для оси времени.
    /// </summary>
    public IEnumerable<(DateTime Time, int PositionPx)> GetTimeMarkers(int markerCount = 10)
    {
        for (int i = 0; i <= markerCount; i++)
        {
            var timePoint = MinTime.AddMilliseconds(TotalDurationMs * i / markerCount);
            var leftPx = (int)(TotalDurationMs * i / markerCount * HtmlConstants.PixelsPerMillisecond);
            yield return (timePoint, leftPx);
        }
    }
}
