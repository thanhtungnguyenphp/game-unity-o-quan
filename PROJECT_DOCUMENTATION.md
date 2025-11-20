# Tài Liệu Dự Án Game Ô Quan (Unity)

## 📋 Thông Tin Tổng Quan

### Thông Tin Cơ Bản
- **Tên dự án**: Ô Kỳ Quan (Ô Quan)
- **Engine**: Unity 2022.3.29f1
- **Platform**: Android
- **Thể loại**: Game truyền thống Việt Nam
- **Ngôn ngữ lập trình**: C#
- **Package ID**: com.lamgame.oquan

### Mô Tả Game
Game Ô Quan là một trò chơi dân gian truyền thống của Việt Nam, được phát triển trên nền tảng Unity cho thiết bị Android. Game mô phỏng trò chơi ô quan với đầy đủ luật chơi truyền thống.

## 🏗️ Cấu Trúc Dự Án

### Thư Mục Chính
```
game-unity-o-quan/
├── Assets/
│   ├── Art/                    # Tài nguyên hình ảnh
│   ├── Audio/                  # Tài nguyên âm thanh
│   ├── Editor/                 # Scripts build
│   ├── Font/                   # Font chữ
│   ├── GameSDK/               # Game State Machine
│   ├── Layer Lab/             # UI Assets
│   ├── Prefabs/               # Prefabs Unity
│   ├── Resources/             # Resources
│   ├── Scenes/                # Scenes Unity
│   └── Scripts/               # Source code
├── Build/                     # Thư mục build output
├── Packages/                  # Unity packages
├── ProjectSettings/           # Cài đặt Unity
├── build_and_install.sh       # Script build & install
└── build_apk.py              # Python build script
```

### Scripts Architecture
```
Scripts/
└── Client/
    ├── Core/                  # Logic game chính
    │   ├── GameMagager.cs     # Quản lý game chính
    │   ├── BoardManager.cs    # Quản lý bàn cờ
    │   ├── PlayerControl.cs   # Điều khiển người chơi
    │   ├── UIControl.cs       # Điều khiển UI
    │   ├── SoundManager.cs    # Quản lý âm thanh
    │   └── EndGameUI.cs       # UI kết thúc game
    ├── Data/                  # Dữ liệu game
    │   ├── MasterData.cs      # Dữ liệu cấu hình
    │   └── PlayerData.cs      # Dữ liệu người chơi
    ├── Other/                 # Utilities
    │   ├── Config.cs          # Cấu hình
    │   ├── VFXControl.cs      # Hiệu ứng
    │   └── SerializeDictionary.cs
    └── State/                 # Game States
        ├── GameState.cs       # State chơi game
        └── LoadingState.cs    # State loading
```

## 🎮 Luật Chơi & Logic Game

### Cơ Bản
- **Số người chơi**: 2 người
- **Bàn cờ**: 12 ô (10 ô dân + 2 ô quan)
- **Quân cờ**: Dân (1 điểm) và Quan (10 điểm)

### Trạng Thái Game
```csharp
public enum States
{
    SelectingCell,      // Chọn ô
    SelectingDirection, // Chọn hướng
    Animating,         // Đang di chuyển
    GameOver           // Kết thúc
}

public enum PlayerTurn
{
    P1,  // Người chơi 1
    P2   // Người chơi 2
}
```

### Luật Chơi Chính
1. **Rải quân**: Lấy tất cả quân trong ô đã chọn, rải từng quân theo hướng đã chọn
2. **Ăn quân**: Khi gặp ô trống, ăn quân ở ô tiếp theo
3. **Ăn dây**: Tiếp tục ăn nếu ô sau ô vừa ăn cũng trống
4. **Mất dân**: Khi hết quân, phải trả 5 quân để tiếp tục
5. **Trả nợ**: Tự động trả nợ khi có điểm

### Điều Kiện Kết Thúc
- Hết tất cả quan (ô 5 và 11)
- Hết tất cả dân trên bàn
- Không đủ quân để trả nợ

## 🔧 Các Component Chính

### 1. GameManager (GameMagager.cs)
**Chức năng**: Quản lý toàn bộ logic game
```csharp
public class GameMagager : MonoBehaviour
{
    // Singleton pattern
    public static GameMagager instance { get; private set; }
    
    // Core components
    BoardManager _boardManager;
    UIControl _uiController;
    EndGameUI _endGameUI;
    
    // Game state
    public PlayerTurn _currentTurn = PlayerTurn.P1;
    public States _currentState = States.SelectingCell;
    
    // Player scores
    int _p1Score, _p2Score;
    int _p1StoneCount, _p2StoneCount;
    int _p1Owed, _p2Owed;
}
```

**Phương thức chính**:
- `OnSelectCell(int index)`: Xử lý chọn ô
- `OnSelectDirection(int dir)`: Xử lý chọn hướng
- `HandleTurn()`: Xử lý lượt chơi
- `CaptureChain(int emptyPos)`: Xử lý ăn dây
- `CheckGameOver()`: Kiểm tra kết thúc game

### 2. BoardManager (BoardManager.cs)
**Chức năng**: Quản lý trạng thái bàn cờ
```csharp
public class BoardManager : MonoBehaviour
{
    public int[] board = new int[12];  // Mảng 12 ô
    bool _quan1, _quan2;               // Trạng thái 2 ô quan
    
    // Prefabs
    public GameObject _prefabDanA;
    public GameObject _prefabDanB;
    public GameObject _prefabQuan;
}
```

**Phương thức chính**:
- `Initialize()`: Khởi tạo bàn cờ
- `IsPlayerCell(int index, PlayerTurn turn)`: Kiểm tra ô của người chơi
- `IsQuan(int idx)`: Kiểm tra ô quan
- `GetPoint(int idx)`: Tính điểm của ô
- `EatStone(int idx)`: Ăn quân tại ô

### 3. UIControl (UIControl.cs)
**Chức năng**: Quản lý giao diện người dùng
- Cập nhật hiển thị bàn cờ
- Hiển thị điểm số và trạng thái
- Quản lý hiệu ứng visual

### 4. SoundManager (SoundManager.cs)
**Chức năng**: Quản lý âm thanh
```csharp
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    
    // Audio sources
    AudioSource musicSource;
    AudioSource sfxSource;
    
    // Audio clips
    public AudioClip[] musicClips;
    public AudioClip[] sfxClips;
}
```

**Âm thanh có sẵn**:
- `sfxClick.wav`: Âm thanh click
- `sfxMove.wav`: Âm thanh di chuyển
- `sfxEat.wav`: Âm thanh ăn quân
- `sfxStart.wav`: Âm thanh bắt đầu
- `sfxEnd.wav`: Âm thanh kết thúc

## 🎨 Tài Nguyên Nghệ Thuật

### Hình Ảnh
```
Art/
├── board.png              # Hình bàn cờ
├── icon.png              # Icon ứng dụng
├── cell1.png             # Ô cờ
├── slashScreen.png       # Màn hình chào
├── slashScreen2.png      # Màn hình chào 2
├── slash_screen_3.png    # Màn hình chào 3
├── da_new/               # Quân cờ mới
│   ├── da_a.png         # Dân loại A
│   ├── da_b.png         # Dân loại B
│   └── da_quan.png      # Quan
└── Da_old/              # Quân cờ cũ (24 files)
```

### Font
- `thu_phap.ttf`: Font thư pháp Việt Nam
- `TiltWarp-Regular-VariableFont_XROT,YROT.ttf`: Font hiện đại

### UI Prefabs
```
Prefabs/UI/
├── SlashScreen.prefab    # Màn hình chào
├── PauseUI.prefab       # UI tạm dừng
├── SettingUI.prefab     # UI cài đặt
├── End.prefab           # UI kết thúc
├── Effect.prefab        # Hiệu ứng
└── Sound.prefab         # Âm thanh
```

## 🔨 Build & Deployment

### Build Scripts
1. **build_and_install.sh** (Bash script):
   - Kiểm tra thiết bị Android
   - Hướng dẫn build manual
   - Tự động cài đặt APK

2. **build_apk.py** (Python script):
   - Build APK tự động qua Unity command line
   - Cài đặt APK lên thiết bị

3. **BuildScript.cs** (Unity Editor):
   - Menu build trong Unity Editor
   - Build APK với cấu hình tùy chỉnh

### Cách Build
```bash
# Sử dụng bash script
./build_and_install.sh

# Sử dụng python script
python3 build_apk.py

# Trong Unity Editor
Build → Build Android APK
```

### Cấu Hình Build
- **Target Platform**: Android
- **API Level**: Tự động
- **Architecture**: ARM64
- **Compression**: LZ4
- **Output**: `Build/game-o-quan.apk`

## ⚙️ Cấu Hình Unity

### Unity Version
- **Editor Version**: 2022.3.29f1
- **Target Framework**: .NET Standard 2.1

### Packages Sử Dụng
```json
{
  "com.unity.collab-proxy": "2.8.2",
  "com.unity.feature.2d": "2.0.0",
  "com.unity.textmeshpro": "3.0.6",
  "com.unity.ugui": "1.0.0",
  "com.unity.visualscripting": "1.9.4"
}
```

### Project Settings
- **Company Name**: DefaultCompany
- **Product Name**: Ô Kỳ Quan
- **Bundle Identifier**: com.lamgame.oquan
- **Target Frame Rate**: 60 FPS
- **Fixed Timestep**: 1/50 (0.02s)

## 🎯 Game State Machine

### State Management
Game sử dụng State Machine pattern để quản lý các trạng thái:

```csharp
public interface GameStateMachine
{
    IEnumerator Load(Main _MainScript);
    void UpdateState();
    void Enable();
    void Disable();
    void StartChangeState(GameStateMachine _NextState, GameStateMachine _LastState);
    void EndChangeState(GameStateMachine _NextState, GameStateMachine _LastState);
}
```

### States Available
1. **LoadingState**: Màn hình loading
2. **GameState**: Trạng thái chơi game chính

## 🐛 Debug & Testing

### Debug Features
- Console logging cho các hành động game
- Test cases trong `TestCase.cs`
- Error logging trong build

### Testing
- Manual testing trên thiết bị Android
- Unity Editor testing
- Build testing với scripts tự động

## 📱 Platform Specific

### Android Configuration
- **Minimum API Level**: 21 (Android 5.0)
- **Target API Level**: 33 (Android 13)
- **Permissions**: Không yêu cầu permissions đặc biệt
- **Orientation**: Portrait/Landscape (tùy cấu hình)

### Performance
- **Target FPS**: 60
- **Memory Usage**: Tối ưu cho thiết bị tầm trung
- **Battery Usage**: Tối ưu hóa

## 🔄 Game Flow

### Luồng Chơi Chính
1. **Khởi động**: LoadingState → GameState
2. **Chọn ô**: Player chọn ô có quân
3. **Chọn hướng**: Player chọn hướng rải (trái/phải)
4. **Rải quân**: Animation rải quân theo hướng
5. **Xử lý ăn**: Kiểm tra và xử lý ăn quân
6. **Chuyển lượt**: Chuyển sang player khác
7. **Kiểm tra kết thúc**: Kiểm tra điều kiện kết thúc
8. **Kết thúc**: Hiển thị kết quả và tùy chọn

### UI Flow
```
Splash Screen → Game Board → [Pause/Settings] → End Game
                    ↓
              [Game Playing Loop]
```

## 📊 Data Management

### Master Data
```csharp
public class MasterData
{
    public Piece piece;
    
    public class Piece
    {
        public Quan quan; // score = 10
        public Dan dan;   // score = 1
    }
}
```

### Player Data
```csharp
public class PlayerData
{
    public Profile profile;
    
    public class Profile
    {
        public int uid;
        public string name;
    }
}
```

## 🚀 Tính Năng Nâng Cao

### Visual Effects
- Highlight ô được chọn
- Animation rải quân
- Particle effects khi ăn quân
- UI transitions

### Audio System
- Background music
- Sound effects cho các hành động
- Volume control trong settings

### UI/UX Features
- Responsive UI cho nhiều kích thước màn hình
- Intuitive controls
- Visual feedback cho mọi hành động
- Pause/Resume functionality

## 📝 Ghi Chú Phát Triển

### Known Issues
- Unity command line build có thể gặp vấn đề → Khuyến nghị build manual
- Cần kiểm tra compatibility với các phiên bản Android mới

### Future Improvements
- Multiplayer online
- AI opponent
- Tournament mode
- Statistics tracking
- Achievement system

### Code Quality
- Sử dụng Singleton pattern cho managers
- Event-driven architecture
- Modular design
- Comprehensive error handling

---

**Tài liệu được tạo tự động vào**: 2025-10-27
**Phiên bản Unity**: 2022.3.29f1
**Tác giả**: Game Development Team
