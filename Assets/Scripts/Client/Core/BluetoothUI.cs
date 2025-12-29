using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BluetoothUI : MonoBehaviour
{
    public static BluetoothUI Instance;
    
    // UI Elements - tạo runtime
    private GameObject overlay;
    private GameObject menuPanel, waitingPanel, devicePanel;
    private Text txtStatus, txtScanning;
    private Transform deviceContainer;
    private List<DeviceInfo> devices = new List<DeviceInfo>();
    private bool isCancelling;
    
    void Awake() => Instance = this;
    
    void Start() => CreateUI();
    
    void CreateUI()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        
        // === OVERLAY (full screen, dark, blocks clicks) ===
        overlay = new GameObject("BT_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        var oRect = overlay.GetComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero;
        oRect.anchorMax = Vector2.one;
        oRect.offsetMin = oRect.offsetMax = Vector2.zero;
        var oImg = overlay.GetComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.95f);
        oImg.raycastTarget = true; // Block clicks to elements behind
        overlay.transform.SetAsLastSibling();
        
        // === MENU PANEL ===
        menuPanel = CreateCenterPanel(overlay.transform, "MenuPanel");
        CreateText(menuPanel.transform, "BLUETOOTH", 42, 160, Color.white);
        CreateButton(menuPanel.transform, "📡  TẠO PHÒNG", 50, new Color(0.2f, 0.55f, 0.2f), OnCreateRoom);
        CreateButton(menuPanel.transform, "🔍  TÌM PHÒNG", -30, new Color(0.2f, 0.45f, 0.7f), OnJoinRoom);
        CreateButton(menuPanel.transform, "✕  ĐÓNG", -120, new Color(0.5f, 0.3f, 0.3f), Hide);
        
        // === WAITING PANEL ===
        waitingPanel = CreateCenterPanel(overlay.transform, "WaitingPanel");
        txtStatus = CreateText(waitingPanel.transform, "Đang chờ...", 32, 80, Color.white);
        CreateText(waitingPanel.transform, "Tên phòng: OQuanGame\nNgười chơi khác chọn 'Tìm phòng'\nđể kết nối với bạn", 22, 0, new Color(0.8f, 0.8f, 0.6f));
        CreateButton(waitingPanel.transform, "❌  HỦY", -100, new Color(0.6f, 0.25f, 0.25f), OnCancelWait);
        waitingPanel.SetActive(false);
        
        // === DEVICE LIST PANEL ===
        devicePanel = CreateCenterPanel(overlay.transform, "DevicePanel");
        CreateText(devicePanel.transform, "TÌM PHÒNG", 36, 160, Color.white);
        txtScanning = CreateText(devicePanel.transform, "Đang quét...", 24, 110, Color.yellow);
        CreateDeviceList(devicePanel.transform);
        CreateButton(devicePanel.transform, "← QUAY LẠI", -150, new Color(0.45f, 0.35f, 0.35f), ShowMenu);
        devicePanel.SetActive(false);
        
        overlay.SetActive(false);
    }
    
    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }
    
    GameObject CreateCenterPanel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500, 400);
        return go;
    }
    
    Text CreateText(Transform parent, string content, int size, float y, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(450, 100);
        rect.anchoredPosition = new Vector2(0, y);
        
        var txt = go.GetComponent<Text>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return txt;
    }
    
    void CreateButton(Transform parent, string label, float y, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(380, 70);
        rect.anchoredPosition = new Vector2(0, y);
        go.GetComponent<Image>().color = color;
        go.GetComponent<Button>().onClick.AddListener(action);
        
        var txt = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(go.transform, false);
        var tRect = txt.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = tRect.offsetMax = Vector2.zero;
        var t = txt.GetComponent<Text>();
        t.text = label;
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    void CreateDeviceList(Transform parent)
    {
        var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scroll.transform.SetParent(parent, false);
        var sRect = scroll.GetComponent<RectTransform>();
        sRect.anchorMin = sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.sizeDelta = new Vector2(420, 200);
        sRect.anchoredPosition = new Vector2(0, -20);
        scroll.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scroll.transform, false);
        var vRect = viewport.GetComponent<RectTransform>();
        vRect.anchorMin = Vector2.zero;
        vRect.anchorMax = Vector2.one;
        vRect.offsetMin = vRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        viewport.GetComponent<Image>().color = Color.white;
        
        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = Vector2.one;
        cRect.pivot = new Vector2(0.5f, 1);
        cRect.offsetMin = cRect.offsetMax = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        deviceContainer = content.transform;
        var sr = scroll.GetComponent<ScrollRect>();
        sr.viewport = vRect;
        sr.content = cRect;
    }
    
    // === PUBLIC METHODS ===
    public void Show()
    {
        if (overlay == null) CreateUI();
        overlay?.SetActive(true);
        ShowMenu();
    }
    
    public void Hide()
    {
        overlay?.SetActive(false);
    }
    
    void ShowMenu()
    {
        menuPanel?.SetActive(true);
        waitingPanel?.SetActive(false);
        devicePanel?.SetActive(false);
    }
    
    void ShowWaiting(string status)
    {
        menuPanel?.SetActive(false);
        waitingPanel?.SetActive(true);
        devicePanel?.SetActive(false);
        if (txtStatus) txtStatus.text = status;
    }
    
    void ShowDeviceList()
    {
        menuPanel?.SetActive(false);
        waitingPanel?.SetActive(false);
        devicePanel?.SetActive(true);
        if (txtScanning) txtScanning.text = "Đang quét...";
        ClearDevices();
    }
    
    // === BUTTON HANDLERS ===
    void OnCreateRoom()
    {
        Debug.Log("🔵 Create room");
        BluetoothGameManager.Instance?.CreateGame();
        ShowWaiting("Đang chờ người chơi...");
    }
    
    void OnJoinRoom()
    {
        Debug.Log("🔵 Join room");
        BluetoothGameManager.Instance?.JoinGame();
        ShowDeviceList();
    }
    
    void OnCancelWait()
    {
        Debug.Log("🔵 Cancel");
        isCancelling = true;
        BluetoothGameManager.Instance?.Disconnect();
        ShowMenu();
        // Keep isCancelling true for 1 second to ignore async callbacks
        Invoke(nameof(ResetCancelling), 1f);
    }
    
    void ResetCancelling() => isCancelling = false;
    
    // === DEVICE LIST ===
    void ClearDevices()
    {
        devices.Clear();
        if (deviceContainer != null)
            foreach (Transform c in deviceContainer) Destroy(c.gameObject);
    }
    
    public void AddDevice(string name, string address)
    {
        // Filter: chỉ hiện OQuan hoặc Paired devices
        bool isOQuan = name.Contains("OQuan");
        bool isPaired = name.Contains("Paired");
        if (!isOQuan && !isPaired) return;
        if (devices.Exists(d => d.address == address)) return;
        
        devices.Add(new DeviceInfo { name = name, address = address });
        if (txtScanning) txtScanning.text = $"Tìm thấy {devices.Count} phòng";
        
        // Create device button
        var go = new GameObject("Device", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(deviceContainer, false);
        go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.3f);
        go.GetComponent<LayoutElement>().preferredHeight = 60;
        string addr = address;
        go.GetComponent<Button>().onClick.AddListener(() => ConnectTo(name, addr));
        
        var txt = new GameObject("T", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(go.transform, false);
        var r = txt.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        var t = txt.GetComponent<Text>();
        t.text = name;
        t.fontSize = 24;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
    
    void ConnectTo(string name, string address)
    {
        Debug.Log($"🔵 Connect to {name}");
        BluetoothGameManager.Instance?.ConnectToDevice(address);
        ShowWaiting("Đang kết nối...\n\nNếu có popup ghép đôi,\nhãy chấp nhận trên CẢ 2 thiết bị");
    }
    
    // === CALLBACKS ===
    public void OnConnected()
    {
        Debug.Log("🔵 Connected!");
        Hide();
    }
    
    public void OnDisconnected()
    {
        if (!isCancelling) ShowWaiting("Mất kết nối!");
    }
    
    public void OnScanFinished()
    {
        if (txtScanning) txtScanning.text = devices.Count > 0 ? $"Tìm thấy {devices.Count} phòng" : "Không tìm thấy phòng";
    }
}
