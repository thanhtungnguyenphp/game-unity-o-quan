# Unity Version Update Guide
## Fixing Security Alert: 2022.3.29f1 → 2022.3.62f3

---

## ✅ **CÓ GÌ ĐÃ LÀM**

### 1. Đã kiểm tra và backup:
- ✅ Git status checked
- ✅ All changes committed to git
- ✅ Backup script created

### 2. Phát hiện:
- ✅ Unity 2022.3.62f3 đã được cài đặt tại:
  ```
  /Applications/Unity/Hub/Editor/2022.3.62f3/
  ```
- ✅ Project hiện tại đang dùng: 2022.3.29f1 (có security alert)

---

## 📋 **CẦN LÀM TIẾP**

### **Bước 1: Đóng Unity Editor** (QUAN TRỌNG!)

1. Lưu tất cả thay đổi trong Unity
2. **File → Exit** hoặc **Cmd+Q**
3. Đợi Unity đóng hoàn toàn

---

### **Bước 2: Chạy Update Script**

Mở Terminal và chạy:

```bash
cd /Users/Shared/jerry/game_unity/game-unity-o-quan
./update_unity_version.sh
```

**Script sẽ tự động:**
- ✅ Tạo backup project
- ✅ Update ProjectVersion.txt
- ✅ Xóa Library folder (để rebuild)
- ✅ Commit changes vào git

---

### **Bước 3: Mở Project với Unity 2022.3.62f3**

#### **Cách 1: Qua Unity Hub (Khuyến nghị)**

1. Mở **Unity Hub**
2. Tìm project "game-unity-o-quan"
3. Click vào **Unity Version** (hiện đang là 2022.3.29f1)
4. Chọn **2022.3.62f3** từ dropdown
5. Click **Open**

#### **Cách 2: Mở trực tiếp**

```bash
open -a /Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app \
     /Users/Shared/jerry/game_unity/game-unity-o-quan
```

---

### **Bước 4: Đợi Unity Reimport Assets**

Unity sẽ:
1. Phát hiện version mới
2. Hỏi "Upgrade project?" → Click **Proceed**
3. Rebuild Library folder (5-10 phút)
4. Reimport all assets

**Tiến trình hiển thị:**
```
Importing Assets...
Compiling Scripts...
Refreshing Assets...
```

---

### **Bước 5: Kiểm Tra Sau Update**

#### **Check 1: Console Window**
```
Window → General → Console
```
**Mong đợi:**
- ✅ Không có error đỏ
- ⚠️ Có thể có vài warning vàng (bình thường)

#### **Check 2: Security Alert**
- ✅ Security Alert ở Unity Hub **BIẾN MẤT**
- ✅ Version hiện: **2022.3.62f3**

#### **Check 3: Test Game**
1. Click **Play** button
2. Test game chạy bình thường
3. Test Bluetooth multiplayer features

#### **Check 4: Build APK**
```
Build → Build Android APK
```
Đảm bảo build thành công

---

## 🔍 **TROUBLESHOOTING**

### **Lỗi: "Library corrupt"**
**Fix:**
```bash
cd /Users/Shared/jerry/game_unity/game-unity-o-quan
rm -rf Library/
# Mở lại Unity, sẽ rebuild
```

### **Lỗi: "Compilation errors"**
**Check:**
```
Window → Package Manager
```
Update các packages có cảnh báo

### **Lỗi: "Missing scripts"**
**Fix:**
1. Kiểm tra Console log
2. Find missing script references
3. Re-assign scripts nếu cần

---

## 📊 **COMPARISON: BEFORE vs AFTER**

### **Before (2022.3.29f1):**
```
⚠️  Security Alert
⚠️  Known vulnerabilities
⚠️  Outdated patches
```

### **After (2022.3.62f3):**
```
✅ No security alert
✅ 33 bug fixes applied
✅ Latest security patches
✅ Better Android build support
```

---

## 🎯 **EXPECTED RESULTS**

### **Successful Update:**
```
Unity Hub:
  Project: game-unity-o-quan
  Version: 2022.3.62f3 ✅
  Status: No security alert ✅
  
Console:
  0 Errors ✅
  Some warnings (OK)
  
Build:
  Android APK builds successfully ✅
```

---

## 💾 **BACKUP INFORMATION**

### **Git Backup:**
```bash
# View backup commit
git log -1

# Revert if needed
git revert HEAD
```

### **File Backup:**
Location: `/Users/Shared/jerry/game_unity/`
Format: `game-unity-o-quan-backup-YYYYMMDD-HHMMSS.tar.gz`

**Restore backup:**
```bash
cd /Users/Shared/jerry/game_unity/
tar -xzf game-unity-o-quan-backup-XXXXXX.tar.gz
```

---

## 🚀 **QUICK START**

**TL;DR Version:**

```bash
# 1. Close Unity
# 2. Run update script
cd /Users/Shared/jerry/game_unity/game-unity-o-quan
./update_unity_version.sh

# 3. Open with Unity Hub → Select 2022.3.62f3
# 4. Wait for reimport
# 5. Done!
```

---

## 📝 **CHECKLIST**

- [ ] Unity Editor đã đóng
- [ ] Chạy `update_unity_version.sh` thành công
- [ ] Mở project với Unity 2022.3.62f3
- [ ] Đợi reimport assets hoàn tất
- [ ] Check Console - không có error
- [ ] Test game Play mode
- [ ] Test Build APK
- [ ] Security alert biến mất ✅

---

## 🎉 **KHI HOÀN THÀNH**

Push changes to git:

```bash
cd /Users/Shared/jerry/game_unity/game-unity-o-quan
git push origin feat/MutiplayerV2
```

---

## 📞 **HỖ TRỢ**

Nếu gặp vấn đề:

1. Check Console log
2. View Editor.log:
   ```bash
   tail -f ~/Library/Logs/Unity/Editor.log
   ```
3. Restore backup nếu cần

---

**Version:** 1.0  
**Date:** January 10, 2026  
**Status:** Ready to Execute
