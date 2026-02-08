using NUnit.ParallelizableTestsVisualizer;

// Включает автоматическое отслеживание и визуализацию выполнения тестов
[assembly: TestExecutionTracker("TestResults")]

// Включает параллельное выполнение тестов на уровне всей сборки
[assembly: NUnit.Framework.Parallelizable(NUnit.Framework.ParallelScope.All)]

// Устанавливает количество параллельных потоков для выполнения тестов
[assembly: NUnit.Framework.LevelOfParallelism(16)]
