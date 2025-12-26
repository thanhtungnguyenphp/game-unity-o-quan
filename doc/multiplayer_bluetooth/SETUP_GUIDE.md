# 🔧 SETUP GUIDE - Bluetooth Multiplayer

## Yêu cầu

### Phần cứng
- 2 thiết bị Android
- Bluetooth 4.0+

### Phần mềm
- Unity 2022.3+
- Android SDK API 24+
- BlueUnity plugin (đã có sẵn)

---

## Bước 1: Kiểm tra Plugin

Đảm bảo các files sau tồn tại:
```
Assets/BlueUnity/
├── Plugins/Android/
│   ├── blueunity-release.aar    ✓
│   ├── BluetoothHandler.cs      ✓
│   ├── CallbackProxy.cs         ✓
│   └── AndroidManifest.xml      ✓
└── Scripts/
    └── BluetoothConnectionExample/
        └── BluetoothManagerExample.cs  ✓
```

---

## Bước 2: Setup UI trong Unity

### Cách 1: Tự động (Recommended)
1. Mở Unity Editor
2. Menu: **Game → Setup Bluetooth UI**
3. Script tự động tạo UI

### Cách 2: Thủ công
1. Trong Scene `GameScene`, tìm `Loading`
2. Tạo GameObject `BluetoothUI` với script `BluetoothUI.cs`
3. Tạo các panels:
   - `BTMenuPanel` - Menu chính
   - `DeviceListPanel` - Danh sách thiết bị
   - `WaitingPanel` - Màn hình chờ

---

## Bước 3: Thêm Button Bluetooth

Trong `Loading/bg`:

1. Duplicate button `play`
2. Đổi tên: `bluetooth`
3. Đổi text: "Bluetooth" hoặc "Chơi 2 máy"
4. Position: Dưới button `guidance`

LoginUI.cs sẽ tự động tìm button `bg/bluetooth`.

---

## Bước 4: Kiểm tra Permissions

File `Assets/Plugins/Android/AndroidManifest.xml` cần có:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    
    <!-- Bluetooth permissions -->
    <uses-permission android:name="android.permission.BLUETOOTH"/>
    <uses-permission android:name="android.permission.BLUETOOTH_ADMIN"/>
    
    <!-- Android 12+ permissions -->
    <uses-permission android:name="android.permission.BLUETOOTH_CONNECT"/>
    <uses-permission android:name="android.permission.BLUETOOTH_SCAN"/>
    <uses-permission android:name="android.permission.BLUETOOTH_ADVERTISE"/>
    
    <!-- Location (required for BT scan) -->
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION"/>
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION"/>
    
</manifest>
```

---

## Bước 5: Build Settings

### Player Settings
```
Edit → Project Settings → Player → Android

- Minimum API Level: 24 (Android 7.0)
- Target API Level: 34 (Android 14)
- Scripting Backend: IL2CPP
- Target Architectures: ARM64, ARMv7
```

### Build
```
File → Build Settings
- Platform: Android
- Build App Bundle: No (for testing)
- Build
```

---

## Bước 6: Test

### Trên 2 thiết bị

**Device A (Host):**
1. Mở app
2. Click "Bluetooth"
3. Click "Tạo phòng"
4. Chờ kết nối

**Device B (Client):**
1. Mở app
2. Click "Bluetooth"
3. Click "Tìm phòng"
4. Chọn "OQuanGame" từ danh sách
5. Chờ kết nối

**Khi connected:**
- Cả 2 tự động vào game
- Host = Player 1 (đi trước)
- Client = Player 2

---

## Checklist

- [ ] BlueUnity plugin có trong project
- [ ] BluetoothUI setup trong scene
- [ ] Button Bluetooth trong menu
- [ ] Permissions trong AndroidManifest
- [ ] Build settings đúng
- [ ] Test trên 2 thiết bị

---

## Tiếp theo

- [API Reference](API_REFERENCE.md) - Chi tiết code
- [Troubleshooting](TROUBLESHOOTING.md) - Xử lý lỗi
