# UX/UI Improvements - Chơi với AI

## Vấn đề hiện tại

1. **Không rõ lượt ai** - Người chơi khó nhận biết đang là lượt mình hay AI
2. **AI đang suy nghĩ** - Không có indicator khi AI đang tính toán
3. **Mũi tên xuất hiện sai lúc** - Mũi tên chọn hướng hiện khi không cần thiết
4. **Thiếu feedback** - Không có animation/highlight khi AI đi

## Đề xuất cải thiện

### 1. Turn Indicator rõ ràng

```
┌─────────────────────────────────────┐
│  👤 LƯỢT CỦA BẠN    │   🤖 AI ĐANG SUY NGHĨ...  │
│  ████████████████   │   ⏳ Loading animation    │
└─────────────────────────────────────┘
```

- Banner lớn ở trên màn hình
- Màu khác nhau: Xanh (bạn) / Đỏ (AI)
- Animation pulse khi đến lượt

### 2. AI Thinking Indicator

```
┌──────────────────┐
│  🤖 AI đang      │
│  suy nghĩ...     │
│  ⏳ ● ● ●        │
└──────────────────┘
```

- Hiện khi AI đang tính toán
- Animation dots hoặc spinner
- Disable input của người chơi

### 3. AI Move Highlight

- Highlight ô AI chọn (màu đỏ/cam)
- Animation đường đi của quân
- Delay ngắn trước khi AI đi (0.5s) để người chơi thấy

### 4. Ẩn mũi tên khi lượt AI

- Chỉ hiện mũi tên khi lượt người chơi
- AI tự chọn hướng, không cần UI

### 5. Sound Effects

- Âm thanh khác khi AI đi
- Âm thanh "ding" khi đến lượt bạn

## Implementation Priority

| # | Feature | Impact | Effort |
|---|---------|--------|--------|
| 1 | AI Thinking Indicator | High | Low |
| 2 | Turn Banner | High | Medium |
| 3 | Ẩn mũi tên lượt AI | High | Low |
| 4 | AI Move Highlight | Medium | Medium |
| 5 | Sound Effects | Low | Low |

## Code Changes Needed

### 1. Thêm AI Thinking UI
- Tạo `AIThinkingUI.cs` - popup "AI đang suy nghĩ"
- Gọi khi `AIManager.IsThinking = true`

### 2. Cập nhật Turn Banner
- Sửa `PlayerControl.cs` - thêm banner text
- Đổi màu/text theo lượt

### 3. Fix Arrow Direction
- Sửa `GameManager.cs` - không show arrow khi AI turn
- Kiểm tra `isAIEnabled && currentTurn == aiPlayer`

### 4. AI Move Animation
- Sửa `MoveHandler.cs` - highlight cell trước khi AI đi
- Thêm delay để người chơi thấy
