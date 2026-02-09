using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Атрибут для автоматического отслеживания и экспорта информации о выполнении тестов.
/// Применяется на уровне сборки для отслеживания всех тестов в проекте.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class TestExecutionTrackerAttribute : Attribute, ITestAction
{
    private static bool _isInitialized;
    private static readonly object _lock = new object();
    private static string? _assemblyName;
    
    /// <summary>
    /// Путь к директории для сохранения результатов.
    /// </summary>
    public string OutputPath { get; set; } = "TestResults";
    
    /// <summary>
    /// Порог длительности теста в секундах для пометки как "долгий".
    /// </summary>
    public double LongRunningTestThresholdSeconds { get; set; } = HtmlConstants.LongRunningTestThresholdSeconds;
    
    /// <summary>
    /// Название сборки для отображения в отчете (опционально, определяется автоматически).
    /// </summary>
    public string? AssemblyName { get; set; }
    
    /// <summary>
    /// Определяет, к каким элементам применяется действие (только к тестам).
    /// </summary>
    public ActionTargets Targets => ActionTargets.Test;

    /// <summary>
    /// Инициализирует новый экземпляр атрибута TestExecutionTrackerAttribute.
    /// </summary>
    /// <param name="outputPath">Путь к директории для сохранения результатов. По умолчанию "TestResults".</param>
    /// <param name="longTimeSeconds">Порог длительности теста в секундах для пометки как "долгий".</param>
    public TestExecutionTrackerAttribute(string outputPath, double longTimeSeconds = HtmlConstants.LongRunningTestThresholdSeconds)
    {
        OutputPath = outputPath;
        LongRunningTestThresholdSeconds = longTimeSeconds;
    }

    /// <summary>
    /// Инициализирует новый экземпляр атрибута TestExecutionTrackerAttribute с путем по умолчанию.
    /// </summary>
    public TestExecutionTrackerAttribute()
    {
    }

    /// <summary>
    /// Вызывается перед началом выполнения теста. Записывает время начала и создает запись о тесте.
    /// </summary>
    /// <param name="test">Информация о тесте.</param>
    public void BeforeTest(ITest test)
    {
        EnsureInitialized(test);
        
        if (test.IsSuite)
            return;

        var testName = test.FullName;
        var workerId = TestContext.CurrentContext.WorkerId ?? "0";
        
        var info = new TestExecutionInfo
        {
            TestName = testName,
            WorkerId = workerId,
            StartTime = DateTime.Now
        };
        
        TestExecutionStorage.AddPendingExecution(testName, info);
    }

    /// <summary>
    /// Вызывается после завершения выполнения теста. Записывает время окончания и статус теста.
    /// </summary>
    /// <param name="test">Информация о тесте.</param>
    public void AfterTest(ITest test)
    {
        if (test.IsSuite)
            return;

        var testName = test.FullName;
        var workerId = TestContext.CurrentContext.WorkerId ?? "0";
        var now = DateTime.Now;
        var status = TestContext.CurrentContext.Result.Outcome.Status.ToString();
        
        if (TestExecutionStorage.TryGetPendingExecution(testName, out var info))
        {
            // Тест был найден в pending executions - обновляем его данные
            info.EndTime = now;
            info.Status = status;
            TestExecutionStorage.CompletePendingExecution(testName);
        }
        else
        {
            // Тест НЕ был найден в pending executions
            // Это означает, что BeforeTest не был вызван (например, для игнорируемых тестов)
            // Создаем новую запись с нулевой длительностью
            var newInfo = new TestExecutionInfo
            {
                TestName = testName,
                WorkerId = workerId,
                StartTime = now,
                EndTime = now,
                Status = status
            };
            
            TestExecutionStorage.AddExecution(newInfo);
        }
    }

    /// <summary>
    /// Гарантирует однократную инициализацию трекера и регистрацию обработчиков экспорта результатов.
    /// </summary>
    private void EnsureInitialized(ITest test)
    {
        if (_isInitialized)
            return;

        lock (_lock)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            TestExecutionStorage.Clear();
            
            // Определяем имя сборки если оно не задано явно
            if (string.IsNullOrWhiteSpace(_assemblyName))
            {
                _assemblyName = AssemblyName ?? TryGetAssemblyName(test);
            }
            
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => ExportResults();
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => ExportResults();
        }
    }

    /// <summary>
    /// Пытается определить имя тестовой сборки.
    /// </summary>
    private static string? TryGetAssemblyName(ITest test)
    {
        try
        {
            // Пытаемся получить имя сборки из типа теста
            if (test.TypeInfo?.Type != null)
            {
                return test.TypeInfo.Type.Assembly.GetName().Name;
            }
            
            // Если не удалось, пытаемся получить из полного имени теста
            if (!string.IsNullOrEmpty(test.FullName))
            {
                var parts = test.FullName.Split('.');
                if (parts.Length > 0)
                {
                    return parts[0];
                }
            }
        }
        catch
        {
            // Игнорируем ошибки
        }
        
        return null;
    }

    /// <summary>
    /// Экспортирует собранные данные о выполнении тестов в JSON и HTML форматы.
    /// </summary>
    private void ExportResults()
    {
        var executions = TestExecutionStorage.GetExecutions().ToList();
        
        if (!executions.Any())
            return;

        try
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var jsonPath = Path.Combine(OutputPath, $"test-execution-{timestamp}.json");
            var htmlPath = Path.Combine(OutputPath, $"test-execution-{timestamp}.html");

            JsonExporter.Export(executions, jsonPath);
            HtmlExporter.Export(executions, htmlPath, _assemblyName, LongRunningTestThresholdSeconds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при экспорте результатов: {ex.Message}");
        }
    }
}
