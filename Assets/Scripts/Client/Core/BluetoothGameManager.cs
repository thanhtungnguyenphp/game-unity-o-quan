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
        InitializeBluetooth();
    }
    
    void InitializeBluetooth()
    {
        Debug.Log("🔧 Initializing Bluetooth...");
        
        btHandler = BluetoothHandler.Instance;
        
        // Subscribe to events
        btHandler.ScanStartedAction += OnScanStarted;
        btHandler.ScanDeviceFoundAction += OnDeviceFound;
        btHandler.ScanFinishedAction += OnScanFinished;
        btHandler.ConnectingAction += OnConnecting;
        btHandler.ConnectedAction += OnConnected;
        btHandler.DisconnectedAction += OnDisconnected;
        btHandler.DataReceivedAction += OnDataReceived;
        btHandler.ErrorAction += OnError;
        
        Debug.Log("✅ Bluetooth initialized");
    }
    
    // Host creates game
    public void CreateGame()
    {
        if (btHandler == null)
        {
            Debug.LogError("❌ BluetoothHandler not initialized!");
            InitializeBluetooth();
            return;
        }
        
        isHost = true;
        myTurn = PlayerTurn.P1;
        
        Debug.Log("🔵 Creating Bluetooth game...");
        
        // Set device name
        btHandler.SetDeviceName("OQuanGame");
        
        // Make discoverable
        btHandler.StartDiscoverable(300); // 5 minutes
        
        // Start server
        btHandler.StartServer();
        
        Debug.Log("🔵 Waiting for player...");
    }
    
    // Client joins game
    public void JoinGame()
    {
        if (btHandler == null)
        {
            Debug.LogError("❌ BluetoothHandler not initialized!");
            InitializeBluetooth();
            return;
        }
        
        isHost = false;
        myTurn = PlayerTurn.P2;
        
        Debug.Log("🔍 Joining Bluetooth game...");
        
        // Start scanning
        btHandler.StartScan();
    }
    
    public void ConnectToDevice(string address)
    {
        Debug.Log($"🔌 Connecting to {address}...");
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
        if (GameManager.instance != null)
        {
            GameManager.instance.currentMode = GameMode.Bluetooth;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
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
