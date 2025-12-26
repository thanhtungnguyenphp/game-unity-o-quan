# 📚 API REFERENCE - Bluetooth Multiplayer

## Classes

### BluetoothGameManager

Singleton quản lý kết nối Bluetooth và game logic.

```csharp
public class BluetoothGameManager : MonoBehaviour
{
    public static BluetoothGameManager Instance;
    
    public bool isHost;           // true = tạo phòng, false = tham gia
    public PlayerTurn myTurn;     // P1 (host) hoặc P2 (client)
}
```

#### Methods

| Method | Description |
|--------|-------------|
| `CreateGame()` | Tạo phòng, bắt đầu server |
| `JoinGame()` | Tìm phòng, bắt đầu scan |
| `ConnectToDevice(address)` | Kết nối đến thiết bị |
| `SendMove(cellIndex, direction)` | Gửi nước đi |
| `Disconnect()` | Ngắt kết nối |

#### Events (từ BluetoothHandler)

| Event | Parameters | Description |
|-------|------------|-------------|
| `ScanStartedAction` | - | Bắt đầu scan |
| `ScanDeviceFoundAction` | name, address | Tìm thấy thiết bị |
| `ScanFinishedAction` | - | Scan xong |
| `ConnectingAction` | address | Đang kết nối |
| `ConnectedAction` | address | Đã kết nối |
| `DisconnectedAction` | address | Mất kết nối |
| `DataReceivedAction` | byte[] | Nhận dữ liệu |
| `ErrorAction` | error | Có lỗi |

---

### BluetoothUI

UI cho Bluetooth menu.

```csharp
public class BluetoothUI : MonoBehaviour
{
    public static BluetoothUI Instance;
    
    public GameObject btMenuPanel;
    public GameObject deviceListPanel;
    public GameObject waitingPanel;
    public Transform deviceListContent;
    public GameObject deviceItemPrefab;
    public Text statusText;
}
```

#### Methods

| Method | Description |
|--------|-------------|
| `Show()` | Hiện BT menu |
| `HideAll()` | Ẩn tất cả panels |
| `OnCreateGame()` | Handler tạo phòng |
| `OnJoinGame()` | Handler tìm phòng |
| `OnCancel()` | Handler hủy |
| `AddDevice(name, address)` | Thêm thiết bị vào list |
| `OnConnected()` | Callback khi connected |
| `OnDisconnected()` | Callback khi disconnected |

---

### BluetoothData

Data classes cho Bluetooth.

```csharp
[Serializable]
public class MoveData
{
    public int cellIndex;    // 0-13
    public int direction;    // -1 hoặc 1
    public int turn;         // 0=P1, 1=P2
}

[Serializable]
public class DeviceInfo
{
    public string name;
    public string address;   // MAC address
}

public enum GameMode
{
    Local,
    Bluetooth
}
```

---

### BluetoothHandler (BlueUnity)

Native Bluetooth wrapper.

```csharp
public class BluetoothHandler
{
    public static BluetoothHandler Instance;
}
```

#### Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `SetDeviceName` | string name | Đặt tên thiết bị |
| `StartDiscoverable` | int seconds | Cho phép tìm thấy |
| `StartServer` | - | Bắt đầu server |
| `StartScan` | - | Bắt đầu scan |
| `StopScan` | - | Dừng scan |
| `ConnectAsClient` | string address | Kết nối đến server |
| `Write` | byte[] data | Gửi dữ liệu |
| `Disconnect` | - | Ngắt kết nối |

---

### UnityMainThreadDispatcher

Thread safety cho Unity.

```csharp
public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static UnityMainThreadDispatcher Instance();
    public void Enqueue(Action action);
}
```

#### Usage
```csharp
// Từ background thread (BT callback)
UnityMainThreadDispatcher.Instance().Enqueue(() =>
{
    // Code chạy trên main thread
    BluetoothUI.Instance.AddDevice(name, address);
});
```

---

## Usage Examples

### Tạo phòng
```csharp
public void OnCreateGameClicked()
{
    BluetoothGameManager.Instance.CreateGame();
    // UI sẽ hiện waiting panel
}
```

### Tìm phòng
```csharp
public void OnJoinGameClicked()
{
    BluetoothGameManager.Instance.JoinGame();
    // UI sẽ hiện device list
}
```

### Gửi nước đi
```csharp
// Trong GameManager.OnSelectDirection()
if (currentMode == GameMode.Bluetooth)
{
    BluetoothGameManager.Instance.SendMove(cellIndex, direction);
}
```

### Nhận nước đi
```csharp
// Trong BluetoothGameManager.OnDataReceived()
void OnDataReceived(byte[] data)
{
    string json = Encoding.UTF8.GetString(data);
    MoveData move = JsonUtility.FromJson<MoveData>(json);
    
    UnityMainThreadDispatcher.Instance().Enqueue(() =>
    {
        ExecuteOpponentMove(move);
    });
}
```

### Validate nước đi
```csharp
bool IsValidOpponentMove(MoveData move)
{
    // 1. Kiểm tra đúng lượt
    if (GameManager.instance._currentTurn == myTurn)
        return false;
    
    // 2. Kiểm tra cell index
    if (move.cellIndex < 0 || move.cellIndex >= 14)
        return false;
    
    // 3. Kiểm tra cell thuộc đối thủ
    PlayerTurn opponent = myTurn == P1 ? P2 : P1;
    if (opponent == P1 && move.cellIndex > 5)
        return false;
    if (opponent == P2 && move.cellIndex < 6)
        return false;
    
    // 4. Kiểm tra direction
    if (move.direction != -1 && move.direction != 1)
        return false;
    
    // 5. Kiểm tra có quân
    if (board[move.cellIndex] <= 0)
        return false;
    
    return true;
}
```

---

## Integration Points

### GameManager
```csharp
// Trong OnSelectDirection()
if (currentMode == GameMode.Bluetooth && BluetoothGameManager.Instance != null)
{
    BluetoothGameManager.Instance.SendMove(selectedCell, direction);
}

// Trong CanSelectCellBluetooth()
bool CanSelectCellBluetooth(int index)
{
    if (BluetoothGameManager.Instance == null)
        return true;
    
    // Kiểm tra đúng lượt
    if (_currentTurn != BluetoothGameManager.Instance.myTurn)
        return false;
    
    // Kiểm tra cell thuộc về mình
    // ...
    
    return true;
}
```

### LoginUI
```csharp
public void ShowBluetoothMenu()
{
    BluetoothUI.Instance?.Show();
}
```
