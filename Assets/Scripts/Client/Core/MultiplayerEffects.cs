using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple particle effects for multiplayer celebrations
/// </summary>
public class MultiplayerEffects : MonoBehaviour
{
    public static MultiplayerEffects Instance;
    
    private GameObject[] confettiPieces;
    private const int CONFETTI_COUNT = 20;
    
    void Awake()
    {
        Instance = this;
        CreateConfetti();
    }
    
    void CreateConfetti()
    {
        confettiPieces = new GameObject[CONFETTI_COUNT];
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        Color[] colors = { Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta };
        
        for (int i = 0; i < CONFETTI_COUNT; i++)
        {
            var go = new GameObject($"Confetti_{i}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(Random.Range(8, 15), Random.Range(8, 15));
            
            var img = go.GetComponent<Image>();
            img.color = colors[i % colors.Length];
            
            go.SetActive(false);
            confettiPieces[i] = go;
        }
    }
    
    public void PlayConfetti(Vector2 position)
    {
        StartCoroutine(DoConfetti(position));
    }
    
    public void PlayWinCelebration()
    {
        StartCoroutine(DoWinCelebration());
    }
    
    IEnumerator DoConfetti(Vector2 pos)
    {
        foreach (var piece in confettiPieces)
        {
            if (piece == null) continue;
            
            piece.SetActive(true);
            var rect = piece.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            
            StartCoroutine(AnimateConfettiPiece(piece, pos));
        }
        
        yield return new WaitForSeconds(1.5f);
        
        foreach (var piece in confettiPieces)
            piece?.SetActive(false);
    }
    
    IEnumerator AnimateConfettiPiece(GameObject piece, Vector2 startPos)
    {
        var rect = piece.GetComponent<RectTransform>();
        var img = piece.GetComponent<Image>();
        
        Vector2 velocity = new Vector2(Random.Range(-200f, 200f), Random.Range(300f, 500f));
        float rotation = Random.Range(-360f, 360f);
        float gravity = 800f;
        float t = 0;
        
        Color origColor = img.color;
        
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            velocity.y -= gravity * Time.deltaTime;
            
            rect.anchoredPosition += velocity * Time.deltaTime;
            rect.Rotate(0, 0, rotation * Time.deltaTime);
            
            // Fade out
            if (t > 1f)
                img.color = new Color(origColor.r, origColor.g, origColor.b, 1f - (t - 1f) / 0.5f);
            
            yield return null;
        }
        
        img.color = origColor;
    }
    
    IEnumerator DoWinCelebration()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayConfetti(new Vector2(Random.Range(-150, 150), Random.Range(-100, 100)));
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
