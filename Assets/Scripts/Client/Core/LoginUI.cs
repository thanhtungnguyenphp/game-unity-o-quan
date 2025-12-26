using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    Button _btnPlay;
    public Button _btnGuidance;
    public Button _btnBluetooth;
    Guidance _guidance;
    
    public delegate void CallbackPlayNow(bool vsAI);
    public CallbackPlayNow _callbackPlayNow;
    
    // Game mode popup
    private GameObject _modePopup;
    
    public void Init(CallbackPlayNow callbackPlayNow)
    {   
        _guidance = transform.Find("bg/Panel/Instruction")?.GetComponent<Guidance>();
        _callbackPlayNow = callbackPlayNow;

        _btnPlay = transform.Find("bg/play")?.GetComponent<Button>();
        _btnGuidance = transform.Find("bg/guidance")?.GetComponent<Button>();
        _btnBluetooth = transform.Find("bg/bluetooth")?.GetComponent<Button>();

        _btnPlay?.onClick.RemoveAllListeners();
        _btnGuidance?.onClick.RemoveAllListeners();
        _btnBluetooth?.onClick.RemoveAllListeners();

        _btnPlay?.onClick.AddListener(ShowModeSelection);
        _btnGuidance?.onClick.AddListener(ShowGuidance);
        _btnBluetooth?.onClick.AddListener(ShowBluetoothMenu);

        _guidance?.Init();
        CreateModePopup();
    }
    
    void CreateModePopup()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        _modePopup = new GameObject("ModePopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _modePopup.transform.SetParent(canvas.transform, false);
        var rect = _modePopup.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        _modePopup.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        _modePopup.GetComponent<Image>().raycastTarget = true;
        
        // Title
        CreateText(_modePopup.transform, "CHỌN CHẾ ĐỘ CHƠI", 38, 150);
        
        // VS AI button
        CreateButton(_modePopup.transform, "🤖  CHƠI VỚI MÁY (AI)", 50, new Color(0.2f, 0.5f, 0.2f), () => StartGame(true));
        
        // VS Human button  
        CreateButton(_modePopup.transform, "👥  CHƠI 2 NGƯỜI", -30, new Color(0.2f, 0.4f, 0.6f), () => StartGame(false));
        
        // Close button
        CreateButton(_modePopup.transform, "✕  ĐÓNG", -120, new Color(0.5f, 0.3f, 0.3f), HideModePopup);
        
        _modePopup.SetActive(false);
    }
    
    void CreateText(Transform parent, string content, int size, float y)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(450, 60);
        r.anchoredPosition = new Vector2(0, y);
        var t = go.GetComponent<Text>();
        t.text = content;
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    void CreateButton(Transform parent, string label, float y, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(400, 70);
        r.anchoredPosition = new Vector2(0, y);
        go.GetComponent<Image>().color = color;
        go.GetComponent<Button>().onClick.AddListener(action);
        
        var txt = new GameObject("T", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(go.transform, false);
        var tr = txt.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        var t = txt.GetComponent<Text>();
        t.text = label;
        t.fontSize = 26;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SlashScreenControl.instance?.Show(true, SlashScreenControl.instance.Sprites.Length - 1, 1);
    }

    void ShowModeSelection()
    {
        _modePopup?.SetActive(true);
    }
    
    void HideModePopup()
    {
        _modePopup?.SetActive(false);
    }
    
    void StartGame(bool vsAI)
    {
        HideModePopup();
        _callbackPlayNow?.Invoke(vsAI);
        Hide();
    }

    public void ShowGuidance() => _guidance?.Show();
    
    public void ShowBluetoothMenu() => BluetoothUI.Instance?.Show();

    public void Hide() => gameObject.SetActive(false);
}
