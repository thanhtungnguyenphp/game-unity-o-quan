# Kiến trúc tích hợp Gemini AI

## Kiến trúc hiện tại

```
Assets/Scripts/Client/AI/
├── IAIPlayer.cs          # Interface cho tất cả AI
├── AIPlayer.cs           # Base class với helper methods
├── AIManager.cs          # Quản lý AI, điều phối
├── AIConfig.cs           # ScriptableObject config
├── RandomAI.cs           # Easy - random moves
├── GreedyAI.cs           # Medium - greedy algorithm
├── MinimaxAI.cs          # Hard - minimax + alpha-beta
└── TranspositionTable.cs # Cache cho Minimax
```

## Kiến trúc mới với Gemini

```
Assets/Scripts/Client/AI/
├── IAIPlayer.cs          # (giữ nguyên)
├── AIPlayer.cs           # (giữ nguyên)
├── AIManager.cs          # (cập nhật - thêm Gemini)
├── AIConfig.cs           # (cập nhật - thêm Gemini settings)
├── RandomAI.cs           # (giữ nguyên)
├── GreedyAI.cs           # (giữ nguyên)
├── MinimaxAI.cs          # (giữ nguyên)
├── TranspositionTable.cs # (giữ nguyên)
│
├── Gemini/               # NEW - Gemini integration
│   ├── GeminiAI.cs       # IAIPlayer implementation
│   ├── GeminiService.cs  # HTTP client cho API
│   ├── GeminiConfig.cs   # API key, model settings
│   ├── GeminiPrompts.cs  # Prompt templates
│   └── GeminiResponse.cs # Response models
```

## Flow xử lý

### 1. Flow nước đi AI (hiện tại)

```
GameManager.OnCellClicked()
    ↓
GameManager.ProcessAITurn()
    ↓
AIManager.MakeAIMove(board, turn, ...)
    ↓
IAIPlayer.MakeMove() → (cellIndex, direction)
    ↓
GameManager.ExecuteMove()
```

### 2. Flow nước đi Gemini AI (mới)

```
GameManager.ProcessAITurn()
    ↓
AIManager.MakeAIMove()
    ↓
GeminiAI.MakeMove()
    ↓
GeminiService.SendRequest()
    ↓ (async HTTP)
Gemini API
    ↓
Parse response → (cellIndex, direction)
    ↓
GameManager.ExecuteMove()
```

## Class Diagram

```
┌──────────────────┐
│   <<interface>>  │
│    IAIPlayer     │
├──────────────────┤
│ +MakeMove()      │
│ +Difficulty      │
└────────┬─────────┘
         │ implements
    ┌────┴────┬────────┬────────┐
    ↓         ↓        ↓        ↓
┌────────┐ ┌────────┐ ┌────────┐ ┌────────────┐
│RandomAI│ │GreedyAI│ │MinimaxAI│ │  GeminiAI  │
└────────┘ └────────┘ └────────┘ └─────┬──────┘
                                       │ uses
                                       ↓
                               ┌───────────────┐
                               │ GeminiService │
                               ├───────────────┤
                               │ +SendAsync()  │
                               │ +apiKey       │
                               └───────────────┘
```

## Sequence Diagram - Gemini Move

```
Player      GameManager     AIManager      GeminiAI      GeminiService    Gemini API
  │              │              │              │              │              │
  │  EndTurn     │              │              │              │              │
  │─────────────>│              │              │              │              │
  │              │ MakeAIMove() │              │              │              │
  │              │─────────────>│              │              │              │
  │              │              │  MakeMove()  │              │              │
  │              │              │─────────────>│              │              │
  │              │              │              │ SendRequest()│              │
  │              │              │              │─────────────>│              │
  │              │              │              │              │  HTTP POST   │
  │              │              │              │              │─────────────>│
  │              │              │              │              │   Response   │
  │              │              │              │              │<─────────────│
  │              │              │              │  (cell,dir)  │              │
  │              │              │              │<─────────────│              │
  │              │              │  (cell,dir)  │              │              │
  │              │              │<─────────────│              │              │
  │              │ ExecuteMove()│              │              │              │
  │              │<─────────────│              │              │              │
  │  Animation   │              │              │              │              │
  │<─────────────│              │              │              │              │
```

## Xử lý lỗi & Fallback

```
GeminiAI.MakeMove()
    │
    ├─→ Success → Return (cellIndex, direction)
    │
    ├─→ Timeout (>5s) → Fallback to MinimaxAI
    │
    ├─→ API Error → Fallback to MinimaxAI
    │
    └─→ Invalid Response → Fallback to MinimaxAI
```

## Caching Strategy

```
┌─────────────────────────────────────────┐
│            GeminiCache                   │
├─────────────────────────────────────────┤
│ Key: hash(board + turn + quan1 + quan2) │
│ Value: (cellIndex, direction, explain)  │
│ TTL: 1 game session                     │
└─────────────────────────────────────────┘
```

- Cache nước đi để tránh gọi API lặp lại
- Clear cache khi bắt đầu game mới
- Giảm chi phí API đáng kể

## Offline Mode

Khi không có internet:
1. Detect connection status
2. Auto-switch to MinimaxAI
3. Show notification to user
4. Resume Gemini when online

## Tiếp theo

Đọc [IMPLEMENTATION.md](IMPLEMENTATION.md) để xem code chi tiết.
