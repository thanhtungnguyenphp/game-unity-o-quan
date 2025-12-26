using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Bluetooth UI - Menu and device list
/// </summary>
public class BluetoothUI : MonoBehaviour
{
    public static BluetoothUI Instance;
    
    [Header("Panels")]
    public GameObject mainMenu;
    public GameObject bluetoothMenu;
    public GameObject deviceListPanel;
    public GameObject waitingPanel;
    
    [Header("Buttons")]
    public Button btnLocal;
    public Button btnBluetooth;
    public Button btnCreateGame;
    public Button btnJoinGame;
    public Button btnBack;
    public Button btnBackFromDevices;
    public Button btnCancelWaiting;
    
    [Header("Device List")]
    public Transform deviceListContent;
    public GameObject deviceItemPrefab;
    
    [Header("Waiting")]
    public Text waitingText;
    
    private List<DeviceInfo> devices = new List<DeviceInfo>();
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        // Setup buttons
        if (btnLocal != null)
            btnLocal.onClick.AddListener(OnLocalGame);
        
        if (btnBluetooth != null)
            btnBluetooth.onClick.AddListener(OnBluetoothMenu);
        
        if (btnCreateGame != null)
            btnCreateGame.onClick.AddListener(OnCreateGame);
        
        if (btnJoinGame != null)
            btnJoinGame.onClick.AddListener(OnJoinGame);
        
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBack);
        
        if (btnBackFromDevices != null)
            btnBackFromDevices.onClick.AddListener(OnBackFromDevices);
        
        if (btnCancelWaiting != null)
            btnCancelWaiting.onClick.AddListener(OnCancelWaiting);
        
        ShowMainMenu();
    }
    
    void ShowMainMenu()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (bluetoothMenu != null) bluetoothMenu.SetActive(false);
        if (deviceListPanel != null) deviceListPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);
    }
    
    void OnLocalGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.currentMode = GameMode.Local;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    void OnBluetoothMenu()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (bluetoothMenu != null) bluetoothMenu.SetActive(true);
    }
    
    void OnCreateGame()
    {
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.CreateGame();
        }
        
        if (bluetoothMenu != null) bluetoothMenu.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(true);
        
        if (waitingText != null)
        {
            waitingText.text = "Đang chờ người chơi kết nối...\n\nTên thiết bị: OQuanGame";
        }
    }
    
    void OnJoinGame()
    {
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.JoinGame();
        }
        
        if (bluetoothMenu != null) bluetoothMenu.SetActive(false);
        if (deviceListPanel != null) deviceListPanel.SetActive(true);
        
        // Clear device list
        ClearDeviceList();
    }
    
    void ClearDeviceList()
    {
        devices.Clear();
        
        if (deviceListContent != null)
        {
            foreach (Transform child in deviceListContent)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    public void AddDevice(string name, string address)
    {
        // Check if already added
        if (devices.Exists(d => d.address == address))
            return;
        
        devices.Add(new DeviceInfo { name = name, address = address });
        
        // Create UI item
        if (deviceItemPrefab != null && deviceListContent != null)
        {
            GameObject item = Instantiate(deviceItemPrefab, deviceListContent);
            
            Text nameText = item.transform.Find("Name")?.GetComponent<Text>();
            if (nameText != null)
                nameText.text = name;
            
            Text addressText = item.transform.Find("Address")?.GetComponent<Text>();
            if (addressText != null)
                addressText.text = address;
            
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnDeviceSelected(address));
            }
        }
    }
    
    void OnDeviceSelected(string address)
    {
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.ConnectToDevice(address);
        }
        
        if (deviceListPanel != null) deviceListPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(true);
        
        if (waitingText != null)
        {
            waitingText.text = "Đang kết nối...";
        }
    }
    
    void OnBack()
    {
        ShowMainMenu();
    }
    
    void OnBackFromDevices()
    {
        if (deviceListPanel != null) deviceListPanel.SetActive(false);
        if (bluetoothMenu != null) bluetoothMenu.SetActive(true);
    }
    
    void OnCancelWaiting()
    {
        if (BluetoothGameManager.Instance != null)
        {
            BluetoothGameManager.Instance.Disconnect();
        }
        
        ShowMainMenu();
    }
}
