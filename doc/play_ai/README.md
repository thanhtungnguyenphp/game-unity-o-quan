# Tích hợp Gemini AI cho Game Ô Quan

## Tổng quan

Tài liệu này hướng dẫn tích hợp Google Gemini AI vào game Ô Quan để tạo đối thủ AI thông minh hơn, có khả năng giải thích nước đi và tương tác tự nhiên với người chơi.

## Mục lục

1. [README.md](README.md) - Tổng quan (file này)
2. [ARCHITECTURE.md](ARCHITECTURE.md) - Kiến trúc hệ thống
3. [IMPLEMENTATION.md](IMPLEMENTATION.md) - Hướng dẫn triển khai
4. [API_REFERENCE.md](API_REFERENCE.md) - Tham khảo API
5. [PROMPT_ENGINEERING.md](PROMPT_ENGINEERING.md) - Thiết kế prompt

## Tại sao dùng Gemini AI?

### So sánh với AI hiện tại (Minimax)

| Tiêu chí | Minimax AI | Gemini AI |
|----------|------------|-----------|
| Tính toán | Cục bộ, nhanh | Cloud, có độ trễ |
| Độ mạnh | Cố định theo depth | Linh hoạt, học được |
| Giải thích | Không | Có thể giải thích nước đi |
| Tương tác | Không | Chat, gợi ý, dạy chơi |
| Chi phí | Miễn phí | Tính theo API calls |
| Offline | Có | Không |

### Use Cases phù hợp

1. **Chế độ "Học chơi"** - AI giải thích luật, gợi ý nước đi
2. **Chế độ "Thách đấu"** - AI mạnh, phân tích game
3. **Chat trong game** - Hỏi đáp về chiến thuật
4. **Phân tích sau game** - Review các nước đi

## Kiến trúc đề xuất

```
┌─────────────────────────────────────────────────────┐
│                    Unity Game                        │
├─────────────────────────────────────────────────────┤
│  AIManager                                           │
│  ├── IAIPlayer (interface)                          │
│  │   ├── RandomAI      (Easy - offline)             │
│  │   ├── GreedyAI      (Medium - offline)           │
│  │   ├── MinimaxAI     (Hard - offline)             │
│  │   └── GeminiAI      (Cloud - NEW)                │
│  │                                                   │
│  └── GeminiService (HTTP client)                    │
│      └── Gemini API                                  │
└─────────────────────────────────────────────────────┘
```

## Quick Start

### 1. Lấy API Key
- Truy cập: https://makersuite.google.com/app/apikey
- Tạo API key mới

### 2. Cài đặt trong Unity
```csharp
// Thêm API key vào config
GeminiConfig.ApiKey = "YOUR_API_KEY";
```

### 3. Sử dụng
```csharp
// Trong AIManager
SetAIDifficulty(AIDifficulty.Gemini);
```

## Ước tính chi phí

| Model | Input (1M tokens) | Output (1M tokens) |
|-------|-------------------|-------------------|
| Gemini 1.5 Flash | $0.075 | $0.30 |
| Gemini 1.5 Pro | $1.25 | $5.00 |

**Ước tính cho game:**
- Mỗi nước đi: ~500 tokens input, ~100 tokens output
- 1 game (~20 nước): ~12,000 tokens
- Chi phí/game (Flash): ~$0.001 (rất rẻ)

## Tiếp theo

Đọc [ARCHITECTURE.md](ARCHITECTURE.md) để hiểu chi tiết kiến trúc.
