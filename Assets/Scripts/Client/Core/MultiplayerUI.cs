using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Multiplayer UI controller - integrates with MultiplayerScoreUI
/// Handles board rotation for P2 perspective
/// </summary>
public class MultiplayerUI : MonoBehaviour
{
    public static MultiplayerUI Instance { get; private set; }
    
    private bool isInitialized;
    private bool boardRotated;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    void Start()
    {
        if (GameManager.instance?.currentMode == GameMode.Bluetooth)
            Initialize();
    }
    
    public void Initialize()
    {
        if (isInitialized) return;
        isInitialized = true;
        
        SetupBoardPerspective();
        SetupScoreUI();
        UpdateTurnDisplay();
        
        Debug.Log("✅ MultiplayerUI initialized");
    }
    
    void SetupBoardPerspective()
    {
        if (boardRotated) return;
        if (BluetoothGameManager.Instance?.myTurn != PlayerTurn.P2) return;
        
        var board = GameManager.instance?.BoardManager?.transform;
        if (board == null) return;
        
        // Rotate board 180° for P2
        board.localRotation = Quaternion.Euler(0, 0, 180);
        
        // Counter-rotate text so it's readable
        foreach (var txt in board.GetComponentsInChildren<Text>(true))
            txt.transform.localRotation = Quaternion.Euler(0, 0, 180);
        
        boardRotated = true;
        Debug.Log("🔄 Board rotated for P2");
    }
    
    void SetupScoreUI()
    {
        if (MultiplayerScoreUI.Instance == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var go = new GameObject("MultiplayerScoreUI");
                go.transform.SetParent(canvas.transform, false);
                go.AddComponent<MultiplayerScoreUI>();
                
                // Add effects
                var effectsGo = new GameObject("MultiplayerEffects");
                effectsGo.transform.SetParent(canvas.transform, false);
                effectsGo.AddComponent<MultiplayerEffects>();
            }
        }
        MultiplayerScoreUI.Instance?.Initialize();
    }
    
    public void UpdateTurnDisplay()
    {
        if (BluetoothGameManager.Instance == null) return;
        
        bool isMyTurn = GameManager.instance?._currentTurn == BluetoothGameManager.Instance.myTurn;
        MultiplayerScoreUI.Instance?.UpdateTurnDisplay(isMyTurn);
        
        // Play sound on turn change
        if (isMyTurn)
            SoundManager.Instance?.PlaySFX(Config.SFX.CLICK);
    }
    
    public void UpdateScores(int p1Score, int p2Score)
    {
        MultiplayerScoreUI.Instance?.UpdateScores(p1Score, p2Score);
    }
    
    public void ShowGameOver(bool won)
    {
        MultiplayerScoreUI.Instance?.ShowGameOver(won);
    }
    
    public void Hide()
    {
        MultiplayerScoreUI.Instance?.Hide();
    }
    
    public void ResetPerspective()
    {
        if (!boardRotated) return;
        
        var board = GameManager.instance?.BoardManager?.transform;
        if (board == null) return;
        
        board.localRotation = Quaternion.identity;
        foreach (var txt in board.GetComponentsInChildren<Text>(true))
            txt.transform.localRotation = Quaternion.identity;
        
        boardRotated = false;
    }
    
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
