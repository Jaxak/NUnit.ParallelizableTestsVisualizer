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
    
    /// <summary>
    /// Путь к директории для сохранения результатов.
    /// </summary>
    public string OutputPath { get; set; } = "TestResults";
    
    /// <summary>
    /// Определяет, к каким элементам применяется действие (только к тестам).
    /// </summary>
    public ActionTargets Targets => ActionTargets.Test;
    
    /// <summary>
    /// Инициализирует новый экземпляр атрибута TestExecutionTrackerAttribute.
    /// </summary>
    /// <param name="outputPath">Путь к директории для сохранения результатов. По умолчанию "TestResults".</param>
    public TestExecutionTrackerAttribute(string outputPath)
    {
        OutputPath = outputPath;
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
        EnsureInitialized();
        
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
        
        if (TestExecutionStorage.TryGetPendingExecution(testName, out var info))
        {
            info.EndTime = DateTime.Now;
            info.Status = TestContext.CurrentContext.Result.Outcome.Status.ToString();
            TestExecutionStorage.CompletePendingExecution(testName);
        }
    }

    /// <summary>
    /// Гарантирует однократную инициализацию трекера и регистрацию обработчиков экспорта результатов.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized)
            return;

        lock (_lock)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            TestExecutionStorage.Clear();
            
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => ExportResults();
            AppDomain.CurrentDomain.DomainUnload += (sender, args) => ExportResults();
        }
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
            HtmlExporter.Export(executions, htmlPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при экспорте результатов: {ex.Message}");
        }
    }
}
