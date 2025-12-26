using UnityEngine;

// Firebase SDK not installed yet - will be added later
// Uncomment when Firebase SDK is imported

/*
#if UNITY_ANDROID || UNITY_IOS
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
#endif
*/

/// <summary>
/// Manages Firebase initialization and services
/// NOTE: Firebase SDK not installed - this is a placeholder
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    
    private bool _isInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("FirebaseManager: SDK not installed, skipping initialization");
    }

    public void LogEvent(string eventName, params (string key, object value)[] parameters)
    {
        Debug.Log($"FirebaseManager: LogEvent {eventName} (SDK not installed)");
    }

    public void LogGameStart(string mode)
    {
        Debug.Log($"FirebaseManager: Game started - mode: {mode}");
    }

    public void LogGameEnd(string winner, int duration, int p1Score, int p2Score)
    {
        Debug.Log($"FirebaseManager: Game ended - winner: {winner}, duration: {duration}s");
    }
}
