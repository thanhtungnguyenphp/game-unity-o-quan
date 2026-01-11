using UnityEngine;
using BlueUnity;

/// <summary>
/// Bluetooth Game Manager - Handles multiplayer via Bluetooth
/// Uses BlueUnity plugin with proper handshake protocol
/// </summary>
public class BluetoothGameManager : MonoBehaviour
{
    public static BluetoothGameManager Instance;
    
    public bool isHost = false;
    public PlayerTurn myTurn;
    public bool isConnected = false;
    
    private BluetoothHandler btHandler;
    private bool wasConnected = false;
    private int moveNumber = 0;
    private bool useEncryption = false;
    private bool waitingForRoleResolution = false;
    
    // Handshake state
    private bool handshakeSent = false;
    private bool handshakeReceived = false;
    private bool gameStarted = false;
    
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
        
        // Initialize encryption
        BluetoothEncryption.GenerateSessionKey();
        
        // Setup role resolver
        if (BluetoothRoleResolver.Instance != null)
            BluetoothRoleResolver.Instance.OnRoleResolved += OnRoleResolved;
        
        Debug.Log("✅ Bluetooth initialized");
    }
    
    void OnRoleResolved(bool resolvedAsHost)
    {
        if (!waitingForRoleResolution) return;
        waitingForRoleResolution = false;
        
        isHost = resolvedAsHost;
        myTurn = isHost ? PlayerTurn.P1 : PlayerTurn.P2;
        Debug.Log($"🎯 Role assigned: {(isHost ? "HOST (P1)" : "CLIENT (P2)")}");
    }
    
    // Host creates game
    public void CreateGame()
    {
        if (btHandler == null || !btHandler.Enabled)
        {
            Debug.LogError("❌ Bluetooth not ready!");
            return;
        }
        
        // Reset all state for new game
        ResetState();
        
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
        
        // Start timeout
        BluetoothTimeout.Instance?.StartTimeout(60f, OnCreateTimeout);
        
        Debug.Log("🔵 Server started, waiting for player...");
    }
    
    void ResetState()
    {
        isConnected = false;
        wasConnected = false;
        handshakeSent = false;
        handshakeReceived = false;
        gameStarted = false;
        moveNumber = 0;
    }
    
    void OnCreateTimeout()
    {
        Debug.LogWarning("⏰ Create room timeout");
        btHandler?.Disconnect();
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            BluetoothUI.Instance?.OnTimeout("Hết thời gian chờ kết nối");
        });
    }
    
    // Client joins game
    public void JoinGame()
    {
        if (btHandler == null || !btHandler.Enabled)
        {
            Debug.LogError("❌ Bluetooth not ready!");
            return;
        }
        
        // Reset all state for new game
        ResetState();
        
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
        
        // Then scan for new devices with timeout
        btHandler.StartScan();
        BluetoothTimeout.Instance?.StartScanTimeout(OnScanTimeout);
    }
    
    void OnScanTimeout()
    {
        Debug.LogWarning("⏰ Scan timeout");
        btHandler?.StopScan();
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            BluetoothUI.Instance?.OnScanFinished();
        });
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
        BluetoothTimeout.Instance?.CancelTimeout();
        
        // Enable pairing mode for new devices
        btHandler.SetPairing(true);
        
        // Start connect timeout
        BluetoothTimeout.Instance?.StartConnectTimeout(OnConnectTimeout);
        
        // Small delay to ensure pairing is enabled
        StartCoroutine(ConnectAfterDelay(address, 0.5f));
    }
    
    void OnConnectTimeout()
    {
        Debug.LogWarning("⏰ Connect timeout");
        btHandler?.Disconnect();
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            BluetoothUI.Instance?.OnTimeout("Không thể kết nối");
        });
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
        
        if (cellIndex < 0 || cellIndex >= GameConstants.BOARD_SIZE)
        {
            Debug.LogError($"❌ Invalid cell index: {cellIndex}");
            return;
        }
        
        var move = new MoveMessage
        {
            cellIndex = cellIndex,
            direction = direction,
            turn = GetCurrentTurn(),
            moveNumber = ++moveNumber
        };
        
        var msg = BluetoothMessage.Create(BluetoothMessageType.Move, move);
        SendMessage(msg);
        
        // Notify state sync
        GameStateSync.Instance?.OnMoveExecuted();
        
        Debug.Log($"📤 Sent: Cell {cellIndex}, Dir {direction}");
    }
    
    public void SendMessage(BluetoothMessage msg)
    {
        if (!isConnected || btHandler == null) return;
        
        try
        {
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            
            // Encrypt if enabled
            if (useEncryption)
                bytes = BluetoothEncryption.Encrypt(bytes);
            
            btHandler.Write(bytes);
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
        BluetoothTimeout.Instance?.CancelTimeout();
    }
    
    void OnConnecting(string address)
    {
        Debug.Log($"🔌 Connecting to {address}...");
    }
    
    void OnConnected(string address)
    {
        Debug.Log($"✅✅✅ OnConnected called! address={address}");
        
        isConnected = true;
        wasConnected = true;
        gameStarted = true;
        
        BluetoothTimeout.Instance?.CancelTimeout();
        ReconnectManager.Instance?.SaveConnectionInfo(address);
        
        Debug.Log($"✅ State: isConnected={isConnected}, gameStarted={gameStarted}");
        
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log("🎮 Enqueued: Calling OnConnected and StartGame");
            BluetoothUI.Instance?.OnConnected();
            HeartbeatManager.Instance?.StartHeartbeat();
            StartGame();
        });
    }
    
    private System.Collections.IEnumerator SendHandshakeDelayed()
    {
        yield return new WaitForSeconds(0.3f);
        
        if (!isConnected) yield break;
        
        // Send ready message
        var ready = new ReadyMessage { isHost = this.isHost, deviceId = SystemInfo.deviceUniqueIdentifier };
        var msg = BluetoothMessage.Create(BluetoothMessageType.Ready, ready);
        SendMessageRaw(msg);
        handshakeSent = true;
        
        Debug.Log($"📤 Sent handshake (isHost={isHost})");
        
        CheckBothReady();
    }
    
    void CheckBothReady()
    {
        if (handshakeSent && handshakeReceived && !gameStarted)
        {
            gameStarted = true;
            Debug.Log("✅ Both devices ready, starting game!");
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BluetoothUI.Instance?.OnConnected();
                HeartbeatManager.Instance?.StartHeartbeat();
                StartGame();
            });
        }
    }
    
    void SendMessageRaw(BluetoothMessage msg)
    {
        if (btHandler == null) return;
        
        try
        {
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            btHandler.Write(bytes);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to send: {e.Message}");
        }
    }
    
    void SendEncryptionKey()
    {
        var keyMsg = new KeyExchangeMessage { key = System.Convert.ToBase64String(BluetoothEncryption.GetSessionKey()) };
        var msg = BluetoothMessage.Create(BluetoothMessageType.KeyExchange, keyMsg);
        
        // Send key unencrypted
        string json = JsonUtility.ToJson(msg);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        btHandler?.Write(bytes);
    }
    
    void OnDisconnected(string address)
    {
        Debug.Log($"❌❌❌ OnDisconnected called! address={address}, wasConnected={wasConnected}, isConnected={isConnected}, gameStarted={gameStarted}");
        
        // Only handle if we were actually connected
        if (!wasConnected && !isConnected)
        {
            Debug.Log("Ignoring disconnect - was not connected");
            return;
        }
        
        bool wasInGame = gameStarted;
        
        isConnected = false;
        wasConnected = false;
        handshakeSent = false;
        handshakeReceived = false;
        
        HeartbeatManager.Instance?.StopHeartbeat();
        
        if (wasInGame)
        {
            Debug.Log("Was in game, starting reconnect...");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BluetoothUI.Instance?.OnDisconnected();
                ReconnectManager.Instance?.StartReconnect();
            });
        }
        else
        {
            Debug.Log("Not in game, showing menu...");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                BluetoothUI.Instance?.ShowMenu();
            });
        }
    }
    
    void OnDataReceived(byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        Debug.Log($"📥 Received: {json}");
        
        try
        {
            var msg = JsonUtility.FromJson<BluetoothMessage>(json);
            if (msg != null && msg.type >= 0)
            {
                var msgType = (BluetoothMessageType)msg.type;
                
                // Handle Ready message for handshake
                if (msgType == BluetoothMessageType.Ready)
                {
                    handshakeReceived = true;
                    Debug.Log("📥 Received handshake from peer");
                    UnityMainThreadDispatcher.Instance().Enqueue(() => CheckBothReady());
                    return;
                }
                
                // Handle key exchange
                if (msgType == BluetoothMessageType.KeyExchange)
                {
                    var keyMsg = JsonUtility.FromJson<KeyExchangeMessage>(msg.payload);
                    BluetoothEncryption.SetSessionKey(System.Convert.FromBase64String(keyMsg.key));
                    Debug.Log("🔐 Encryption key received");
                    return;
                }
                
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    BluetoothMessageQueue.Instance?.Enqueue(msg);
                });
                return;
            }
        }
        catch { }
        
        // Fallback to old MoveData format
        try
        {
            var move = JsonUtility.FromJson<MoveData>(json);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ExecuteOpponentMove(move);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to parse message: {e.Message}");
        }
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
        
        // Set flag for LoadingState to detect
        PlayerPrefs.SetString("GameMode", "Bluetooth");
        PlayerPrefs.Save();
        
        // Load GameScene - LoadingState will handle the rest
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    
    void OnGameSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnGameSceneLoaded;
            StartCoroutine(SetModeAfterLoad());
        }
    }
    
    private System.Collections.IEnumerator SetModeAfterLoad()
    {
        // Wait for GameManager to be ready
        float timeout = 3f;
        while (GameManager.instance == null && timeout > 0)
        {
            yield return new WaitForSeconds(0.1f);
            timeout -= 0.1f;
        }
        
        if (GameManager.instance != null)
        {
            GameManager.instance.currentMode = GameMode.Bluetooth;
            Debug.Log("✅ Bluetooth mode set after scene load!");
        }
        else
        {
            Debug.LogError("❌ GameManager not found after timeout!");
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
        BluetoothTimeout.Instance?.CancelTimeout();
        BluetoothRoleResolver.Instance?.Reset();
        if (btHandler != null)
        {
            btHandler.Disconnect();
        }
        isConnected = false;
        waitingForRoleResolution = false;
        handshakeSent = false;
        handshakeReceived = false;
        gameStarted = false;
    }
    
    void OnDestroy()
    {
        Disconnect();
        
        if (BluetoothRoleResolver.Instance != null)
            BluetoothRoleResolver.Instance.OnRoleResolved -= OnRoleResolved;
        
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
