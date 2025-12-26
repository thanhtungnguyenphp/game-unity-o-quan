using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Performance Monitor - Tracks FPS, memory usage, and performance metrics
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    public static PerformanceMonitor Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private float updateInterval = 0.5f;

    // FPS Tracking
    private float _fps = 0f;
    private float _minFps = float.MaxValue;
    private float _maxFps = 0f;
    private int _frameCount = 0;
    private float _deltaTime = 0f;
    private float _lastUpdateTime = 0f;

    // Memory Tracking
    private long _currentMemory = 0;
    private long _peakMemory = 0;
    private long _lastGCMemory = 0;

    // Performance History
    private Queue<float> _fpsHistory = new Queue<float>();
    private const int MAX_HISTORY = 100;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Toggle display
        if (Input.GetKeyDown(toggleKey))
        {
            showOnScreen = !showOnScreen;
        }

        // Update FPS
        _frameCount++;
        _deltaTime += Time.unscaledDeltaTime;

        if (Time.realtimeSinceStartup - _lastUpdateTime >= updateInterval)
        {
            _fps = _frameCount / _deltaTime;
            _minFps = Mathf.Min(_minFps, _fps);
            _maxFps = Mathf.Max(_maxFps, _fps);

            // Add to history
            _fpsHistory.Enqueue(_fps);
            if (_fpsHistory.Count > MAX_HISTORY)
                _fpsHistory.Dequeue();

            // Update memory
            UpdateMemoryStats();

            // Reset counters
            _frameCount = 0;
            _deltaTime = 0f;
            _lastUpdateTime = Time.realtimeSinceStartup;
        }
    }

    void UpdateMemoryStats()
    {
        // Get current memory usage
        _currentMemory = System.GC.GetTotalMemory(false);
        _peakMemory = System.Math.Max(_peakMemory, _currentMemory);

        // Check for GC
        long afterGC = System.GC.GetTotalMemory(true);
        if (afterGC < _lastGCMemory)
        {
            long freed = _lastGCMemory - afterGC;
            Debug.Log($"🗑️ GC: Freed {FormatBytes(freed)}");
        }
        _lastGCMemory = afterGC;
    }

    void OnGUI()
    {
        if (!showOnScreen) return;

        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = GetFPSColor(_fps);

        // Create background box
        Rect rect = new Rect(10, 10, 250, 180);
        GUI.Box(rect, "");

        // Display stats
        int yOffset = 15;
        int lineHeight = 25;

        GUI.Label(new Rect(15, yOffset, 240, 20), $"FPS: {_fps:F1}", style);
        yOffset += lineHeight;

        style.normal.textColor = Color.white;
        GUI.Label(new Rect(15, yOffset, 240, 20), $"Min: {_minFps:F1} | Max: {_maxFps:F1}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(15, yOffset, 240, 20), $"Avg: {GetAverageFPS():F1}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(15, yOffset, 240, 20), $"Memory: {FormatBytes(_currentMemory)}", style);
        yOffset += lineHeight;

        GUI.Label(new Rect(15, yOffset, 240, 20), $"Peak: {FormatBytes(_peakMemory)}", style);
        yOffset += lineHeight;

        style.fontSize = h * 1 / 80;
        GUI.Label(new Rect(15, yOffset, 240, 20), $"Press {toggleKey} to toggle", style);
    }

    #region Public Methods

    public float GetFPS() => _fps;
    public float GetMinFPS() => _minFps;
    public float GetMaxFPS() => _maxFps;
    public float GetAverageFPS()
    {
        if (_fpsHistory.Count == 0) return 0f;

        float sum = 0f;
        foreach (float fps in _fpsHistory)
        {
            sum += fps;
        }
        return sum / _fpsHistory.Count;
    }

    public long GetCurrentMemory() => _currentMemory;
    public long GetPeakMemory() => _peakMemory;

    public void ResetStats()
    {
        _minFps = float.MaxValue;
        _maxFps = 0f;
        _peakMemory = 0;
        _fpsHistory.Clear();
        Debug.Log("🔄 Performance stats reset");
    }

    public string GetPerformanceReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Performance Report ===");
        sb.AppendLine($"Current FPS: {_fps:F1}");
        sb.AppendLine($"Min FPS: {_minFps:F1}");
        sb.AppendLine($"Max FPS: {_maxFps:F1}");
        sb.AppendLine($"Avg FPS: {GetAverageFPS():F1}");
        sb.AppendLine($"Current Memory: {FormatBytes(_currentMemory)}");
        sb.AppendLine($"Peak Memory: {FormatBytes(_peakMemory)}");
        sb.AppendLine($"Target FPS: {Application.targetFrameRate}");
        sb.AppendLine($"VSync: {QualitySettings.vSyncCount}");
        return sb.ToString();
    }

    public void LogPerformanceReport()
    {
        Debug.Log(GetPerformanceReport());
    }

    #endregion

    #region Helper Methods

    private Color GetFPSColor(float fps)
    {
        if (fps >= 55f) return Color.green;
        if (fps >= 30f) return Color.yellow;
        return Color.red;
    }

    private string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024f * 1024f):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024f:F2} KB";
        return $"{bytes} B";
    }

    #endregion
}
