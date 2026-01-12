using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Score panel for Bluetooth multiplayer - "My score" always at bottom
/// </summary>
public class MultiplayerScoreUI : MonoBehaviour
{
    public static MultiplayerScoreUI Instance;
    
    private GameObject panel;
    private Text myScoreText, opponentScoreText;
    private Image myScoreBg, opponentScoreBg;
    private int lastMyScore, lastOpponentScore;
    
    void Awake() => Instance = this;
    
    public void Initialize()
    {
        if (panel != null) return;
        
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Main panel at bottom
        panel = new GameObject("MultiplayerScorePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 100);
        
        panel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
        
        // My Score (left, larger - always YOUR score)
        myScoreBg = CreateScoreBox(panel.transform, "MyScore", -100, out var myLabel, out myScoreText);
        myLabel.text = "🎯 BẠN";
        myLabel.color = new Color(0.4f, 0.9f, 0.5f);
        myScoreText.fontSize = 42;
        myScoreBg.color = new Color(0.15f, 0.35f, 0.2f, 0.8f);
        
        // VS
        CreateText(panel.transform, "VS", 22, 0, 0, new Color(0.5f, 0.5f, 0.5f));
        
        // Opponent Score (right, smaller)
        opponentScoreBg = CreateScoreBox(panel.transform, "OpponentScore", 100, out var oppLabel, out opponentScoreText);
        oppLabel.text = "👤 ĐỐI THỦ";
        oppLabel.color = new Color(0.7f, 0.7f, 0.7f);
        opponentScoreText.fontSize = 32;
        opponentScoreBg.color = new Color(0.2f, 0.2f, 0.25f, 0.6f);
    }
    
    Image CreateScoreBox(Transform parent, string name, float x, out Text label, out Text score)
    {
        var box = new GameObject(name, typeof(RectTransform), typeof(Image));
        box.transform.SetParent(parent, false);
        
        var rect = box.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, 0);
        rect.sizeDelta = new Vector2(140, 80);
        
        var img = box.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.7f);
        
        label = CreateText(box.transform, "", 16, 0, 28, Color.white);
        score = CreateText(box.transform, "0", 42, 0, -8, Color.white);
        
        return img;
    }
    
    Text CreateText(Transform parent, string content, int size, float x, float y, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(140, 50);
        rect.anchoredPosition = new Vector2(x, y);
        
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
        
        // Animate if score changed
        if (myScore != lastMyScore)
        {
            myScoreText.text = myScore.ToString();
            if (myScore > lastMyScore) PulseScore(myScoreText, myScoreBg, true);
            lastMyScore = myScore;
        }
        
        if (oppScore != lastOpponentScore)
        {
            opponentScoreText.text = oppScore.ToString();
            if (oppScore > lastOpponentScore) PulseScore(opponentScoreText, opponentScoreBg, false);
            lastOpponentScore = oppScore;
        }
    }
    
    void PulseScore(Text txt, Image bg, bool isGain)
    {
        StopAllCoroutines();
        StartCoroutine(DoPulse(txt, bg, isGain));
    }
    
    System.Collections.IEnumerator DoPulse(Text txt, Image bg, bool isGain)
    {
        Color origTxt = txt.color;
        Color origBg = bg.color;
        Color highlight = isGain ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.8f, 0.4f, 0.3f);
        
        txt.color = highlight;
        txt.transform.localScale = Vector3.one * 1.2f;
        
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            txt.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t / 0.3f);
            txt.color = Color.Lerp(highlight, origTxt, t / 0.3f);
            yield return null;
        }
        
        txt.transform.localScale = Vector3.one;
        txt.color = origTxt;
    }
    
    public void Show() => panel?.SetActive(true);
    public void Hide() => panel?.SetActive(false);
    
    public void Reset()
    {
        lastMyScore = lastOpponentScore = 0;
        if (myScoreText != null) myScoreText.text = "0";
        if (opponentScoreText != null) opponentScoreText.text = "0";
    }
    
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
