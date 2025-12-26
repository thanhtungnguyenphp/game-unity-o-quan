# 📡 PROTOCOL - Bluetooth Communication

## Overview

Giao thức truyền dữ liệu giữa 2 thiết bị qua Bluetooth.

---

## Data Format

### MoveData (JSON)

```json
{
  "cellIndex": 3,
  "direction": 1,
  "turn": 0
}
```

| Field | Type | Range | Description |
|-------|------|-------|-------------|
| `cellIndex` | int | 0-13 | Ô được chọn |
| `direction` | int | -1, 1 | Hướng đi (-1=trái, 1=phải) |
| `turn` | int | 0, 1 | Lượt (0=P1, 1=P2) |

### Encoding
- Format: JSON string
- Encoding: UTF-8
- Transport: byte[]

```csharp
// Serialize
MoveData data = new MoveData { cellIndex = 3, direction = 1, turn = 0 };
string json = JsonUtility.ToJson(data);
byte[] bytes = Encoding.UTF8.GetBytes(json);

// Deserialize
string json = Encoding.UTF8.GetString(bytes);
MoveData data = JsonUtility.FromJson<MoveData>(json);
```

---

## Board Layout

```
Index:  [0] [1] [2] [3] [4] [5]  [6]  [7] [8] [9] [10] [11] [12] [13]
        ─────────────────────────────────────────────────────────────
Player:  P1  P1  P1  P1  P1  P1  QUAN  P2  P2  P2  P2   P2   P2  QUAN
```

| Player | Cell Range | Quan |
|--------|------------|------|
| P1 (Host) | 0-5 | 6 |
| P2 (Client) | 7-12 | 13 |

---

## Message Flow

### Turn Sequence
```
┌─────────┐                              ┌─────────┐
│ Player1 │                              │ Player2 │
│ (Host)  │                              │(Client) │
└────┬────┘                              └────┬────┘
     │                                        │
     │  1. Select cell (local)                │
     │  2. Select direction (local)           │
     │  3. Execute move (local)               │
     │                                        │
     │  ──────── MoveData ────────────────►   │
     │                                        │
     │                    4. Validate move    │
     │                    5. Execute move     │
     │                                        │
     │                    6. Select cell      │
     │                    7. Select direction │
     │                    8. Execute move     │
     │                                        │
     │   ◄──────── MoveData ──────────────    │
     │                                        │
     │  9. Validate move                      │
     │  10. Execute move                      │
     │                                        │
```

### Connection Handshake
```
Host                                    Client
  │                                        │
  │  StartServer()                         │
  │  StartDiscoverable()                   │
  │                                        │
  │                         StartScan()    │
  │                                        │
  │  ◄──────── RFCOMM Connect ──────────   │
  │                                        │
  │  OnConnected()           OnConnected() │
  │  isHost = true           isHost = false│
  │  myTurn = P1             myTurn = P2   │
  │                                        │
  │  ──────── Game Start ──────────────►   │
  │                                        │
```

---

## Validation Rules

### Sender Validation (trước khi gửi)
```csharp
// Trong GameManager.OnSelectDirection()
if (currentMode == GameMode.Bluetooth)
{
    // Chỉ gửi nếu đúng lượt của mình
    if (_currentTurn == BluetoothGameManager.Instance.myTurn)
    {
        BluetoothGameManager.Instance.SendMove(cellIndex, direction);
    }
}
```

### Receiver Validation (khi nhận)
```csharp
bool IsValidOpponentMove(MoveData move)
{
    // 1. Đúng lượt đối thủ
    if (GameManager.instance._currentTurn == myTurn)
        return false;  // Không phải lượt đối thủ
    
    // 2. Cell index hợp lệ
    if (move.cellIndex < 0 || move.cellIndex >= 14)
        return false;
    
    // 3. Cell thuộc đối thủ
    PlayerTurn opponent = myTurn == P1 ? P2 : P1;
    if (opponent == P1 && move.cellIndex > 5)
        return false;  // P1 chỉ có cell 0-5
    if (opponent == P2 && move.cellIndex < 6)
        return false;  // P2 chỉ có cell 7-12
    
    // 4. Direction hợp lệ
    if (move.direction != -1 && move.direction != 1)
        return false;
    
    // 5. Cell có quân
    int[] board = GameManager.instance.GetCellValues();
    if (board[move.cellIndex] <= 0)
        return false;
    
    return true;
}
```

---

## Error Handling

### Connection Lost
```csharp
void OnDisconnected(string address)
{
    isConnected = false;
    
    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
        // Chuyển về Local mode
        GameManager.instance.currentMode = GameMode.Local;
        
        // Pause game
        GameManager.instance.PauseGame();
        
        // Thông báo user
        BluetoothUI.Instance?.OnDisconnected();
    });
}
```

### Invalid Move Received
```csharp
void ExecuteOpponentMove(MoveData move)
{
    if (!IsValidOpponentMove(move))
    {
        Debug.LogError($"Invalid move: Cell {move.cellIndex}, Dir {move.direction}");
        // Có thể disconnect hoặc ignore
        return;
    }
    
    // Execute valid move
    GameManager.instance.OnSelectCell(move.cellIndex);
    GameManager.instance.OnSelectDirection(move.direction);
}
```

---

## Security Considerations

### Chống gian lận
1. **Validate mọi move nhận được** - Không tin tưởng client
2. **Kiểm tra đúng lượt** - Không cho phép đi 2 lần liên tiếp
3. **Kiểm tra cell ownership** - Không cho phép đi ô của đối thủ
4. **Kiểm tra game state** - Không cho phép move khi game over

### Limitations
- Không có server trung gian → Không thể verify 100%
- Peer-to-peer → Cả 2 phải trust nhau
- Nếu cần anti-cheat mạnh → Cần server authoritative

---

## Performance

### Latency
- Bluetooth Classic: ~10-100ms
- Acceptable cho turn-based game

### Data Size
- MoveData JSON: ~50 bytes
- Rất nhỏ, không cần optimize

### Frequency
- 1 message per turn
- ~10-30 messages per game
