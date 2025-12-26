using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Bluetooth UI - Popup menu for Bluetooth multiplayer
/// </summary>
public class BluetoothUI : MonoBehaviour
{
    public static BluetoothUI Instance;
    
    [Header("Main Popup")]
    public GameObject popupPanel;        // Full screen overlay
    public GameObject menuContent;       // Menu buttons container
    public GameObject waitingContent;    // Waiting screen
    public GameObject deviceListContent; // Device list screen
    
    [Header("Menu Buttons")]
    public Button btnCreateRoom;
    public Button btnJoinRoom;
    public Button btnClose;
    
    [Header("Waiting Screen")]
    public Text txtStatus;
    public Text txtRoomName;
    public Button btnCancelWait;
    
    [Header("Device List")]
    public Transform deviceContainer;
    public GameObject deviceItemPrefab;
    public Button btnBackFromList;
    public Text txtScanning;
    
    [Header("Room Settings")]
    public InputField inputRoomName;
    
    private List<DeviceInfo> devices = new List<DeviceInfo>();
    private string roomName = "OQuanGame";
    
    void Awake() => Instance = this;
    
    void Start()
    {
        SetupButtons();
        Hide();
    }
    
    void SetupButtons()
    {
        // Menu buttons
        btnCreateRoom?.onClick.AddListener(OnCreateRoom);
        btnJoinRoom?.onClick.AddListener(OnJoinRoom);
        btnClose?.onClick.AddListener(Hide);
        
        // Waiting buttons
        btnCancelWait?.onClick.AddListener(OnCancelWait);
        
        // Device list buttons
        btnBackFromList?.onClick.AddListener(ShowMenu);
        
        // Room name input
        if (inputRoomName != null)
            inputRoomName.onEndEdit.AddListener(s => roomName = string.IsNullOrEmpty(s) ? "OQuanGame" : s);
    }
    
    public void Show()
    {
        if (popupPanel != null) popupPanel.SetActive(true);
        ShowMenu();
    }
    
    public void Hide()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }
    
    void ShowMenu()
    {
        if (menuContent != null) menuContent.SetActive(true);
        if (waitingContent != null) waitingContent.SetActive(false);
        if (deviceListContent != null) deviceListContent.SetActive(false);
    }
    
    void ShowWaiting(string status, string room = "")
    {
        if (menuContent != null) menuContent.SetActive(false);
        if (waitingContent != null) waitingContent.SetActive(true);
        if (deviceListContent != null) deviceListContent.SetActive(false);
        
        if (txtStatus != null) txtStatus.text = status;
        if (txtRoomName != null) txtRoomName.text = room;
    }
    
    void ShowDeviceList()
    {
        if (menuContent != null) menuContent.SetActive(false);
        if (waitingContent != null) waitingContent.SetActive(false);
        if (deviceListContent != null) deviceListContent.SetActive(true);
        
        if (txtScanning != null) txtScanning.text = "Đang tìm phòng...";
        ClearDevices();
    }
    
    // === Button Handlers ===
    
    void OnCreateRoom()
    {
        Debug.Log($"🔵 Creating room: {roomName}");
        
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.CreateGame();
        }
        
        ShowWaiting("Đang chờ người chơi kết nối...", $"Phòng: {roomName}");
    }
    
    void OnJoinRoom()
    {
        Debug.Log("🔵 Joining room...");
        
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.JoinGame();
        }
        
        ShowDeviceList();
    }
    
    private bool isCancelling = false;
    
    void OnCancelWait()
    {
        Debug.Log("🔵 Cancel waiting");
        isCancelling = true;
        BluetoothGameManager.Instance?.Disconnect();
        ShowMenu();
        isCancelling = false;
    }
    
    // === Device List ===
    
    void ClearDevices()
    {
        devices.Clear();
        if (deviceContainer != null)
            foreach (Transform child in deviceContainer)
                Destroy(child.gameObject);
    }
    
    public void AddDevice(string name, string address)
    {
        // Filter: chỉ hiện thiết bị có tên chứa "OQuan" (phòng game)
        if (!name.Contains("OQuan")) return;
        if (devices.Exists(d => d.address == address)) return;
        
        Debug.Log($"🔵 Found game room: {name}");
        devices.Add(new DeviceInfo { name = name, address = address });
        
        if (txtScanning != null) txtScanning.text = $"Tìm thấy {devices.Count} phòng";
        
        if (deviceItemPrefab != null && deviceContainer != null)
        {
            var item = Instantiate(deviceItemPrefab, deviceContainer);
            var txt = item.GetComponentInChildren<Text>();
            if (txt != null) txt.text = name;
            
            var btn = item.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => ConnectTo(name, address));
        }
    }
    
    void ConnectTo(string name, string address)
    {
        Debug.Log($"🔵 Connecting to: {name}");
        BluetoothGameManager.Instance?.ConnectToDevice(address);
        ShowWaiting("Đang kết nối...", $"Phòng: {name}");
    }
    
    // === Callbacks from BluetoothGameManager ===
    
    public void OnConnected()
    {
        Debug.Log("🔵 Connected! Starting game...");
        Hide();
    }
    
    public void OnDisconnected()
    {
        if (isCancelling) return; // Ignore if user cancelled
        ShowWaiting("Mất kết nối!", "Vui lòng thử lại");
    }
    
    public void OnScanFinished()
    {
        if (txtScanning != null)
            txtScanning.text = devices.Count > 0 
                ? $"Tìm thấy {devices.Count} phòng" 
                : "Không tìm thấy phòng nào";
    }
}
