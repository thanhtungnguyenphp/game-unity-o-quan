# 🎮 Ô Quan - Tài Liệu Tổng Quan Game

**Tác giả:** Jerry Nguyen  
**Email:** jerry06082018@gmail.com  
**Website:** https://lamgame.vn/source-game/game-o-quan  
**Cập nhật:** 2026-02-03

---

## 📋 Mục Lục

1. [Tổng Quan](#tổng-quan)
2. [Kiến Trúc Code](#kiến-trúc-code)
3. [Chức Năng Chính](#chức-năng-chính)
4. [Luật Chơi](#luật-chơi)
5. [Cấu Trúc Thư Mục](#cấu-trúc-thư-mục)

---

## Tổng Quan

**Ô Quan (Ô Ăn Quan)** là game dân gian truyền thống Việt Nam được phát triển trên Unity.

### Thông Tin Kỹ Thuật

| Thuộc tính | Giá trị |
|------------|---------|
| Engine | Unity 2022.3.62f3 |
| Platform | Android (SDK 34+) |
| Language | C# |
| Architecture | Component-based + Manager Pattern |

### Các Chế Độ Chơi

1. **Local 2 Players** - Chơi 2 người trên 1 thiết bị
2. **vs AI** - Chơi với máy (4 độ khó)
3. **Bluetooth Multiplayer** - Chơi 2 máy qua Bluetooth

---

## Kiến Trúc Code

### Core Managers

```
GameManager (Singleton)
├── BoardManager      - Quản lý bàn cờ
├── TurnManager       - Quản lý lượt chơi
├── ScoreManager      - Quản lý điểm số
├── RuleEngine        - Xử lý luật chơi
├── MoveHandler       - Xử lý nước đi
├── AnimationController - Animation rải quân
└── UIControl         - Điều khiển UI
```

### Support Systems

```
AIManager             - Quản lý AI (4 độ khó)
BluetoothGameManager  - Multiplayer Bluetooth
LevelSystem           - Hệ thống cấp độ & XP
AchievementManager    - Thành tựu
DailyRewardManager    - Phần thưởng hàng ngày
CurrencyManager       - Quản lý coins/gems
SoundManager          - Âm thanh
TutorialManager       - Hướng dẫn chơi
```

### Design Patterns

- **Singleton** - GameManager, AIManager, SoundManager...
- **State Machine** - States: SelectingCell → SelectingDirection → Animating → GameOver
- **Strategy** - IAIPlayer interface cho các AI khác nhau
- **Observer** - Events cho Level Up, Achievement Unlock...

---

## Chức Năng Chính

### 1. Gameplay Core ✅
- Luật chơi Ô Ăn Quan đầy đủ
- Rải quân, ăn quân, tính điểm
- Xử lý trường hợp đặc biệt (hết quân, vay quân)

### 2. AI System ✅
| Độ khó | Algorithm | Mô tả |
|--------|-----------|-------|
| Easy | RandomAI | Chọn ngẫu nhiên |
| Medium | GreedyAI | Chọn nước ăn nhiều nhất |
| Hard | MinimaxAI | Minimax với alpha-beta pruning |
| Gemini | GeminiAI | Google Gemini API (cloud) |

### 3. Bluetooth Multiplayer ⚠️ 70%
- Kết nối Host/Client
- Đồng bộ nước đi
- Cần test thực tế 2 máy

### 4. Level & XP System ✅
- 100 cấp độ
- XP tăng theo ván chơi (thắng: 50XP, thua: 10XP)
- Phần thưởng coins khi lên cấp

### 5. Achievement System ✅
- 10 thành tựu
- Theo dõi tiến độ
- Phần thưởng coins/gems

### 6. Daily Rewards ✅
- 7 ngày liên tiếp
- Reset streak nếu bỏ lỡ 48h
- Phần thưởng tăng dần

### 7. Tutorial ✅
- 5 bước hướng dẫn
- Hiển thị lần đầu chơi
- Có thể skip

### 8. Sound System ✅
- BGM với fade in/out
- SFX cho các action
- Lưu settings âm lượng

---

## Luật Chơi

### Bàn Cờ
```
        [QUAN 2 - Ô 11]
    [10] [9] [8] [7] [6]     ← Player 2
    [0]  [1] [2] [3] [4]     ← Player 1
        [QUAN 1 - Ô 5]
```

### Điểm Số
- **Quân Dân**: 1 điểm
- **Quân Lớn (Quan)**: 10 điểm
- Mỗi ô bắt đầu: 5 dân
- Mỗi ô Quan: 1 quan

### Cách Chơi
1. Chọn 1 ô bên mình (có quân)
2. Chọn hướng rải (trái/phải)
3. Rải từng quân vào các ô tiếp theo
4. Nếu ô cuối trống → ăn ô tiếp theo (nếu có quân)
5. Tiếp tục ăn cho đến khi gặp ô trống liên tiếp hoặc ô Quan

### Kết Thúc
- Cả 2 ô Quan đều bị ăn
- Hoặc tất cả ô dân đều trống

---

## Cấu Trúc Thư Mục

```
Assets/
├── Scripts/
│   └── Client/
│       ├── Core/           # GameManager, BoardManager, UI...
│       ├── AI/             # AIManager, MinimaxAI, GeminiAI...
│       ├── Data/           # Data classes
│       ├── State/          # Game states
│       ├── Services/       # Analytics, Firebase
│       ├── UI/             # UI components
│       └── Other/          # Utilities, Constants
├── Prefabs/                # Game objects
├── Scenes/                 # Unity scenes
├── Art/                    # Sprites, images
├── Audio/                  # Sound files
└── Editor/                 # Editor scripts
```

---

## Xem Thêm

- [FEATURES.md](FEATURES.md) - Chi tiết tính năng
- [TASKS.md](TASKS.md) - Danh sách task và trạng thái
- [../play_ai/README.md](../play_ai/README.md) - Tài liệu Gemini AI
