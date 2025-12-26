# Thiết kế Prompt cho Gemini AI

## Nguyên tắc thiết kế prompt

1. **Rõ ràng** - Mô tả luật chơi đầy đủ
2. **Cấu trúc** - Format output cố định (JSON)
3. **Context** - Cung cấp đủ thông tin trạng thái
4. **Constraints** - Giới hạn nước đi hợp lệ

## Prompt Templates

### 1. Basic Move Prompt

```
Bạn là AI chơi game Ô Quan. Trả lời ĐÚNG format JSON.

Bàn cờ: [5, 5, 5, 5, 5, 10, 5, 5, 5, 5, 5, 10]
Lượt: P1 (ô 0-4)
Ô hợp lệ: 0, 1, 2, 3, 4

Trả lời: {"cellIndex": <0-4>, "direction": <1 hoặc -1>}
```

### 2. Strategic Move Prompt (nâng cao)

```
Bạn là AI chơi game Ô Quan cấp độ cao.

LUẬT:
- Bàn cờ 12 ô: P1 (0-4), Quan1 (5), P2 (6-10), Quan2 (11)
- Rải quân theo hướng, ăn khi gặp ô trống + ô có quân
- Thắng khi đối thủ hết quân hoặc có nhiều điểm hơn

TRẠNG THÁI:
Board: [3, 0, 7, 2, 4, 8, 1, 5, 3, 2, 6, 9]
Turn: P1
Quan1: còn, Quan2: còn
Điểm P1: 12, Điểm P2: 15

CHIẾN THUẬT CẦN XEM XÉT:
1. Ăn được bao nhiêu quân?
2. Có thể ăn Quan không?
3. Đối thủ có thể phản công không?
4. Kiểm soát vị trí chiến lược

Trả lời JSON:
{
  "cellIndex": <số>,
  "direction": <1 hoặc -1>,
  "explanation": "<phân tích ngắn>"
}
```

### 3. Teaching Mode Prompt

```
Bạn là thầy dạy chơi Ô Quan cho người mới.

Trạng thái: [board]
Lượt: P1

Hãy:
1. Giải thích 2-3 nước đi tốt nhất
2. Phân tích ưu/nhược điểm mỗi nước
3. Đề xuất nước đi và giải thích tại sao

Format:
{
  "moves": [
    {"cell": 2, "dir": 1, "score": 8, "reason": "Ăn được 5 quân"},
    {"cell": 3, "dir": -1, "score": 6, "reason": "An toàn, không bị phản"}
  ],
  "recommendation": {"cellIndex": 2, "direction": 1},
  "lesson": "Khi có cơ hội ăn nhiều quân, nên ưu tiên..."
}
```

### 4. Analysis Prompt (sau game)

```
Phân tích ván cờ Ô Quan:

Lịch sử nước đi:
1. P1: ô 2, hướng 1 → ăn 3 quân
2. P2: ô 7, hướng -1 → ăn 5 quân
...

Kết quả: P1 thắng (35-25)

Hãy phân tích:
1. Nước đi hay nhất của mỗi bên
2. Sai lầm lớn nhất
3. Thời điểm quyết định
4. Bài học rút ra
```

## Tối ưu Prompt

### Giảm tokens (tiết kiệm chi phí)

**Trước (dài):**
```
Bạn đang chơi game Ô Quan, một trò chơi dân gian Việt Nam...
```

**Sau (ngắn):**
```
Ô Quan AI. Board:[...] Turn:P1 Valid:0,1,2,3,4
Reply: {"c":<cell>,"d":<dir>}
```

### Tăng độ chính xác

1. **Few-shot examples:**
```
Ví dụ 1:
Board: [5,5,5,5,5,10,5,5,5,5,5,10], Turn: P1
Answer: {"cellIndex": 2, "direction": 1}

Ví dụ 2:
Board: [0,3,7,2,4,8,1,5,3,2,6,9], Turn: P1
Answer: {"cellIndex": 2, "direction": -1}

Bây giờ:
Board: [current_board], Turn: P1
Answer:
```

2. **Chain of thought:**
```
Suy nghĩ từng bước:
1. Liệt kê ô có quân: ...
2. Tính điểm mỗi nước: ...
3. Chọn nước tốt nhất: ...
```

## Xử lý Response

### Parse JSON an toàn

```csharp
private (int, int) ParseResponse(string response)
{
    // Tìm JSON block
    var match = Regex.Match(response, @"\{[^}]+\}");
    if (!match.Success) return Fallback();
    
    try
    {
        var move = JsonUtility.FromJson<AIMoveResponse>(match.Value);
        if (IsValid(move)) return (move.cellIndex, move.direction);
    }
    catch { }
    
    return Fallback();
}
```

### Xử lý response không hợp lệ

| Response | Xử lý |
|----------|-------|
| JSON đúng format | Parse và validate |
| JSON sai format | Regex extract numbers |
| Text thuần | Tìm số trong text |
| Timeout | Fallback MinimaxAI |
| Error | Fallback MinimaxAI |

## Prompt cho các chế độ chơi

### Easy Mode
- Prompt đơn giản
- Không yêu cầu chiến thuật
- Random trong các nước hợp lệ

### Medium Mode
- Prompt có chiến thuật cơ bản
- Ưu tiên ăn quân
- Tránh để bị ăn

### Hard Mode
- Prompt đầy đủ chiến thuật
- Tính toán nhiều bước
- Phân tích đối thủ

## Testing Prompts

### Test cases

```python
# Test 1: Opening move
board = [5,5,5,5,5,10,5,5,5,5,5,10]
expected = valid move from 0-4

# Test 2: Capture opportunity
board = [3,0,7,2,4,8,1,5,3,2,6,9]
expected = move that captures most

# Test 3: Endgame
board = [0,0,1,0,2,0,0,1,0,0,1,0]
expected = optimal endgame move
```

### Metrics

- **Accuracy**: % nước đi hợp lệ
- **Strength**: Win rate vs MinimaxAI
- **Latency**: Thời gian response
- **Cost**: Tokens per move

## Tiếp theo

Đọc [API_REFERENCE.md](API_REFERENCE.md) để xem chi tiết API.
