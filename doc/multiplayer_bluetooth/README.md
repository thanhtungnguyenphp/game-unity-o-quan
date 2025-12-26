# 📱 BLUETOOTH MULTIPLAYER - TECHNICAL DOCUMENTATION

## 📋 Mục lục
1. [Tổng quan](#tổng-quan)
2. [Kiến trúc](#kiến-trúc)
3. [Setup Guide](SETUP_GUIDE.md)
4. [API Reference](API_REFERENCE.md)
5. [Protocol](PROTOCOL.md)
6. [Troubleshooting](TROUBLESHOOTING.md)

---

## Tổng quan

Chức năng Bluetooth Multiplayer cho phép 2 người chơi Ô Quan trên 2 thiết bị Android khác nhau thông qua kết nối Bluetooth.

### Tính năng
- ✅ Tạo phòng (Host)
- ✅ Tìm và tham gia phòng (Client)
- ✅ Đồng bộ nước đi real-time
- ✅ Validation chống gian lận
- ✅ Xử lý mất kết nối

### Yêu cầu
- Android 7.0+ (API 24)
- Bluetooth enabled
- Location permission (để scan)

---

## Kiến trúc

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                      GAME LAYER                              │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐         ┌─────────────┐                    │
│  │ GameManager │◄───────►│   GameState │                    │
│  │ (mode=BT)   │         │             │                    │
│  └──────┬──────┘         └─────────────┘                    │
│         │                                                    │
├─────────┼────────────────────────────────────────────────────┤
│         │           BLUETOOTH LAYER                          │
├─────────┼────────────────────────────────────────────────────┤
│         ▼                                                    │
│  ┌─────────────────┐    ┌─────────────┐                     │
│  │BluetoothGame    │◄──►│ BluetoothUI │                     │
│  │   Manager       │    │             │                     │
│  └────────┬────────┘    └─────────────┘                     │
│           │                                                  │
│           ▼                                                  │
│  ┌─────────────────┐                                        │
│  │BluetoothHandler │  (BlueUnity Plugin)                    │
│  │   (Native)      │                                        │
│  └─────────────────┘                                        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Component Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                     Scripts/Client/                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Core/                                                       │
│  ├── BluetoothGameManager.cs   # Main BT logic              │
│  ├── BluetoothUI.cs            # UI panels                  │
│  ├── UnityMainThreadDispatcher.cs  # Thread safety          │
│  ├── GameManager.cs            # Game coordinator           │
│  └── LoginUI.cs                # Menu with BT button        │
│                                                              │
│  Data/                                                       │
│  └── BluetoothData.cs          # MoveData, DeviceInfo       │
│                                                              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                     BlueUnity/                               │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Plugins/Android/                                            │
│  ├── blueunity-release.aar     # Native Android library     │
│  ├── BluetoothHandler.cs       # Unity wrapper              │
│  ├── CallbackProxy.cs          # JNI callbacks              │
│  └── AndroidManifest.xml       # Permissions                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Sequence Diagram - Connection
```
    Host                    BlueUnity                   Client
      │                         │                          │
      │  CreateGame()           │                          │
      │────────────────────────►│                          │
      │                         │                          │
      │  StartServer()          │                          │
      │  StartDiscoverable()    │                          │
      │                         │                          │
      │                         │         JoinGame()       │
      │                         │◄─────────────────────────│
      │                         │                          │
      │                         │         StartScan()      │
      │                         │◄─────────────────────────│
      │                         │                          │
      │                         │    OnDeviceFound()       │
      │                         │─────────────────────────►│
      │                         │                          │
      │                         │    ConnectAsClient()     │
      │                         │◄─────────────────────────│
      │                         │                          │
      │    OnConnected()        │      OnConnected()       │
      │◄────────────────────────│─────────────────────────►│
      │                         │                          │
      │  StartGame()            │      StartGame()         │
      │  (myTurn=P1)            │      (myTurn=P2)         │
      │                         │                          │
```

### Sequence Diagram - Gameplay
```
    Player1 (Host)          Bluetooth              Player2 (Client)
         │                      │                         │
         │  OnSelectCell(3)     │                         │
         │  OnSelectDirection(1)│                         │
         │                      │                         │
         │  SendMove(3, 1)      │                         │
         │─────────────────────►│                         │
         │                      │    OnDataReceived()     │
         │                      │────────────────────────►│
         │                      │                         │
         │                      │    ValidateMove()       │
         │                      │    ExecuteMove(3, 1)    │
         │                      │                         │
         │                      │                         │
         │                      │    OnSelectCell(8)      │
         │                      │    OnSelectDirection(-1)│
         │                      │                         │
         │                      │    SendMove(8, -1)      │
         │    OnDataReceived()  │◄────────────────────────│
         │◄─────────────────────│                         │
         │                      │                         │
         │  ValidateMove()      │                         │
         │  ExecuteMove(8, -1)  │                         │
         │                      │                         │
```

---

## Files Structure

```
game-unity-o-quan/
├── Assets/
│   ├── Scripts/Client/
│   │   ├── Core/
│   │   │   ├── BluetoothGameManager.cs
│   │   │   ├── BluetoothUI.cs
│   │   │   ├── UnityMainThreadDispatcher.cs
│   │   │   ├── GameManager.cs
│   │   │   └── LoginUI.cs
│   │   └── Data/
│   │       └── BluetoothData.cs
│   │
│   ├── BlueUnity/
│   │   ├── Plugins/Android/
│   │   │   ├── blueunity-release.aar
│   │   │   ├── BluetoothHandler.cs
│   │   │   ├── CallbackProxy.cs
│   │   │   └── AndroidManifest.xml
│   │   └── Scripts/
│   │       └── BluetoothConnectionExample/
│   │
│   └── Editor/
│       └── BluetoothUISetup.cs
│
└── doc/
    └── multiplayer_bluetooth/
        ├── README.md (this file)
        ├── SETUP_GUIDE.md
        ├── API_REFERENCE.md
        ├── PROTOCOL.md
        └── TROUBLESHOOTING.md
```

---

## Quick Links

- [Setup Guide](SETUP_GUIDE.md) - Hướng dẫn cài đặt
- [API Reference](API_REFERENCE.md) - Chi tiết API
- [Protocol](PROTOCOL.md) - Giao thức truyền dữ liệu
- [Troubleshooting](TROUBLESHOOTING.md) - Xử lý lỗi

---

**Version:** 1.0  
**Last Updated:** 2025-12-26
