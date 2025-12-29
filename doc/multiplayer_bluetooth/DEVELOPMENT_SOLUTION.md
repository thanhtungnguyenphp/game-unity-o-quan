# 🚀 GIẢI PHÁP PHÁT TRIỂN BLUETOOTH MULTIPLAYER

**Ngày tạo:** 2025-12-29  
**Phiên bản:** 1.0

---

## 📋 MỤC LỤC

1. [Tổng quan giải pháp](#1-tổng-quan-giải-pháp)
2. [Chi tiết implementation](#2-chi-tiết-implementation)
3. [Code mẫu](#3-code-mẫu)
4. [Hướng dẫn tích hợp](#4-hướng-dẫn-tích-hợp)
5. [Testing guide](#5-testing-guide)

---

## 1. TỔNG QUAN GIẢI PHÁP

### 1.1 Mục tiêu

| Mục tiêu | Mô tả | Độ ưu tiên |
|----------|-------|------------|
| **Stability** | Không crash, không desync | P0 |
| **Reconnect** | Cho phép kết nối lại sau disconnect | P1 |
| **State Sync** | Đồng bộ game state giữa 2 máy | P1 |
| **UX** | Trải nghiệm mượt mà, thông báo rõ ràng | P2 |

### 1.2 Kiến trúc đề xuất

```
┌─────────────────────────────────────────────────────────────┐
│                    ENHANCED BLUETOOTH LAYER                  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────┐    ┌─────────────────┐                 │
│  │BluetoothGame    │◄──►│ BluetoothUI     │                 │
│  │   Manager       │    │  (Enhanced)     │                 │
│  └────────┬────────┘    └─────────────────┘                 │
│           │                                                  │
│           ▼                                                  │
│  ┌─────────────────┐    ┌─────────────────┐                 │
│  │ MessageQueue    │◄──►│ GameStateSync   │  ← NEW          │
│  │ (Ordered exec)  │    │ (State sync)    │                 │
│  └────────┬────────┘    └────────┬────────┘                 │
│           │                      │                           │
│           ▼                      ▼                           │
│  ┌─────────────────┐    ┌─────────────────┐                 │
│  │ HeartbeatMgr    │    │ ReconnectMgr    │  ← NEW          │
│  │ (Connection)    │    │ (Auto reconnect)│                 │
│  └─────────────────┘    └─────────────────┘                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 1.3 Message Types

```csharp
public enum BluetoothMessageType
{
    Move = 0,           // Nước đi
    StateSync = 1,      // Đồng bộ state
    Heartbeat = 2,      // Kiểm tra kết nối
    Ack = 3,            // Xác nhận nhận được
    RequestSync = 4,    // Yêu cầu sync
    GameOver = 5        // Kết thúc game
}
```

---

## 2. CHI TIẾT IMPLEMENTATION

### 2.1 Message Queue System

**Mục đích:** Đảm bảo các message được xử lý theo thứ tự, tránh race condition.

**Flow:**
```
Receive Data → Parse → Enqueue → Process One by One → Execute
```

**Key Features:**
- FIFO queue
- Chờ animation xong mới process tiếp
- Timeout cho mỗi message
- Error handling

### 2.2 State Sync System

**Mục đích:** Đảm bảo 2 máy luôn có cùng game state.

**Khi nào sync:**
1. Sau mỗi 5 nước đi
2. Khi reconnect
3. Khi detect desync
4. Khi request từ đối thủ

**Sync Data:**
```csharp
public class GameStateData
{
    public int[] board;          // 14 cells
    public int p1Score;
    public int p2Score;
    public int currentTurn;      // 0 or 1
    public int moveCount;        // Để detect desync
    public bool quan1Available;
    public bool quan2Available;
    public long timestamp;       // Unix timestamp
}
```

### 2.3 Reconnect System

**Flow:**
```
Disconnect Detected
       │
       ▼
Save Current State
       │
       ▼
Show Reconnect UI
       │
       ▼
Attempt Reconnect (5 times, 3s interval)
       │
       ├─► Success → Sync State → Resume Game
       │
       └─► Fail → Show Options (Retry/Exit)
```

### 2.4 Heartbeat System

**Mục đích:** Detect silent disconnect (khi Bluetooth không trigger event).

**Config:**
- Interval: 2 giây
- Timeout: 6 giây (3 missed heartbeats)

---

## 3. CODE MẪU

### 3.1 BluetoothMessage.cs (Data Models)

```csharp
using System;
using UnityEngine;

[Serializable]
public class BluetoothMessage
{
    public int type;        // BluetoothMessageType
    public int messageId;
    public string payload;  // JSON của data cụ thể
    public long timestamp;
    
    public static BluetoothMessage Create<T>(BluetoothMessageType type, T data)
    {
        return new BluetoothMessage
        {
            type = (int)type,
            messageId = GenerateId(),
            payload = JsonUtility.ToJson(data),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
    
    private static int _nextId = 0;
    private static int GenerateId() => _nextId++;
}

[Serializable]
public class MoveMessage
{
    public int cellIndex;
    public int direction;
    public int turn;
    public int moveNumber;
}

[Serializable]
public class StateSyncMessage
{
    public int[] board;
    public int p1Score;
    public int p2Score;
    public int currentTurn;
    public int moveCount;
    public bool quan1Available;
    public bool quan2Available;
}

[Serializable]
public class HeartbeatMessage
{
    public long timestamp;
    public int moveCount;
}

[Serializable]
public class AckMessage
{
    public int messageId;
    public bool success;
    public string error;
}
```

### 3.2 BluetoothMessageQueue.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluetoothMessageQueue : MonoBehaviour
{
    public static BluetoothMessageQueue Instance { get; private set; }
    
    private Queue<BluetoothMessage> messageQueue = new Queue<BluetoothMessage>();
    private bool isProcessing = false;
    private float processDelay = 0.1f;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void Enqueue(BluetoothMessage message)
    {
        messageQueue.Enqueue(message);
        if (!isProcessing)
            StartCoroutine(ProcessQueue());
    }
    
    private IEnumerator ProcessQueue()
    {
        isProcessing = true;
        
        while (messageQueue.Count > 0)
        {
            var msg = messageQueue.Dequeue();
            yield return ProcessMessage(msg);
            yield return new WaitForSeconds(processDelay);
        }
        
        isProcessing = false;
    }
    
    private IEnumerator ProcessMessage(BluetoothMessage msg)
    {
        var type = (BluetoothMessageType)msg.type;
        
        switch (type)
        {
            case BluetoothMessageType.Move:
                yield return ProcessMove(msg);
                break;
            case BluetoothMessageType.StateSync:
                ProcessStateSync(msg);
                break;
            case BluetoothMessageType.Heartbeat:
                ProcessHeartbeat(msg);
                break;
            case BluetoothMessageType.Ack:
                ProcessAck(msg);
                break;
            case BluetoothMessageType.RequestSync:
                SendStateSync();
                break;
        }
    }
    
    private IEnumerator ProcessMove(BluetoothMessage msg)
    {
        var move = JsonUtility.FromJson<MoveMessage>(msg.payload);
        
        // Validate
        if (!BluetoothGameManager.Instance.IsValidOpponentMove(move))
        {
            Debug.LogError($"Invalid move: {move.cellIndex}");
            yield break;
        }
        
        // Execute
        GameManager.instance.OnSelectCell(move.cellIndex);
        GameManager.instance.OnSelectDirection(move.direction);
        
        // Wait for animation
        yield return new WaitForSeconds(0.5f);
        
        // Send ACK
        SendAck(msg.messageId, true);
    }
    
    private void ProcessStateSync(BluetoothMessage msg)
    {
        var state = JsonUtility.FromJson<StateSyncMessage>(msg.payload);
        GameStateSync.Instance?.ApplyState(state);
    }
    
    private void ProcessHeartbeat(BluetoothMessage msg)
    {
        HeartbeatManager.Instance?.OnHeartbeatReceived();
    }
    
    private void ProcessAck(BluetoothMessage msg)
    {
        var ack = JsonUtility.FromJson<AckMessage>(msg.payload);
        // Handle ACK (remove from pending, etc.)
    }
    
    private void SendAck(int messageId, bool success)
    {
        var ack = new AckMessage { messageId = messageId, success = success };
        var msg = BluetoothMessage.Create(BluetoothMessageType.Ack, ack);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
    
    private void SendStateSync()
    {
        GameStateSync.Instance?.SendCurrentState();
    }
}
```

### 3.3 GameStateSync.cs

```csharp
using UnityEngine;

public class GameStateSync : MonoBehaviour
{
    public static GameStateSync Instance { get; private set; }
    
    private int syncInterval = 5; // Sync mỗi 5 moves
    private int lastSyncMoveCount = 0;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void OnMoveExecuted(int moveCount)
    {
        if (moveCount - lastSyncMoveCount >= syncInterval)
        {
            SendCurrentState();
            lastSyncMoveCount = moveCount;
        }
    }
    
    public void SendCurrentState()
    {
        if (GameManager.instance == null) return;
        
        var state = new StateSyncMessage
        {
            board = GameManager.instance.GetCellValues(),
            p1Score = GetScore(PlayerTurn.P1),
            p2Score = GetScore(PlayerTurn.P2),
            currentTurn = (int)GameManager.instance._currentTurn,
            moveCount = GetMoveCount(),
            quan1Available = GameManager.instance.BoardManager.Quan1Available,
            quan2Available = GameManager.instance.BoardManager.Quan2Available
        };
        
        var msg = BluetoothMessage.Create(BluetoothMessageType.StateSync, state);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
    
    public void ApplyState(StateSyncMessage state)
    {
        if (GameManager.instance == null) return;
        
        // Check for desync
        var localState = GetCurrentState();
        if (!StatesMatch(localState, state))
        {
            Debug.LogWarning("⚠️ Desync detected! Applying remote state...");
            ForceApplyState(state);
        }
    }
    
    private bool StatesMatch(StateSyncMessage a, StateSyncMessage b)
    {
        if (a.moveCount != b.moveCount) return false;
        if (a.p1Score != b.p1Score) return false;
        if (a.p2Score != b.p2Score) return false;
        
        for (int i = 0; i < a.board.Length; i++)
        {
            if (a.board[i] != b.board[i]) return false;
        }
        
        return true;
    }
    
    private void ForceApplyState(StateSyncMessage state)
    {
        // Apply board
        var board = GameManager.instance.BoardManager;
        for (int i = 0; i < state.board.Length; i++)
        {
            board.board[i] = state.board[i];
        }
        
        // Apply scores (need ScoreManager access)
        // Apply turn
        // Update UI
        
        GameManager.instance.UIControl?.UpdateBoard(board.board);
    }
    
    private StateSyncMessage GetCurrentState()
    {
        return new StateSyncMessage
        {
            board = GameManager.instance.GetCellValues(),
            p1Score = GetScore(PlayerTurn.P1),
            p2Score = GetScore(PlayerTurn.P2),
            currentTurn = (int)GameManager.instance._currentTurn,
            moveCount = GetMoveCount()
        };
    }
    
    private int GetScore(PlayerTurn player) => 0; // TODO: Get from ScoreManager
    private int GetMoveCount() => 0; // TODO: Track move count
}
```

### 3.4 HeartbeatManager.cs

```csharp
using UnityEngine;

public class HeartbeatManager : MonoBehaviour
{
    public static HeartbeatManager Instance { get; private set; }
    
    [SerializeField] private float heartbeatInterval = 2f;
    [SerializeField] private float timeoutDuration = 6f;
    
    private float lastHeartbeatSent;
    private float lastHeartbeatReceived;
    private bool isActive = false;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    void Update()
    {
        if (!isActive) return;
        if (BluetoothGameManager.Instance?.isConnected != true) return;
        
        // Send heartbeat
        if (Time.time - lastHeartbeatSent >= heartbeatInterval)
        {
            SendHeartbeat();
            lastHeartbeatSent = Time.time;
        }
        
        // Check timeout
        if (Time.time - lastHeartbeatReceived > timeoutDuration)
        {
            OnConnectionTimeout();
        }
    }
    
    public void StartHeartbeat()
    {
        isActive = true;
        lastHeartbeatReceived = Time.time;
        lastHeartbeatSent = Time.time;
    }
    
    public void StopHeartbeat()
    {
        isActive = false;
    }
    
    public void OnHeartbeatReceived()
    {
        lastHeartbeatReceived = Time.time;
    }
    
    private void SendHeartbeat()
    {
        var hb = new HeartbeatMessage
        {
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            moveCount = 0 // TODO: Get actual move count
        };
        
        var msg = BluetoothMessage.Create(BluetoothMessageType.Heartbeat, hb);
        BluetoothGameManager.Instance?.SendMessage(msg);
    }
    
    private void OnConnectionTimeout()
    {
        Debug.LogWarning("⚠️ Connection timeout detected!");
        isActive = false;
        
        // Trigger reconnect
        ReconnectManager.Instance?.StartReconnect();
    }
}
```

### 3.5 ReconnectManager.cs

```csharp
using System.Collections;
using UnityEngine;

public class ReconnectManager : MonoBehaviour
{
    public static ReconnectManager Instance { get; private set; }
    
    [SerializeField] private int maxAttempts = 5;
    [SerializeField] private float attemptInterval = 3f;
    
    private string lastAddress;
    private StateSyncMessage savedState;
    private bool isReconnecting = false;
    
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public void SaveConnectionInfo(string address)
    {
        lastAddress = address;
    }
    
    public void StartReconnect()
    {
        if (isReconnecting) return;
        if (string.IsNullOrEmpty(lastAddress)) return;
        
        // Save current state
        savedState = GameStateSync.Instance?.GetCurrentState();
        
        // Pause game
        GameManager.instance?.PauseGame();
        
        // Start reconnect
        StartCoroutine(ReconnectCoroutine());
    }
    
    private IEnumerator ReconnectCoroutine()
    {
        isReconnecting = true;
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            attempts++;
            BluetoothUI.Instance?.ShowReconnecting(attempts, maxAttempts);
            
            BluetoothGameManager.Instance?.ConnectToDevice(lastAddress);
            
            yield return new WaitForSeconds(attemptInterval);
            
            if (BluetoothGameManager.Instance?.isConnected == true)
            {
                OnReconnectSuccess();
                yield break;
            }
        }
        
        OnReconnectFailed();
    }
    
    private void OnReconnectSuccess()
    {
        isReconnecting = false;
        
        // Sync state
        GameStateSync.Instance?.SendCurrentState();
        
        // Resume game
        GameManager.instance?.ResumeGame();
        
        // Update UI
        BluetoothUI.Instance?.ShowReconnected();
        
        // Restart heartbeat
        HeartbeatManager.Instance?.StartHeartbeat();
    }
    
    private void OnReconnectFailed()
    {
        isReconnecting = false;
        
        // Show options
        BluetoothUI.Instance?.ShowReconnectFailed(
            onRetry: () => StartReconnect(),
            onExit: () => ExitToMenu()
        );
    }
    
    private void ExitToMenu()
    {
        GameManager.instance?.currentMode = GameMode.Local;
        // Load menu scene
    }
}
```

---

## 4. HƯỚNG DẪN TÍCH HỢP

### 4.1 Bước 1: Tạo các file mới

1. Tạo `Assets/Scripts/Client/Data/BluetoothMessage.cs`
2. Tạo `Assets/Scripts/Client/Core/BluetoothMessageQueue.cs`
3. Tạo `Assets/Scripts/Client/Core/GameStateSync.cs`
4. Tạo `Assets/Scripts/Client/Core/HeartbeatManager.cs`
5. Tạo `Assets/Scripts/Client/Core/ReconnectManager.cs`

### 4.2 Bước 2: Cập nhật BluetoothGameManager.cs

```csharp
// Thêm vào BluetoothGameManager.cs

// Thay đổi OnDataReceived
void OnDataReceived(byte[] data)
{
    string json = System.Text.Encoding.UTF8.GetString(data);
    var msg = JsonUtility.FromJson<BluetoothMessage>(json);
    
    // Sử dụng queue thay vì execute trực tiếp
    BluetoothMessageQueue.Instance?.Enqueue(msg);
}

// Thêm method mới
public void SendMessage(BluetoothMessage msg)
{
    if (!isConnected) return;
    
    string json = JsonUtility.ToJson(msg);
    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
    btHandler.Write(bytes);
}

// Cập nhật OnConnected
void OnConnected(string address)
{
    isConnected = true;
    
    // Save for reconnect
    ReconnectManager.Instance?.SaveConnectionInfo(address);
    
    // Start heartbeat
    HeartbeatManager.Instance?.StartHeartbeat();
    
    // ... existing code
}
```

### 4.3 Bước 3: Cập nhật GameManager.cs

```csharp
// Thêm vào GameManager.cs sau khi execute move

// Trong HandleTurn() sau khi move thành công
if (currentMode == GameMode.Bluetooth)
{
    GameStateSync.Instance?.OnMoveExecuted(moveCount);
}
```

### 4.4 Bước 4: Setup Scene

1. Tạo GameObject "BluetoothManagers"
2. Add components:
   - BluetoothMessageQueue
   - GameStateSync
   - HeartbeatManager
   - ReconnectManager
3. Đặt DontDestroyOnLoad

---

## 5. TESTING GUIDE

### 5.1 Unit Tests

```csharp
// Test message serialization
[Test]
public void TestMoveMessageSerialization()
{
    var move = new MoveMessage { cellIndex = 3, direction = 1, turn = 0 };
    var msg = BluetoothMessage.Create(BluetoothMessageType.Move, move);
    
    string json = JsonUtility.ToJson(msg);
    var parsed = JsonUtility.FromJson<BluetoothMessage>(json);
    var parsedMove = JsonUtility.FromJson<MoveMessage>(parsed.payload);
    
    Assert.AreEqual(move.cellIndex, parsedMove.cellIndex);
    Assert.AreEqual(move.direction, parsedMove.direction);
}

// Test state comparison
[Test]
public void TestStateComparison()
{
    var state1 = new StateSyncMessage { board = new int[14], moveCount = 5 };
    var state2 = new StateSyncMessage { board = new int[14], moveCount = 5 };
    
    Assert.IsTrue(GameStateSync.StatesMatch(state1, state2));
    
    state2.moveCount = 6;
    Assert.IsFalse(GameStateSync.StatesMatch(state1, state2));
}
```

### 5.2 Integration Tests

| Test | Steps | Expected |
|------|-------|----------|
| **Reconnect** | 1. Connect 2 devices<br>2. Start game<br>3. Turn off BT on one device<br>4. Turn on BT | Auto reconnect, game continues |
| **Desync** | 1. Manually modify board on one device<br>2. Wait for sync | State corrected |
| **Timeout** | 1. Connect<br>2. Move one device out of range | Timeout detected after 6s |

### 5.3 Manual Test Checklist

- [ ] Tạo phòng thành công
- [ ] Tìm phòng thành công
- [ ] Kết nối thành công
- [ ] Gửi move thành công
- [ ] Nhận move thành công
- [ ] Heartbeat hoạt động
- [ ] Timeout detection hoạt động
- [ ] Reconnect thành công
- [ ] State sync sau reconnect
- [ ] Desync detection hoạt động
- [ ] UI hiển thị đúng trạng thái

---

**Tác giả:** Kiro AI Assistant  
**Ngày cập nhật:** 2025-12-29
