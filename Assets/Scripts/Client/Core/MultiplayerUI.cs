using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI controller for Bluetooth multiplayer mode
/// - Turn indicator at top
/// - Board perspective rotation for P2
/// - Integrates with MultiplayerScoreUI
/// </summary>
public class MultiplayerUI : MonoBehaviour
{
    public static MultiplayerUI Instance { get; private set; }
    
    private GameObject turnPanel;
    private Text turnText;
    private Image turnBg;
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
        
        CreateTurnIndicator();
        SetupBoardPerspective();
        SetupScoreUI();
        UpdateTurnDisplay();
    }
    
    void CreateTurnIndicator()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        turnPanel = new GameObject("TurnIndicator", typeof(RectTransform), typeof(Image));
        turnPanel.transform.SetParent(canvas.transform, false);
        turnPanel.transform.SetAsLastSibling();
        
        var rect = turnPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -50);
        rect.sizeDelta = new Vector2(280, 55);
        
        turnBg = turnPanel.GetComponent<Image>();
        turnBg.color = new Color(0.2f, 0.6f, 0.3f, 0.95f);
        
        var textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(turnPanel.transform, false);
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        
        turnText = textObj.GetComponent<Text>();
        turnText.text = "Lượt của bạn";
        turnText.fontSize = 26;
        turnText.color = Color.white;
        turnText.alignment = TextAnchor.MiddleCenter;
        turnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    /// <summary>
    /// Rotate board 180° for P2 so their cells are at bottom
    /// </summary>
    void SetupBoardPerspective()
    {
        if (boardRotated) return;
        if (BluetoothGameManager.Instance?.myTurn != PlayerTurn.P2) return;
        
        var board = GameManager.instance?.BoardManager?.transform;
        if (board == null) return;
        
        // Rotate entire board
        board.localRotation = Quaternion.Euler(0, 0, 180);
        
        // Counter-rotate text elements so they remain readable
        foreach (var txt in board.GetComponentsInChildren<Text>(true))
            txt.transform.localRotation = Quaternion.Euler(0, 0, 180);
        
        boardRotated = true;
        Debug.Log("🔄 Board rotated for P2 perspective");
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
            }
        }
        MultiplayerScoreUI.Instance?.Initialize();
    }
    
    public void UpdateTurnDisplay()
    {
        if (!isInitialized || turnText == null) return;
        if (BluetoothGameManager.Instance == null) return;
        
        bool isMyTurn = GameManager.instance?._currentTurn == BluetoothGameManager.Instance.myTurn;
        
        if (isMyTurn)
        {
            turnText.text = "👆 LƯỢT CỦA BẠN";
            turnBg.color = new Color(0.15f, 0.55f, 0.25f, 0.95f);
            StartCoroutine(PulsePanel());
        }
        else
        {
            turnText.text = "⏳ Chờ đối thủ...";
            turnBg.color = new Color(0.4f, 0.35f, 0.25f, 0.85f);
        }
        
        turnPanel?.SetActive(true);
    }
    
    IEnumerator PulsePanel()
    {
        if (turnPanel == null) yield break;
        
        float t = 0;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t * 25f) * 0.04f;
            turnPanel.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        turnPanel.transform.localScale = Vector3.one;
    }
    
    public void UpdateScores(int p1Score, int p2Score)
    {
        MultiplayerScoreUI.Instance?.UpdateScores(p1Score, p2Score);
    }
    
    public void Hide()
    {
        turnPanel?.SetActive(false);
        MultiplayerScoreUI.Instance?.Hide();
    }
    
    public void ShowGameOver(bool won)
    {
        if (turnText == null) return;
        
        StopAllCoroutines();
        
        if (won)
        {
            turnText.text = "🎉 BẠN THẮNG!";
            turnBg.color = new Color(0.75f, 0.6f, 0.1f, 0.95f);
        }
        else
        {
            turnText.text = "😔 BẠN THUA";
            turnBg.color = new Color(0.5f, 0.2f, 0.2f, 0.95f);
        }
        
        turnPanel.transform.localScale = Vector3.one * 1.1f;
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
