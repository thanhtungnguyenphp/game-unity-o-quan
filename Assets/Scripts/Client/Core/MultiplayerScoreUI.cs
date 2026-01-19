using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Professional multiplayer UI with fun effects
/// - Player panels at top/bottom
/// - Clear turn indicator with glow
/// - Score animations and emoji reactions
/// </summary>
public class MultiplayerScoreUI : MonoBehaviour
{
    public static MultiplayerScoreUI Instance;
    
    private GameObject topPanel, bottomPanel;
    private Text myScoreText, oppScoreText, myLabel, oppLabel;
    private Image myPanelBg, oppPanelBg;
    private GameObject turnIndicator;
    private Text turnText;
    private int lastMyScore, lastOppScore;
    private GameObject emojiPopup;
    
    void Awake() => Instance = this;
    
    public void Initialize()
    {
        if (topPanel != null) return;
        
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        CreateOpponentPanel(canvas.transform);
        CreateMyPanel(canvas.transform);
        CreateEmojiPopup(canvas.transform);
    }
    
    void CreateOpponentPanel(Transform parent)
    {
        // Top panel - Opponent
        topPanel = CreatePanel(parent, "OpponentPanel", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(0, 70));
        oppPanelBg = topPanel.GetComponent<Image>();
        oppPanelBg.color = new Color(0.3f, 0.2f, 0.15f, 0.85f);
        
        // Avatar + Name
        oppLabel = CreateText(topPanel.transform, "👤 ĐỐI THỦ", 20, new Vector2(-80, 0), Color.white);
        oppLabel.alignment = TextAnchor.MiddleLeft;
        
        // Score with star
        oppScoreText = CreateText(topPanel.transform, "⭐ 0", 28, new Vector2(100, 0), new Color(1f, 0.85f, 0.4f));
        oppScoreText.alignment = TextAnchor.MiddleRight;
    }
    
    void CreateMyPanel(Transform parent)
    {
        // Bottom panel - Me (larger, more prominent)
        bottomPanel = CreatePanel(parent, "MyPanel", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(0, 90));
        myPanelBg = bottomPanel.GetComponent<Image>();
        myPanelBg.color = new Color(0.15f, 0.4f, 0.25f, 0.9f);
        
        // Avatar + Name
        myLabel = CreateText(bottomPanel.transform, "🎯 BẠN", 22, new Vector2(-80, 15), new Color(0.6f, 1f, 0.7f));
        myLabel.alignment = TextAnchor.MiddleLeft;
        
        // Turn indicator inside my panel
        turnIndicator = new GameObject("TurnIndicator", typeof(RectTransform), typeof(Image));
        turnIndicator.transform.SetParent(bottomPanel.transform, false);
        var tiRect = turnIndicator.GetComponent<RectTransform>();
        tiRect.anchorMin = tiRect.anchorMax = new Vector2(0.5f, 0.5f);
        tiRect.sizeDelta = new Vector2(180, 32);
        tiRect.anchoredPosition = new Vector2(0, -15);
        turnIndicator.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.3f, 0.9f);
        
        turnText = CreateText(turnIndicator.transform, "👆 LƯỢT CỦA BẠN", 16, Vector2.zero, Color.white);
        turnIndicator.SetActive(false);
        
        // Score with star (larger)
        myScoreText = CreateText(bottomPanel.transform, "⭐ 0", 36, new Vector2(100, 0), new Color(1f, 0.9f, 0.3f));
        myScoreText.alignment = TextAnchor.MiddleRight;
    }
    
    void CreateEmojiPopup(Transform parent)
    {
        emojiPopup = new GameObject("EmojiPopup", typeof(RectTransform), typeof(Text));
        emojiPopup.transform.SetParent(parent, false);
        var rect = emojiPopup.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100, 100);
        
        var txt = emojiPopup.GetComponent<Text>();
        txt.fontSize = 60;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        emojiPopup.SetActive(false);
    }
    
    GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        return go;
    }
    
    Text CreateText(Transform parent, string content, int size, Vector2 pos, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200, 50);
        rect.anchoredPosition = pos;
        
        var txt = go.GetComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return txt;
    }
    
    public void UpdateScores(int p1Score, int p2Score)
    {
        if (myScoreText == null) return;
        
        bool isP1 = BluetoothGameManager.Instance?.myTurn == PlayerTurn.P1;
        int myScore = isP1 ? p1Score : p2Score;
        int oppScore = isP1 ? p2Score : p1Score;
        
        // Animate my score
        if (myScore != lastMyScore)
        {
            int gained = myScore - lastMyScore;
            myScoreText.text = $"⭐ {myScore}";
            
            if (gained > 0)
            {
                StartCoroutine(BounceScore(myScoreText));
                if (gained >= 5)
                {
                    ShowEmoji("🎉");
                    MultiplayerEffects.Instance?.PlayConfetti(Vector2.zero);
                }
                else if (gained >= 3) ShowEmoji("😄");
            }
            lastMyScore = myScore;
        }
        
        // Update opponent score
        if (oppScore != lastOppScore)
        {
            int gained = oppScore - lastOppScore;
            oppScoreText.text = $"⭐ {oppScore}";
            
            if (gained > 0)
                StartCoroutine(BounceScore(oppScoreText));
            if (gained >= 5) ShowEmoji("😰");
            
            lastOppScore = oppScore;
        }
    }
    
    public void UpdateTurnDisplay(bool isMyTurn)
    {
        if (turnIndicator == null) return;
        
        turnIndicator.SetActive(isMyTurn);
        
        if (isMyTurn)
        {
            turnText.text = "👆 LƯỢT CỦA BẠN";
            myPanelBg.color = new Color(0.15f, 0.5f, 0.3f, 0.95f);
            oppPanelBg.color = new Color(0.25f, 0.2f, 0.15f, 0.6f);
            StartCoroutine(PulseGlow(myPanelBg));
        }
        else
        {
            myPanelBg.color = new Color(0.2f, 0.25f, 0.2f, 0.7f);
            oppPanelBg.color = new Color(0.4f, 0.3f, 0.2f, 0.9f);
        }
    }
    
    IEnumerator BounceScore(Text txt)
    {
        Vector3 orig = txt.transform.localScale;
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float scale = 1f + Mathf.Sin(t * Mathf.PI / 0.3f) * 0.3f;
            txt.transform.localScale = orig * scale;
            yield return null;
        }
        txt.transform.localScale = orig;
    }
    
    IEnumerator PulseGlow(Image img)
    {
        Color orig = img.color;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4) * 0.1f;
            img.color = new Color(orig.r * pulse, orig.g * pulse, orig.b, orig.a);
            yield return null;
        }
        img.color = orig;
    }
    
    void ShowEmoji(string emoji)
    {
        if (emojiPopup == null) return;
        StartCoroutine(DoShowEmoji(emoji));
    }
    
    IEnumerator DoShowEmoji(string emoji)
    {
        var txt = emojiPopup.GetComponent<Text>();
        txt.text = emoji;
        emojiPopup.SetActive(true);
        emojiPopup.transform.localScale = Vector3.zero;
        
        // Pop in
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Sin(t / 0.2f * Mathf.PI * 0.5f) * 1.2f;
            emojiPopup.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Fade out and float up
        var rect = emojiPopup.GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = startPos + Vector2.up * (t / 0.3f * 50);
            txt.color = new Color(1, 1, 1, 1 - t / 0.3f);
            yield return null;
        }
        
        emojiPopup.SetActive(false);
        rect.anchoredPosition = Vector2.zero;
        txt.color = Color.white;
    }
    
    public void ShowGameOver(bool won)
    {
        StopAllCoroutines();
        
        if (won)
        {
            myLabel.text = "🏆 THẮNG!";
            myPanelBg.color = new Color(0.8f, 0.6f, 0.1f, 0.95f);
            ShowEmoji("🎉");
            MultiplayerEffects.Instance?.PlayWinCelebration();
        }
        else
        {
            myLabel.text = "😔 THUA";
            myPanelBg.color = new Color(0.4f, 0.2f, 0.2f, 0.9f);
        }
        
        turnIndicator?.SetActive(false);
    }
    
    public void Reset()
    {
        lastMyScore = lastOppScore = 0;
        if (myScoreText != null) myScoreText.text = "⭐ 0";
        if (oppScoreText != null) oppScoreText.text = "⭐ 0";
        if (myLabel != null) myLabel.text = "🎯 BẠN";
    }
    
    public void Show()
    {
        topPanel?.SetActive(true);
        bottomPanel?.SetActive(true);
    }
    
    public void Hide()
    {
        topPanel?.SetActive(false);
        bottomPanel?.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
