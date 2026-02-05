# 📋 Task List - Ô Quan Game

**Mục tiêu:** Upload lên Google Play Store trong 2 ngày  
**Cập nhật:** 2026-02-03

---

## 📊 Tổng Quan Trạng Thái

| Hạng mục | Hoàn thành | Tổng | % |
|----------|------------|------|---|
| Core Features | 8 | 8 | 100% |
| Store Requirements | 2 | 6 | 33% |
| Polish | 1 | 4 | 25% |

**Điểm đánh giá tổng: 7.5/10**

---

## ✅ ĐÃ HOÀN THÀNH

### Core Features
- [x] Gameplay Ô Ăn Quan đầy đủ
- [x] Chơi 2 người local
- [x] AI System (4 độ khó)
- [x] Gemini AI integration
- [x] Level & XP System
- [x] Achievement System
- [x] Daily Rewards
- [x] Tutorial
- [x] Sound System
- [x] Settings UI

### Documentation
- [x] README.md
- [x] Thông tin tác giả
- [x] Privacy Policy

---

## 🔴 CẦN LÀM NGAY (Ngày 1 - 03/02)

### Priority 1: Store Requirements (BẮT BUỘC)

| # | Task | Effort | Status |
|---|------|--------|--------|
| 1 | **App Icon 512x512** | 30 phút | ⬜ TODO |
| 2 | **Feature Graphic 1024x500** | 30 phút | ⬜ TODO |
| 3 | **Screenshots (5-8 ảnh)** | 1 giờ | ⬜ TODO |
| 4 | **Store Description (VI + EN)** | 1 giờ | ⬜ TODO |

### Priority 2: Build & Test

| # | Task | Effort | Status |
|---|------|--------|--------|
| 5 | **Build Release APK/AAB** | 30 phút | ⬜ TODO |
| 6 | **Test trên thiết bị thật** | 1 giờ | ⬜ TODO |
| 7 | **Fix crash/bug nếu có** | ? | ⬜ TODO |

---

## 🟡 CẦN LÀM (Ngày 2 - 04/02)

### Priority 3: Google Play Console

| # | Task | Effort | Status |
|---|------|--------|--------|
| 8 | Tạo app trên Play Console | 30 phút | ⬜ TODO |
| 9 | Upload AAB | 15 phút | ⬜ TODO |
| 10 | Điền Store Listing | 1 giờ | ⬜ TODO |
| 11 | Content Rating | 15 phút | ⬜ TODO |
| 12 | Data Safety Form | 30 phút | ⬜ TODO |
| 13 | Submit for Review | 15 phút | ⬜ TODO |

---

## 🟢 SAU KHI RELEASE (Optional)

### Polish
| Task | Priority | Effort |
|------|----------|--------|
| Loading screen đẹp hơn | Low | 2 giờ |
| Thêm animation | Low | 4 giờ |
| Optimize performance | Medium | 2 giờ |

### Bluetooth (70% done)
| Task | Priority | Effort |
|------|----------|--------|
| Test 2 máy thật | Medium | 2 giờ |
| Fix UI button Hủy | Medium | 1 giờ |
| Reconnection logic | Low | 2 giờ |

### Monetization
| Task | Priority | Effort |
|------|----------|--------|
| AdMob integration | Low | 4 giờ |
| Remove Ads IAP | Low | 2 giờ |

---

## 📝 CHI TIẾT TASK QUAN TRỌNG

### Task 1: App Icon 512x512

**File hiện tại:** `Assets/Art/icon.png`

**Yêu cầu:**
- PNG format
- 512x512 pixels
- Không có alpha/transparency
- Không có text nhỏ

**Action:**
```bash
# Resize icon nếu cần
sips -z 512 512 Assets/Art/icon.png --out PlayStoreAssets/icon_512.png
```

---

### Task 2: Feature Graphic 1024x500

**Yêu cầu:**
- PNG hoặc JPG
- 1024x500 pixels
- Hiển thị trên Play Store

**Gợi ý nội dung:**
- Logo game
- Hình bàn cờ
- Text "Ô Ăn Quan - Game Dân Gian Việt Nam"

---

### Task 3: Screenshots

**Yêu cầu:**
- Tối thiểu 2, tối đa 8
- Phone: 16:9 hoặc 9:16
- PNG hoặc JPG

**Cần chụp:**
1. Màn hình chính (Login)
2. Gameplay đang chơi
3. Chọn độ khó AI
4. Kết quả thắng
5. Daily Reward popup
6. Achievement popup
7. Settings
8. Tutorial

---

### Task 4: Store Description

**Short Description (80 chars):**
```
Ô Ăn Quan - Trò chơi dân gian Việt Nam. Chơi với AI hoặc bạn bè!
```

**Full Description (4000 chars):**
```
🎮 Ô ĂN QUAN - GAME DÂN GIAN VIỆT NAM

Ô Ăn Quan (Ô Quan) là trò chơi dân gian truyền thống của Việt Nam, 
nay được đưa lên điện thoại với đồ họa đẹp mắt và nhiều tính năng hấp dẫn!

✨ TÍNH NĂNG:
• Chơi 2 người trên 1 thiết bị
• Chơi với AI (4 độ khó: Dễ, Trung bình, Khó, Gemini AI)
• Hệ thống Level & XP
• 10+ Thành tựu để mở khóa
• Phần thưởng hàng ngày
• Hướng dẫn chi tiết cho người mới

🎯 CÁCH CHƠI:
1. Chọn một ô bên phía bạn
2. Chọn hướng rải quân (trái/phải)
3. Ăn quân đối thủ khi có cơ hội
4. Người có nhiều điểm hơn thắng!

📱 PHÙ HỢP MỌI LỨA TUỔI
Game không có quảng cáo, không thu thập dữ liệu cá nhân.
An toàn cho trẻ em.

🇻🇳 MADE IN VIETNAM
Phát triển bởi Jerry Nguyen
Website: lamgame.vn
```

---

### Task 5: Build Release

**Unity Settings:**
1. File → Build Settings
2. Platform: Android
3. Build App Bundle (AAB) ✓
4. Compression: LZ4HC
5. Scripting Backend: IL2CPP
6. Target Architectures: ARMv7 + ARM64

**Keystore:**
- Tạo keystore mới nếu chưa có
- LƯU GIỮ keystore cẩn thận (mất = không update được app)

---

## ⏰ TIMELINE

### Ngày 1 (03/02/2026)
| Thời gian | Task |
|-----------|------|
| 14:30-15:00 | App Icon |
| 15:00-15:30 | Feature Graphic |
| 15:30-16:30 | Screenshots |
| 16:30-17:30 | Store Description |
| 17:30-18:00 | Build Release |
| 18:00-19:00 | Test trên thiết bị |

### Ngày 2 (04/02/2026)
| Thời gian | Task |
|-----------|------|
| 09:00-10:00 | Tạo app Play Console |
| 10:00-11:00 | Upload & Store Listing |
| 11:00-11:30 | Content Rating & Data Safety |
| 11:30-12:00 | Submit for Review |

---

## 📌 LƯU Ý QUAN TRỌNG

### Google Play Requirements
- ✅ Target SDK 34+ (đã có)
- ✅ 64-bit support (IL2CPP)
- ✅ Privacy Policy URL (đã có)
- ⬜ App signing by Google Play

### Chi phí
- Google Play Developer: $25 (1 lần)
- Gemini API: ~$1-5/tháng (tùy usage)

### Thời gian review
- Thường 1-3 ngày làm việc
- App mới có thể lâu hơn

---

## 🎯 NEXT ACTIONS

1. **NGAY BÂY GIỜ:** Tạo App Icon 512x512
2. **Tiếp theo:** Feature Graphic
3. **Sau đó:** Chụp Screenshots

**Bắt đầu từ task nhỏ nhất, hoàn thành từng cái một!**
