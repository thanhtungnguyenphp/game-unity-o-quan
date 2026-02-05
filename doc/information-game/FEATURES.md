# 📋 Chi Tiết Tính Năng - Ô Quan

**Cập nhật:** 2026-02-03

---

## 1. Gameplay Core

### BoardManager
- Quản lý mảng `board[12]` - trạng thái bàn cờ
- Khởi tạo: 5 dân/ô, 1 quan/ô quan
- Xử lý ăn quân, reset board

### RuleEngine
- Validate nước đi hợp lệ
- Kiểm tra game over
- Xác định người thắng

### MoveHandler
- Thực thi nước đi với animation
- Rải quân tuần tự
- Xử lý ăn quân liên tiếp

### TurnManager
- State machine: SelectingCell → SelectingDirection → Animating → GameOver
- Quản lý lượt P1/P2
- Lưu cell và direction đã chọn

---

## 2. AI System

### Interface
```csharp
public interface IAIPlayer
{
    (int cellIndex, int direction) MakeMove(
        int[] board, 
        PlayerTurn turn, 
        bool quan1Available, 
        bool quan2Available
    );
}
```

### Implementations

| Class | Algorithm | Độ phức tạp |
|-------|-----------|-------------|
| RandomAI | Random valid move | O(1) |
| GreedyAI | Max immediate score | O(n) |
| MinimaxAI | Minimax + Alpha-Beta | O(b^d) |
| GeminiAI | Cloud API call | Network latency |

### GeminiAI
- Sử dụng Google Gemini 1.5 Flash
- Prompt engineering cho game context
- Fallback về MinimaxAI nếu offline

---

## 3. Bluetooth Multiplayer

### Flow
```
Host                          Client
  │                              │
  ├─ CreateGame()                │
  │   └─ StartServer()           │
  │                              ├─ JoinGame()
  │                              │   └─ StartScan()
  │◄─────── Connect ────────────►│
  │                              │
  ├─ SendHandshake() ───────────►│
  │◄─────── SendHandshake() ─────┤
  │                              │
  │         Game Start           │
  │                              │
  ├─ SendMove(cell, dir) ───────►│
  │◄─────── SendMove() ──────────┤
```

### BluetoothGameManager
- Host: P1, Client: P2
- Handshake protocol
- Move synchronization
- Reconnection support

---

## 4. Level System

### XP Rewards
| Action | XP |
|--------|-----|
| Thắng | 50 |
| Thua | 10 |
| Điểm cao (>40) | +20 |

### Level Formula
```
XP cần = 100 + (level - 1) * 50
```

### Milestone Rewards
- Mỗi level: `level * 10` coins
- Mỗi 5 level: +100 coins bonus
- Level 5, 10, 15: Unlock skin

---

## 5. Achievement System

| ID | Tên | Mô tả | Target | Coins |
|----|-----|-------|--------|-------|
| FirstWin | First Win | Thắng ván đầu | 1 | 50 |
| QuickLearner | Quick Learner | Hoàn thành tutorial | 1 | 100 |
| StoneCollector | Stone Collector | Ăn 100 dân | 100 | 200 |
| QuanHunter | Quan Hunter | Ăn 10 quan | 10 | 300 |
| WinningStreak | Winning Streak | Thắng 5 liên tiếp | 5 | 500 |
| PerfectGame | Perfect Game | Thắng với 50+ điểm | 1 | 300 |
| SpeedDemon | Speed Demon | Thắng trong 2 phút | 1 | 400 |
| ComebackKing | Comeback King | Comeback từ -20 điểm | 1 | 500 |
| MasterPlayer | Master Player | Thắng 100 ván | 100 | 1000 |
| AIDestroyer | AI Destroyer | Thắng Hard AI 20 lần | 20 | 800 |

---

## 6. Daily Rewards

| Ngày | Coins | Gems | Special |
|------|-------|------|---------|
| 1 | 50 | - | - |
| 2 | 75 | - | - |
| 3 | 100 | - | Hint x1 |
| 4 | 150 | - | - |
| 5 | 200 | - | Undo x1 |
| 6 | 250 | - | - |
| 7 | 500 | 10 | Special Skin |

- Reset streak nếu bỏ lỡ >48h
- Sau ngày 7 quay lại ngày 1

---

## 7. Sound System

### BGM
- Loading screen music
- In-game music
- Fade in/out transition

### SFX
| Event | Sound |
|-------|-------|
| Click | sfxClick |
| Move | sfxMove |
| Eat | sfxEat |
| Start | sfxStart |
| End | sfxEnd |

### Settings
- Master volume
- Music volume (on/off)
- SFX volume (on/off)
- Lưu vào PlayerPrefs

---

## 8. Tutorial

### Steps
1. Giới thiệu game
2. Giải thích mục tiêu & điểm số
3. Hướng dẫn chọn ô
4. Hướng dẫn chọn hướng
5. Giải thích cách ăn quân

### Features
- Highlight UI elements
- Skip button
- Chỉ hiện lần đầu (PlayerPrefs)

---

## 9. UI Components

### Main Screens
- **LoginUI** - Màn hình chính, chọn mode
- **GameScene** - Màn hình chơi game
- **EndGameUI** - Kết quả, play again
- **SettingUI** - Cài đặt âm thanh, độ khó
- **PauseUI** - Tạm dừng game

### Game UI
- **UIControl** - Điều khiển UI chính
- **CellUIControl** - UI từng ô
- **TurnIndicatorUI** - Hiển thị lượt
- **MultiplayerUI** - UI Bluetooth mode
- **DailyRewardUI** - Popup daily reward
- **AchievementPopup** - Popup achievement

---

## 10. Data Persistence

### PlayerPrefs Keys
```
PlayerLevel, PlayerXP, PlayerTotalXP
Coins, Gems
AIDifficulty
MusicVolume, SFXVolume, MusicEnabled, SFXEnabled
TutorialCompleted
DailyReward_Streak, DailyReward_LastClaim
Achievement_{Type}_Unlocked, Achievement_{Type}_Progress
```

### No Cloud Save
- Tất cả data lưu local
- Mất khi uninstall app
