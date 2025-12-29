using UnityEngine;
using BlueUnity;

/// <summary>
/// Bluetooth Game Manager - Handles multiplayer via Bluetooth
/// Uses BlueUnity plugin
/// </summary>
public class BluetoothGameManager : MonoBehaviour
{
    public static BluetoothGameManager Instance;
    
    public bool isHost = false;
    public PlayerTurn myTurn;
    
    private BluetoothHandler btHandler;
    private bool isConnected = false;
    private bool wasConnected = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        StartCoroutine(InitializeBluetooth());
    }
    
    System.Collections.IEnumerator InitializeBluetooth()
    {
        Debug.Log("🔧 Initializing Bluetooth...");
        
        btHandler = BluetoothHandler.Instance;
        
        // Wait for Bluetooth to be enabled
        Debug.Log("⏳ Waiting for Bluetooth to be enabled...");
        yield return new WaitUntil(() => btHandler.Enabled);
        Debug.Log("✅ Bluetooth is enabled");
        
        // Subscribe to events
        btHandler.ScanStartedAction += OnScanStarted;
        btHandler.ScanDeviceFoundAction += OnDeviceFound;
        btHandler.ScanFinishedAction += OnScanFinished;
        btHandler.ConnectingAction += OnConnecting;
        btHandler.ConnectedAction += OnConnected;
        btHandler.DisconnectedAction += OnDisconnected;
        btHandler.DataReceivedAction += OnDataReceived;
        btHandler.ErrorAction += OnError;
        
        // Enable pairing by default
        btHandler.SetPairing(true);
        
        Debug.Log("✅ Bluetooth initialized");
    }
    
    // Host creates game
    public void CreateGame()
    {
        if (btHandler == null || !btHandler.Enabled)
        {
            Debug.LogError("❌ Bluetooth not ready!");
            return;
        }
        
        isHost = true;
        myTurn = PlayerTurn.P1;
        
        Debug.Log("🔵 Creating Bluetooth game...");
        
        // Disconnect any existing connection first
        btHandler.Disconnect();
        
        // Set device name
        btHandler.SetDeviceName("OQuanGame");
        
        // Make discoverable and start server (like example)
        btHandler.StartDiscoverable(300); // 5 minutes
        btHandler.StartServer();
        
        Debug.Log("🔵 Server started, waiting for player...");
    }
    
    // Client joins game
    public void JoinGame()
    {
        if (btHandler == null || !btHandler.Enabled)
        {
            Debug.LogError("❌ Bluetooth not ready!");
            return;
        }
        
        isHost = false;
        myTurn = PlayerTurn.P2;
        
        Debug.Log("🔍 Scanning for games...");
        
        // Enable pairing
        btHandler.SetPairing(true);
        
        // First check paired devices
        var pairedDevices = btHandler.PairedDevices;
        Debug.Log($"📋 Found {pairedDevices.Length} paired devices");
        foreach (var device in pairedDevices)
        {
            Debug.Log($"📱 Paired: {device.name} ({device.address})");
            // Add paired devices to UI first
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BluetoothUI.Instance?.AddDevice(device.name + " (Paired)", device.address);
            });
        }
        
        // Then scan for new devices
        btHandler.StartScan();
    }
    
    public void ConnectToDevice(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("❌ Invalid address!");
            return;
        }
        
        Debug.Log($"🔌 Connecting to {address}...");
        
        // Stop scanning before connect
        btHandler.StopScan();
        
        // Enable pairing mode for new devices
        btHandler.SetPairing(true);
        
        // Small delay to ensure pairing is enabled
        StartCoroutine(ConnectAfterDelay(address, 0.5f));
    }
    
    private System.Collections.IEnumerator ConnectAfterDelay(string address, float delay)
    {
        yield return new WaitForSeconds(delay);
        btHandler.ConnectAsClient(address);
    }
    
    public void SendMove(int cellIndex, int direction)
    {
        if (!isConnected)
        {
            Debug.LogWarning("⚠️ Not connected");
            return;
        }
        
        // Validate our own move before sending
        if (cellIndex < 0 || cellIndex >= GameConstants.BOARD_SIZE)
        {
            Debug.LogError($"❌ Invalid cell index to send: {cellIndex}");
            return;
        }
        
        MoveData data = new MoveData
        {
            cellIndex = cellIndex,
            direction = direction,
            turn = GetCurrentTurn()
        };
        
        try
        {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
            btHandler.Write(bytes);
            
            Debug.Log($"📤 Sent: Cell {cellIndex}, Direction {direction}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to send move: {e.Message}");
        }
    }
    
    // === Bluetooth Events ===
    
    void OnScanStarted()
    {
        Debug.Log("🔍 Scan started");
    }
    
    void OnDeviceFound(string name, string address)
    {
        Debug.Log($"📱 Found: {name} ({address})");
        
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (BluetoothUI.Instance != null)
            {
                BluetoothUI.Instance.AddDevice(name, address);
            }
        });
    }
    
    void OnScanFinished()
    {
        Debug.Log("✅ Scan finished");
    }
    
    void OnConnecting(string address)
    {
        Debug.Log($"🔌 Connecting to {address}...");
    }
    
    void OnConnected(string address)
    {
        isConnected = true;
        wasConnected = true;
        Debug.Log($"✅ Connected to {address}!");
        
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            BluetoothUI.Instance?.OnConnected();
            StartGame();
        });
    }
    
    void OnDisconnected(string address)
    {
        Debug.Log($"❌ Disconnected from {address}");
        
        // Only notify UI if we were actually connected before
        bool shouldNotifyUI = wasConnected || isConnected;
        isConnected = false;
        wasConnected = false;
        
        if (shouldNotifyUI)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BluetoothUI.Instance?.OnDisconnected();
                HandleDisconnection();
            });
        }
    }
    
    private void HandleDisconnection()
    {
        Debug.Log("🔄 Handling disconnection...");
        
        // Save current game state for potential resume
        if (GameManager.instance != null)
        {
            // Switch to local mode
            GameManager.instance.currentMode = GameMode.Local;
            
            // Show disconnection notification to user
            Debug.LogWarning("⚠️ Connection lost! Switching to local mode.");
            
            // Pause game to give user time to react
            GameManager.instance.PauseGame();
        }
        
        // Attempt reconnection if desired
        // StartCoroutine(AttemptReconnection());
    }
    
    void OnDataReceived(byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        
        Debug.Log($"📥 Received: {json}");
        
        MoveData move = JsonUtility.FromJson<MoveData>(json);
        
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ExecuteOpponentMove(move);
        });
    }
    
    void OnError(string error)
    {
        Debug.LogError($"❌ Bluetooth error: {error}");
    }
    
    // === Game Logic ===
    
    void ExecuteOpponentMove(MoveData move)
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("❌ GameManager not found!");
            return;
        }
        
        // Validate opponent move before executing
        if (!IsValidOpponentMove(move))
        {
            Debug.LogError($"❌ Invalid opponent move received: Cell {move.cellIndex}, Direction {move.direction}");
            // TODO: Report potential cheating to server
            return;
        }
        
        // Execute validated move
        GameManager.instance.OnSelectCell(move.cellIndex);
        GameManager.instance.OnSelectDirection(move.direction);
    }
    
    private bool IsValidOpponentMove(MoveData move)
    {
        // Check if it's opponent's turn
        if (GameManager.instance._currentTurn == myTurn)
        {
            Debug.LogWarning("⚠️ Opponent moved on our turn!");
            return false;
        }
        
        // Validate cell index
        if (move.cellIndex < 0 || move.cellIndex >= GameConstants.BOARD_SIZE)
        {
            Debug.LogWarning($"⚠️ Invalid cell index: {move.cellIndex}");
            return false;
        }
        
        // Check if cell belongs to opponent
        PlayerTurn opponentTurn = myTurn == PlayerTurn.P1 ? PlayerTurn.P2 : PlayerTurn.P1;
        if (opponentTurn == PlayerTurn.P1 && move.cellIndex > 5)
        {
            Debug.LogWarning("⚠️ Opponent tried to move our cells!");
            return false;
        }
        
        if (opponentTurn == PlayerTurn.P2 && move.cellIndex < 6)
        {
            Debug.LogWarning("⚠️ Opponent tried to move our cells!");
            return false;
        }
        
        // Validate direction (-1 or 1)
        if (move.direction != -1 && move.direction != 1)
        {
            Debug.LogWarning($"⚠️ Invalid direction: {move.direction}");
            return false;
        }
        
        // Check if cell has stones
        int[] board = GameManager.instance.GetCellValues();
        if (board[move.cellIndex] <= 0)
        {
            Debug.LogWarning($"⚠️ Cell {move.cellIndex} has no stones!");
            return false;
        }
        
        return true;
    }
    
    void StartGame()
    {
        Debug.Log("🎮 Starting Bluetooth game...");
        
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"📍 Current scene: {currentScene}");
        
        if (currentScene == "GameScene")
        {
            // Already in game scene, just set mode and reset
            StartCoroutine(SetupGameAfterDelay());
        }
        else
        {
            // Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
    
    private System.Collections.IEnumerator SetupGameAfterDelay()
    {
        // Wait for GameManager to be ready
        yield return new WaitForSeconds(0.5f);
        
        int attempts = 0;
        while (GameManager.instance == null && attempts < 10)
        {
            yield return new WaitForSeconds(0.2f);
            attempts++;
        }
        
        if (GameManager.instance != null)
        {
            Debug.Log("🎮 Setting Bluetooth mode...");
            GameManager.instance.currentMode = GameMode.Bluetooth;
            GameManager.instance.ResetGame();
        }
        else
        {
            Debug.LogError("❌ GameManager not found!");
        }
    }
    
    int GetCurrentTurn()
    {
        if (GameManager.instance != null)
        {
            return (int)GameManager.instance._currentTurn;
        }
        return 0;
    }
    
    public void Disconnect()
    {
        if (btHandler != null)
        {
            btHandler.Disconnect();
        }
        isConnected = false;
    }
    
    void OnDestroy()
    {
        Disconnect();
        
        if (btHandler != null)
        {
            btHandler.ScanStartedAction -= OnScanStarted;
            btHandler.ScanDeviceFoundAction -= OnDeviceFound;
            btHandler.ScanFinishedAction -= OnScanFinished;
            btHandler.ConnectingAction -= OnConnecting;
            btHandler.ConnectedAction -= OnConnected;
            btHandler.DisconnectedAction -= OnDisconnected;
            btHandler.DataReceivedAction -= OnDataReceived;
            btHandler.ErrorAction -= OnError;
        }
    }
}
