using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Analytics Manager - Tracks gameplay events and user behavior
/// Can be integrated with Firebase Analytics, Unity Analytics, or custom backend
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
    private float _sessionStartTime;
    private int _gamesPlayed = 0;
    private int _gamesWon = 0;
    private int _gamesLost = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        _sessionStartTime = Time.realtimeSinceStartup;
    }

    #region Event Tracking

    /// <summary>
    /// Track a custom event
    /// </summary>
    public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        // Increment counter
        if (!_eventCounts.ContainsKey(eventName))
            _eventCounts[eventName] = 0;
        _eventCounts[eventName]++;

        // Log locally
        Debug.Log($"📊 Analytics: {eventName}" + (parameters != null ? $" {FormatParameters(parameters)}" : ""));

        // TODO: Send to Firebase Analytics
        // FirebaseAnalytics.LogEvent(eventName, parameters);
    }

    /// <summary>
    /// Track game start
    /// </summary>
    public void TrackGameStart(GameMode mode, AIDifficulty? aiDifficulty = null)
    {
        var param = new Dictionary<string, object>
        {
            { "game_mode", mode.ToString() },
            { "ai_difficulty", aiDifficulty?.ToString() ?? "None" }
        };
        TrackEvent("game_start", param);
        _gamesPlayed++;
    }

    /// <summary>
    /// Track game end
    /// </summary>
    public void TrackGameEnd(bool won, int finalScore, int opponentScore, float gameDuration)
    {
        if (won) _gamesWon++;
        else _gamesLost++;

        var param = new Dictionary<string, object>
        {
            { "result", won ? "win" : "lose" },
            { "final_score", finalScore },
            { "opponent_score", opponentScore },
            { "duration_seconds", gameDuration },
            { "score_difference", Mathf.Abs(finalScore - opponentScore) }
        };
        TrackEvent("game_end", param);
    }

    /// <summary>
    /// Track achievement unlocked
    /// </summary>
    public void TrackAchievement(string achievementId, string achievementName)
    {
        var param = new Dictionary<string, object>
        {
            { "achievement_id", achievementId },
            { "achievement_name", achievementName }
        };
        TrackEvent("achievement_unlocked", param);
    }

    /// <summary>
    /// Track level up
    /// </summary>
    public void TrackLevelUp(int newLevel, int xpEarned)
    {
        var param = new Dictionary<string, object>
        {
            { "level", newLevel },
            { "xp_earned", xpEarned }
        };
        TrackEvent("level_up", param);
    }

    /// <summary>
    /// Track AI move
    /// </summary>
    public void TrackAIMove(AIDifficulty difficulty, float thinkTime)
    {
        var param = new Dictionary<string, object>
        {
            { "difficulty", difficulty.ToString() },
            { "think_time_ms", thinkTime }
        };
        TrackEvent("ai_move", param);
    }

    /// <summary>
    /// Track error
    /// </summary>
    public void TrackError(string errorType, string errorMessage)
    {
        var param = new Dictionary<string, object>
        {
            { "error_type", errorType },
            { "error_message", errorMessage }
        };
        TrackEvent("error", param);
    }

    /// <summary>
    /// Track feature usage
    /// </summary>
    public void TrackFeatureUsage(string featureName)
    {
        var param = new Dictionary<string, object>
        {
            { "feature", featureName }
        };
        TrackEvent("feature_used", param);
    }

    #endregion

    #region Session Info

    public float GetSessionDuration()
    {
        return Time.realtimeSinceStartup - _sessionStartTime;
    }

    public int GetGamesPlayed() => _gamesPlayed;
    public int GetGamesWon() => _gamesWon;
    public int GetGamesLost() => _gamesLost;
    public float GetWinRate()
    {
        return _gamesPlayed > 0 ? (_gamesWon / (float)_gamesPlayed) * 100f : 0f;
    }

    #endregion

    #region Statistics

    public string GetEventStats()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Analytics Summary ===");
        sb.AppendLine($"Session Duration: {GetSessionDuration():F1}s");
        sb.AppendLine($"Games Played: {_gamesPlayed}");
        sb.AppendLine($"Win/Loss: {_gamesWon}/{_gamesLost} ({GetWinRate():F1}%)");
        sb.AppendLine("\nEvent Counts:");
        
        foreach (var kvp in _eventCounts)
        {
            sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        return sb.ToString();
    }

    public void LogStats()
    {
        Debug.Log(GetEventStats());
    }

    #endregion

    #region Helper Methods

    private string FormatParameters(Dictionary<string, object> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return "";

        var parts = new List<string>();
        foreach (var kvp in parameters)
        {
            parts.Add($"{kvp.Key}={kvp.Value}");
        }
        return "(" + string.Join(", ", parts) + ")";
    }

    #endregion

    void OnApplicationQuit()
    {
        LogStats();
    }
}
