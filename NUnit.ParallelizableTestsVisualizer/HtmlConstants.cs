namespace NUnit.ParallelizableTestsVisualizer;

/// <summary>
/// Константы для HTML экспорта.
/// </summary>
internal static class HtmlConstants
{
    /// <summary>
    /// Масштаб временной шкалы: количество пикселей на миллисекунду.
    /// </summary>
    public const double PixelsPerMillisecond = 0.1;
    
    /// <summary>
    /// Порог длительности теста в секундах для пометки как "долгий".
    /// </summary>
    public const double LongRunningTestThresholdSeconds = 120;
    
    /// <summary>
    /// Минимальный отступ между соседними тестами в пикселях.
    /// </summary>
    public const double MinTestGapPixels = 1;
    
    /// <summary>
    /// Минимальная ширина блока теста в пикселях.
    /// </summary>
    public const double MinTestBlockWidth = 2;
    
    /// <summary>
    /// Порог разницы во времени между тестами для добавления отступа (в миллисекундах).
    /// </summary>
    public const double TestGapThresholdMs = 0.1;

    /// <summary>
    /// CSS стили для HTML страницы.
    /// </summary>
    public static string GetCssStyles() => @"
        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }
        h1 { color: #333; }
        .timeline-container { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow-x: auto; }
        .timeline { position: relative; margin-top: 20px; min-width: max-content; margin-left: 70px; }
        .worker-row { position: relative; height: 60px; margin-bottom: 10px; border-left: 2px solid #333; }
        .worker-label { position: absolute; left: -60px; top: 20px; width: 50px; text-align: right; font-weight: bold; color: #666; }
        .test-block { position: absolute; height: 40px; top: 10px; border-radius: 4px; cursor: pointer; transition: transform 0.2s; display: flex; align-items: center; padding: 0 8px; font-size: 12px; color: white; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; border: 1px solid black; box-sizing: border-box; }
        .test-block:hover { transform: translateY(-2px); box-shadow: 0 4px 8px rgba(0,0,0,0.2); z-index: 10; }
        .test-block.passed { background: #4CAF50; }
        .test-block.failed { background: #f44336; }
        .test-block.skipped { background: #FF9800; }
        .test-block.inconclusive { background: #9E9E9E; }
        .test-block.long-running { background: #FFD700; color: #333; }
        .tooltip { position: fixed; display: none; background: #333; color: white; padding: 12px; border-radius: 4px; font-size: 14px; z-index: 1000; pointer-events: none; max-width: 400px; box-shadow: 0 4px 8px rgba(0,0,0,0.3); }
        .tooltip-visible { display: block; }
        .time-axis { height: 30px; border-bottom: 2px solid #333; position: relative; margin-bottom: 20px; margin-left: 0; }
        .time-marker { position: absolute; font-size: 11px; color: #666; top: 5px; }
        .stats { margin-bottom: 20px; padding: 15px; background: #e3f2fd; border-radius: 4px; }
        .stats-item { display: inline-block; margin-right: 30px; }
        .stats-label { font-weight: bold; color: #1976d2; }
        .zoom-control { margin-bottom: 20px; padding: 15px; background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); display: flex; align-items: center; gap: 15px; }
        .zoom-control label { font-weight: bold; color: #333; }
        .zoom-slider { flex: 1; max-width: 300px; height: 6px; -webkit-appearance: none; appearance: none; background: #ddd; outline: none; border-radius: 3px; }
        .zoom-slider::-webkit-slider-thumb { -webkit-appearance: none; appearance: none; width: 20px; height: 20px; background: #1976d2; cursor: pointer; border-radius: 50%; }
        .zoom-slider::-moz-range-thumb { width: 20px; height: 20px; background: #1976d2; cursor: pointer; border-radius: 50%; border: none; }
        .zoom-input { width: 80px; padding: 6px 10px; border: 1px solid #ddd; border-radius: 4px; font-size: 14px; text-align: center; }
        .zoom-buttons { display: flex; gap: 10px; }
        .zoom-button { padding: 6px 12px; background: #1976d2; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; transition: background 0.2s; }
        .zoom-button:hover { background: #1565c0; }
        .zoom-button:active { background: #0d47a1; }
        .test-lists-container { margin-top: 30px; display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 20px; }
        .test-list-block { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .test-list-block h2 { margin-top: 0; margin-bottom: 15px; font-size: 18px; color: #333; border-bottom: 2px solid #ddd; padding-bottom: 10px; }
        .test-list-block.long-running h2 { border-color: #FFD700; color: #F57C00; }
        .test-list-block.failed h2 { border-color: #f44336; color: #f44336; }
        .test-list-block.skipped h2 { border-color: #FF9800; color: #FF9800; }
        .test-list-block.retried h2 { border-color: #2196F3; color: #2196F3; }
        .test-list { list-style: none; padding: 0; margin: 0; }
        .test-list li { padding: 10px; margin-bottom: 8px; background: #f9f9f9; border-radius: 4px; border-left: 4px solid #ddd; font-size: 14px; word-break: break-word; }
        .test-list li.long-running { border-left-color: #FFD700; background: #FFFDE7; }
        .test-list li.failed { border-left-color: #f44336; background: #FFEBEE; }
        .test-list li.skipped { border-left-color: #FF9800; background: #FFF3E0; }
        .test-list li.retried { border-left-color: #2196F3; background: #E3F2FD; }
        .test-list .test-name { font-weight: bold; color: #333; }
        .test-list .test-duration { color: #666; font-size: 12px; margin-left: 10px; }
        .test-list .test-worker { color: #888; font-size: 12px; margin-left: 10px; }
        .test-list .test-retry-count { color: #2196F3; font-size: 12px; margin-left: 10px; font-weight: bold; }
        .test-list .empty-message { color: #999; font-style: italic; text-align: center; padding: 20px; }
    ";

    /// <summary>
    /// JavaScript код для интерактивности.
    /// </summary>
    public static string GetJavaScript() => @"
        const tooltip = document.getElementById('tooltip');
        const testBlocks = document.querySelectorAll('.test-block');
        
        // Обработка всплывающих подсказок
        testBlocks.forEach(block => {
            block.addEventListener('mouseenter', (e) => {
                const testName = e.target.dataset.testName;
                const worker = e.target.dataset.worker;
                const start = e.target.dataset.start;
                const end = e.target.dataset.end;
                const duration = e.target.dataset.duration;
                const status = e.target.dataset.status;
                
                tooltip.innerHTML = `
                    <strong>Тест:</strong> ${testName}<br>
                    <strong>Worker:</strong> ${worker}<br>
                    <strong>Начало:</strong> ${start}<br>
                    <strong>Окончание:</strong> ${end}<br>
                    <strong>Длительность:</strong> ${duration} мс<br>
                    <strong>Статус:</strong> ${status}
                `;
                
                tooltip.classList.add('tooltip-visible');
            });
            
            block.addEventListener('mousemove', (e) => {
                tooltip.style.left = (e.clientX + 15) + 'px';
                tooltip.style.top = (e.clientY + 15) + 'px';
            });
            
            block.addEventListener('mouseleave', () => {
                tooltip.classList.remove('tooltip-visible');
            });
        });

        // Управление масштабом
        let currentZoom = 1.0;
        const zoomSlider = document.getElementById('zoom-slider');
        const zoomInput = document.getElementById('zoom-input');
        const timeAxis = document.querySelector('.time-axis');
        const workerRows = document.querySelectorAll('.worker-row');
        const timeMarkers = document.querySelectorAll('.time-marker');

        function applyZoom(zoom) {
            currentZoom = zoom;
            
            // Обновляем значения в контролах
            zoomSlider.value = zoom;
            zoomInput.value = zoom.toFixed(2);
            
            // Обновляем ширину временной оси и рядов
            const originalWidth = parseFloat(timeAxis.dataset.originalWidth);
            const newWidth = originalWidth * zoom;
            timeAxis.style.width = newWidth + 'px';
            workerRows.forEach(row => {
                row.style.width = newWidth + 'px';
            });
            
            // Обновляем позиции временных меток
            timeMarkers.forEach(marker => {
                const originalLeft = parseFloat(marker.dataset.originalLeft);
                marker.style.left = (originalLeft * zoom) + 'px';
            });
            
            // Обновляем позиции и размеры блоков тестов
            testBlocks.forEach(block => {
                const originalLeft = parseFloat(block.dataset.originalLeft);
                const originalWidth = parseFloat(block.dataset.originalWidth);
                block.style.left = (originalLeft * zoom) + 'px';
                block.style.width = (originalWidth * zoom) + 'px';
            });
        }

        // Обработчик изменения ползунка
        zoomSlider.addEventListener('input', (e) => {
            const zoom = parseFloat(e.target.value);
            applyZoom(zoom);
        });

        // Обработчик изменения поля ввода
        zoomInput.addEventListener('change', (e) => {
            let zoom = parseFloat(e.target.value);
            if (isNaN(zoom) || zoom < 0.1) zoom = 0.1;
            if (zoom > 50) zoom = 50;
            applyZoom(zoom);
        });

        // Обработчик Enter в поле ввода
        zoomInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.target.blur();
            }
        });

        // Кнопки быстрого масштабирования
        document.getElementById('zoom-reset').addEventListener('click', () => {
            applyZoom(1.0);
        });

        document.getElementById('zoom-in').addEventListener('click', () => {
            const newZoom = Math.min(50, currentZoom * 1.5);
            applyZoom(newZoom);
        });

        document.getElementById('zoom-out').addEventListener('click', () => {
            const newZoom = Math.max(0.1, currentZoom / 1.5);
            applyZoom(newZoom);
        });

        // Масштабирование колесом мыши
        const timelineContainer = document.querySelector('.timeline-container');
        timelineContainer.addEventListener('wheel', (e) => {
            if (e.ctrlKey) {
                e.preventDefault();
                const delta = e.deltaY > 0 ? 0.9 : 1.1;
                const newZoom = Math.max(0.1, Math.min(50, currentZoom * delta));
                applyZoom(newZoom);
            }
        }, { passive: false });
    ";
}
