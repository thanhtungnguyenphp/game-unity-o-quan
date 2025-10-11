using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("Private")]
    [SerializeField] Text _p1ScoreText;
    [SerializeField] Text _p2ScoreText;
    [SerializeField] Text _p1OweText;
    [SerializeField] Text _p2OweText;
    [SerializeField] List<Image> _p1Stones = new List<Image>();
    [SerializeField] List<Image> _p2Stones = new List<Image>();

    OulineBlinker _outlineBlinker;
    int _currentPoinP1 = 0;
    int _currentPoinP2 = 0;
    float _durationScaleFX = 0.85f;
    float _scaleFactor = 3f;

    public void Initialize()
    {
        _p1Stones.Clear();
        _p2Stones.Clear();

        Transform p1 = transform.Find("Player1").Find("cell");
        Transform p2 = transform.Find("Player2").Find("cell");

        _outlineBlinker = transform.GetComponent<OulineBlinker>();
        _outlineBlinker.Init(p1, p2);

        _p1ScoreText = p1.Find("score").GetComponent<Text>();
        _p2ScoreText = p2.Find("score").GetComponent<Text>();

        _p1OweText = p1.Find("owe").GetComponent<Text>();
        _p2OweText = p2.Find("owe").GetComponent<Text>();

        foreach (Transform child in p1.Find("item").Find("da"))
        {
            _p1Stones.Add(child.GetComponent<Image>());
        }
        foreach (Transform child in p2.Find("item").Find("da"))
        {
            _p2Stones.Add(child.GetComponent<Image>());
        }

        _p1ScoreText.text = $"{0}";
        _p2ScoreText.text = $"{0}";

    }

    public void UpdateOutline(PlayerTurn turn) => _outlineBlinker.SetTurn(turn);

    public void UpdatePlayer(int p1Score, int p2Score, int p1Da, int p2Da, int p1Owe, int p2Owe)
    {
        print($"P1 Point:{p1Score}|{p1Da} || P2 point:{p2Score}|{p2Da} / c1|c2: {_currentPoinP1}|{_currentPoinP2}");
        if (p1Score != _currentPoinP1)
        {
            print($"P1 Score: c: {_currentPoinP1} -> {p1Score}");
            _p1ScoreText.text = $"{p1Score}";
            if (p1Score > _currentPoinP1)
            {
                print("run VFX P1");
                RunVFX(text: _p1ScoreText, duration: _durationScaleFX, scaleFactor: _scaleFactor, colorHex: "#15ff00fb");
                SoundManager.Instance.PlaySFX(Config.SFX.EAT);
            }
            else
            { }
            _currentPoinP1 = p1Score;
        }
        if (p2Score != _currentPoinP2)
        {
            print($"P2 Score: c: {_currentPoinP2} -> {p2Score}");
            _p2ScoreText.text = $"{p2Score}";
            if (p2Score > _currentPoinP2)
            {
                print("run VFX P2");
                RunVFX(text: _p2ScoreText, duration: _durationScaleFX, scaleFactor: _scaleFactor, colorHex: "#15ff00fb");
                SoundManager.Instance.PlaySFX(Config.SFX.EAT);
            }
            else
            { }
            _currentPoinP2 = p2Score;
        }

        _p1OweText.text = p1Owe > 0 ? $"-{p1Owe}" : string.Empty;
        _p2OweText.text = p2Owe > 0 ? $"-{p2Owe}" : string.Empty;

        for (int i = 0; i < _p1Stones.Count; i++)
        {
            _p1Stones[i].gameObject.SetActive(i < p1Da);
        }
        for (int i = 0; i < _p2Stones.Count; i++)
        {
            _p2Stones[i].gameObject.SetActive(i < p2Da);
        }

    }

    void RunVFX(Text text, float duration, float scaleFactor, string colorHex = null)
    {
        if (text == null)
        {
            Debug.LogError("Text component is null");
            return;
        }
        if (colorHex == null)
            colorHex = "#FFFFFF"; // Default color
        VFXControl._instance.ScaleImage(text, scaleFactor, duration, colorHex);
    }

    public void ResetPlayer()
    {
        _currentPoinP1 = 0;
        _currentPoinP2 = 0;
        _p1ScoreText.text = $"{_currentPoinP1}";
        _p2ScoreText.text = $"{_currentPoinP2}";
        _p1OweText.text = string.Empty;
        _p2OweText.text = string.Empty;

        foreach (var stone in _p1Stones)
            stone.gameObject.SetActive(false);
        foreach (var stone in _p2Stones)
            stone.gameObject.SetActive(false);
    }

}
