# 🔧 TROUBLESHOOTING - Bluetooth Multiplayer

## Common Issues

### 1. Không tìm thấy thiết bị

**Triệu chứng:**
- Scan không hiện thiết bị nào
- Device list trống

**Nguyên nhân & Giải pháp:**

| Nguyên nhân | Giải pháp |
|-------------|-----------|
| Bluetooth tắt | Bật Bluetooth trong Settings |
| Location tắt | Bật Location (cần cho BT scan) |
| Thiếu permission | Cấp quyền Location cho app |
| Host chưa discoverable | Host cần click "Tạo phòng" trước |
| Khoảng cách xa | Đến gần hơn (<10m) |

**Debug:**
```
adb logcat -s Unity | grep -i "bluetooth\|scan"
```

---

### 2. Kết nối thất bại

**Triệu chứng:**
- Click thiết bị nhưng không connect
- Timeout hoặc error

**Nguyên nhân & Giải pháp:**

| Nguyên nhân | Giải pháp |
|-------------|-----------|
| Host đã disconnect | Host tạo phòng lại |
| Đã paired với thiết bị khác | Unpair trong Settings |
| Bluetooth busy | Restart Bluetooth |
| App crash | Restart app cả 2 bên |

**Debug:**
```
adb logcat -s Unity | grep -i "connect\|error"
```

---

### 3. Mất kết nối giữa game

**Triệu chứng:**
- Game đột ngột dừng
- Thông báo "Mất kết nối"

**Nguyên nhân & Giải pháp:**

| Nguyên nhân | Giải pháp |
|-------------|-----------|
| Khoảng cách xa | Đến gần hơn |
| Interference | Tránh nhiều thiết bị BT |
| Battery saver | Tắt battery optimization cho app |
| App bị kill | Giữ app foreground |

**Recovery:**
- Game tự động chuyển về Local mode
- Có thể tiếp tục chơi offline

---

### 4. Move không sync

**Triệu chứng:**
- Đi xong nhưng đối thủ không thấy
- Board state khác nhau

**Nguyên nhân & Giải pháp:**

| Nguyên nhân | Giải pháp |
|-------------|-----------|
| Connection lost | Kiểm tra kết nối |
| Validation failed | Kiểm tra logs |
| Race condition | Đợi animation xong |

**Debug:**
```
adb logcat -s Unity | grep -i "send\|receive\|move"
```

---

### 5. UI không hiện

**Triệu chứng:**
- Click Bluetooth nhưng không có gì
- Panels không hiện

**Nguyên nhân & Giải pháp:**

| Nguyên nhân | Giải pháp |
|-------------|-----------|
| BluetoothUI chưa setup | Chạy Game → Setup Bluetooth UI |
| Button chưa link | Kiểm tra LoginUI.cs |
| Panel bị disable | Kiểm tra hierarchy |

---

## Debug Commands

### Xem logs
```bash
# Tất cả Unity logs
adb logcat -s Unity

# Chỉ Bluetooth logs
adb logcat -s Unity | grep -i bluetooth

# Chỉ connection logs
adb logcat -s Unity | grep -i "connect\|disconnect"

# Chỉ data logs
adb logcat -s Unity | grep -i "send\|receive\|data"
```

### Clear logs
```bash
adb logcat -c
```

### Restart Bluetooth
```bash
adb shell svc bluetooth disable
adb shell svc bluetooth enable
```

---

## Log Messages

### Normal Flow
```
🔧 Initializing Bluetooth...
✅ Bluetooth initialized
🔵 Creating Bluetooth game...        # Host
🔍 Joining Bluetooth game...         # Client
📱 Found: OQuanGame (XX:XX:XX:XX)    # Client
🔌 Connecting to XX:XX:XX:XX...      # Client
✅ Connected to XX:XX:XX:XX!         # Both
📤 Sent: Cell 3, Direction 1         # Sender
📥 Received: {"cellIndex":3...}      # Receiver
```

### Error Messages
```
❌ BluetoothHandler not initialized!
❌ Bluetooth error: Connection refused
❌ Invalid opponent move received
⚠️ Not your turn!
⚠️ Connection lost! Switching to local mode.
```

---

## Checklist khi có lỗi

### Trước khi test
- [ ] Bluetooth ON trên cả 2 thiết bị
- [ ] Location ON trên cả 2 thiết bị
- [ ] App có quyền Location
- [ ] Khoảng cách < 10m
- [ ] Không có thiết bị BT khác đang connect

### Khi test
- [ ] Host tạo phòng TRƯỚC
- [ ] Client tìm phòng SAU
- [ ] Đợi "Connected" trước khi chơi
- [ ] Không thoát app giữa chừng

### Khi có lỗi
- [ ] Kiểm tra logs: `adb logcat -s Unity`
- [ ] Restart app cả 2 bên
- [ ] Restart Bluetooth
- [ ] Thử lại từ đầu

---

## FAQ

**Q: Có thể chơi qua WiFi không?**
A: Hiện tại chỉ hỗ trợ Bluetooth. WiFi cần implement riêng.

**Q: Tối đa bao nhiêu người?**
A: 2 người (1 Host + 1 Client).

**Q: Có thể reconnect không?**
A: Chưa implement. Nếu mất kết nối, cần tạo game mới.

**Q: Hoạt động trên iOS không?**
A: Không. BlueUnity chỉ hỗ trợ Android.

**Q: Khoảng cách tối đa?**
A: ~10m (Bluetooth Classic). Có thể xa hơn nếu không có vật cản.

---

## Contact

Nếu vẫn gặp lỗi, cung cấp:
1. Device model
2. Android version
3. Full logs (`adb logcat -s Unity`)
4. Steps to reproduce
