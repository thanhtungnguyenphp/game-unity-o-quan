# 🎮 Game Ô Quan - Roadmap to Google Play

## 📊 Trạng thái hiện tại

| Chức năng | Trạng thái | Ghi chú |
|-----------|------------|---------|
| Gameplay cơ bản | ✅ Hoàn thành | Luật chơi đầy đủ |
| Chơi 2 người (local) | ✅ Hoàn thành | |
| Chơi với AI (Gemini) | ✅ Hoàn thành | Mới thêm |
| AI Offline (Minimax) | ✅ Hoàn thành | Fallback |
| Bluetooth multiplayer | ⚠️ 70% | UI xong, cần test thực tế |
| Âm thanh/Nhạc | ✅ Hoàn thành | |
| Hướng dẫn chơi | ✅ Hoàn thành | |
| Lưu/Load game | ❌ Chưa có | |
| Leaderboard | ❌ Chưa có | |
| Achievements | ❌ Chưa có | |
| Ads integration | ❌ Chưa có | |
| In-app purchase | ❌ Chưa có | |

**Điểm đánh giá: 7.4/10**

---

## 🚀 Roadmap phát triển

### Phase 1: Polish & Bug Fixes (3-5 ngày)
> Mục tiêu: Game ổn định, UX tốt

| Task | Priority | Effort |
|------|----------|--------|
| Fix Bluetooth UI (button Hủy) | High | 1 ngày |
| Test Bluetooth 2 máy thật | High | 1 ngày |
| Thêm Settings (âm lượng, độ khó AI) | Medium | 1 ngày |
| Loading screen đẹp hơn | Low | 0.5 ngày |
| App icon & splash screen | High | 0.5 ngày |

### Phase 2: Google Play Requirements (3-4 ngày)
> Mục tiêu: Đủ điều kiện publish

| Task | Priority | Effort |
|------|----------|--------|
| Privacy Policy page | Required | 0.5 ngày |
| Target SDK 34 (Android 14) | Required | 0.5 ngày |
| App signing setup | Required | 0.5 ngày |
| Store listing (screenshots, description) | Required | 1 ngày |
| Content rating questionnaire | Required | 0.5 ngày |
| Data safety form | Required | 0.5 ngày |

### Phase 3: Monetization (Optional, 3-5 ngày)
> Mục tiêu: Kiếm tiền từ game

| Task | Priority | Effort |
|------|----------|--------|
| AdMob integration (banner, interstitial) | Medium | 2 ngày |
| Remove ads IAP | Low | 1 ngày |
| Rewarded ads (hint, undo) | Low | 1 ngày |

### Phase 4: Engagement Features (5-7 ngày)
> Mục tiêu: Giữ chân người chơi

| Task | Priority | Effort |
|------|----------|--------|
| Google Play Games login | Medium | 2 ngày |
| Leaderboard (điểm cao) | Medium | 1 ngày |
| Achievements | Low | 2 ngày |
| Daily rewards | Low | 1 ngày |

---

## 📋 Checklist Google Play

### Bắt buộc
- [ ] App icon 512x512 PNG
- [ ] Feature graphic 1024x500
- [ ] Screenshots (phone + tablet)
- [ ] Short description (80 chars)
- [ ] Full description (4000 chars)
- [ ] Privacy policy URL
- [ ] Content rating
- [ ] Target audience
- [ ] Data safety declarations
- [ ] App signing by Google Play

### Kỹ thuật
- [ ] Target SDK 34+
- [ ] 64-bit support (ARM64)
- [ ] App Bundle (.aab) thay vì APK
- [ ] ProGuard/R8 enabled
- [ ] Remove debug logs
- [ ] Test trên nhiều thiết bị

### Tối ưu
- [ ] APK size < 150MB
- [ ] Startup time < 3s
- [ ] No ANR/Crash
- [ ] Battery efficient

---

## 🎯 Đề xuất chức năng tiếp theo

### Option A: Hoàn thiện Bluetooth (Recommended)
**Lý do:** Đã code 70%, chỉ cần fix UI và test
**Thời gian:** 2-3 ngày
**Giá trị:** Unique feature, chơi với bạn bè

### Option B: Settings & Polish
**Lý do:** Cải thiện UX, chuẩn bị release
**Thời gian:** 2-3 ngày
**Giá trị:** Professional feel

### Option C: Google Play Setup
**Lý do:** Có thể release sớm, iterate sau
**Thời gian:** 3-4 ngày
**Giá trị:** Có mặt trên store, nhận feedback

---

## 📅 Timeline đề xuất

```
Tuần 1: Phase 1 (Polish)
├── Ngày 1-2: Fix Bluetooth
├── Ngày 3: Settings screen
├── Ngày 4: App icon, splash
└── Ngày 5: Testing

Tuần 2: Phase 2 (Play Store)
├── Ngày 1: Privacy policy, SDK update
├── Ngày 2: Store assets (screenshots)
├── Ngày 3: Store listing
├── Ngày 4: Submit for review
└── Ngày 5-7: Wait for approval

Tuần 3+: Phase 3-4 (Post-launch)
├── Monitor crashes/reviews
├── Add monetization
└── Add engagement features
```

---

## 💰 Chi phí ước tính

| Item | Chi phí | Ghi chú |
|------|---------|---------|
| Google Play Developer | $25 (1 lần) | Bắt buộc |
| Gemini API | ~$1-5/tháng | Tùy lượng chơi |
| Domain (privacy policy) | $10-15/năm | Hoặc dùng GitHub Pages free |
| **Tổng khởi đầu** | **~$30** | |

---

## ❓ Câu hỏi cần quyết định

1. **Monetization strategy?**
   - Free with ads
   - Paid app ($0.99-1.99)
   - Free, no ads (hobby project)

2. **Target audience?**
   - Việt Nam only
   - Global (cần dịch tiếng Anh)

3. **Release timeline?**
   - ASAP (1-2 tuần)
   - Polished (3-4 tuần)

---

**Bạn muốn tiến hành theo hướng nào?**
