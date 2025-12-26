# Hướng dẫn triển khai Gemini AI

## Bước 1: Tạo GeminiConfig

```csharp
// Assets/Scripts/Client/AI/Gemini/GeminiConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GeminiConfig", menuName = "Game/Gemini Config")]
public class GeminiConfig : ScriptableObject
{
    [Header("API Settings")]
    public string apiKey = "";
    public string model = "gemini-1.5-flash";
    public string baseUrl = "https://generativelanguage.googleapis.com/v1beta";
    
    [Header("Request Settings")]
    public int timeoutSeconds = 10;
    public int maxRetries = 2;
    public float temperature = 0.7f;
    
    [Header("Fallback")]
    public bool enableFallback = true;
    public AIDifficulty fallbackDifficulty = AIDifficulty.Hard;
}
```

## Bước 2: Tạo Response Models

```csharp
// Assets/Scripts/Client/AI/Gemini/GeminiModels.cs
using System;

[Serializable]
public class GeminiRequest
{
    public Content[] contents;
    public GenerationConfig generationConfig;
}

[Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}

[Serializable]
public class GenerationConfig
{
    public float temperature;
    public int maxOutputTokens;
}

[Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public Content content;
}

[Serializable]
public class AIMoveResponse
{
    public int cellIndex;
    public int direction; // 1 = clockwise, -1 = counter-clockwise
    public string explanation;
}
```

## Bước 3: Tạo GeminiService

```csharp
// Assets/Scripts/Client/AI/Gemini/GeminiService.cs
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService
{
    private readonly GeminiConfig _config;
    
    public GeminiService(GeminiConfig config)
    {
        _config = config;
    }
    
    public async Task<string> SendAsync(string prompt)
    {
        string url = $"{_config.baseUrl}/models/{_config.model}:generateContent?key={_config.apiKey}";
        
        var request = new GeminiRequest
        {
            contents = new[] 
            {
                new Content 
                {
                    role = "user",
                    parts = new[] { new Part { text = prompt } }
                }
            },
            generationConfig = new GenerationConfig
            {
                temperature = _config.temperature,
                maxOutputTokens = 256
            }
        };
        
        string json = JsonUtility.ToJson(request);
        
        using var webRequest = new UnityWebRequest(url, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.timeout = _config.timeoutSeconds;
        
        var operation = webRequest.SendWebRequest();
        
        while (!operation.isDone)
            await Task.Yield();
        
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Gemini API Error: {webRequest.error}");
            throw new Exception(webRequest.error);
        }
        
        var response = JsonUtility.FromJson<GeminiResponse>(webRequest.downloadHandler.text);
        return response.candidates[0].content.parts[0].text;
    }
}
```

## Bước 4: Tạo GeminiAI

```csharp
// Assets/Scripts/Client/AI/Gemini/GeminiAI.cs
using System.Threading.Tasks;
using UnityEngine;

public class GeminiAI : AIPlayer
{
    private readonly GeminiService _service;
    private readonly GeminiConfig _config;
    private IAIPlayer _fallbackAI;
    
    public override AIDifficulty Difficulty => AIDifficulty.Hard;
    
    public GeminiAI(GeminiConfig config)
    {
        _config = config;
        _service = new GeminiService(config);
        _fallbackAI = new MinimaxAI();
    }
    
    public override (int cellIndex, int direction) MakeMove(int[] board, PlayerTurn turn, bool quan1Available, bool quan2Available)
    {
        // Sync wrapper - gọi async method
        var task = MakeMoveAsync(board, turn, quan1Available, quan2Available);
        task.Wait();
        return task.Result;
    }
    
    public async Task<(int cellIndex, int direction)> MakeMoveAsync(int[] board, PlayerTurn turn, bool quan1, bool quan2)
    {
        try
        {
            string prompt = BuildPrompt(board, turn, quan1, quan2);
            string response = await _service.SendAsync(prompt);
            return ParseResponse(response, board, turn);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Gemini failed, using fallback: {e.Message}");
            return _fallbackAI.MakeMove(board, turn, quan1, quan2);
        }
    }
    
    private string BuildPrompt(int[] board, PlayerTurn turn, bool quan1, bool quan2)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("Bạn là AI chơi game Ô Quan (Vietnamese Mandarin Square Capturing).");
        sb.AppendLine();
        sb.AppendLine("LUẬT CHƠI:");
        sb.AppendLine("- Bàn cờ có 12 ô: ô 0-4 của P1, ô 5 là Quan1, ô 6-10 của P2, ô 11 là Quan2");
        sb.AppendLine("- Mỗi lượt chọn 1 ô có quân và hướng đi (1=thuận, -1=ngược chiều kim đồng hồ)");
        sb.AppendLine("- Rải quân từng ô một theo hướng đã chọn");
        sb.AppendLine("- Ăn quân khi ô tiếp theo trống và ô sau đó có quân");
        sb.AppendLine();
        sb.AppendLine("TRẠNG THÁI HIỆN TẠI:");
        sb.AppendLine($"- Lượt: {turn}");
        sb.AppendLine($"- Bàn cờ: [{string.Join(", ", board)}]");
        sb.AppendLine($"- Quan1 (ô 5): {(quan1 ? "còn" : "hết")}");
        sb.AppendLine($"- Quan2 (ô 11): {(quan2 ? "còn" : "hết")}");
        sb.AppendLine();
        
        // Liệt kê nước đi hợp lệ
        sb.AppendLine("CÁC Ô HỢP LỆ (có quân, thuộc về bạn):");
        int start = turn == PlayerTurn.P1 ? 0 : 6;
        for (int i = 0; i < 5; i++)
        {
            int idx = start + i;
            if (board[idx] > 0)
                sb.AppendLine($"- Ô {idx}: {board[idx]} quân");
        }
        
        sb.AppendLine();
        sb.AppendLine("TRẢ LỜI ĐÚNG FORMAT JSON:");
        sb.AppendLine("{\"cellIndex\": <số>, \"direction\": <1 hoặc -1>, \"explanation\": \"<giải thích ngắn>\"}");
        
        return sb.ToString();
    }
    
    private (int cellIndex, int direction) ParseResponse(string response, int[] board, PlayerTurn turn)
    {
        try
        {
            // Tìm JSON trong response
            int start = response.IndexOf('{');
            int end = response.LastIndexOf('}') + 1;
            if (start >= 0 && end > start)
            {
                string json = response.Substring(start, end - start);
                var move = JsonUtility.FromJson<AIMoveResponse>(json);
                
                // Validate
                if (IsValidMove(board, move.cellIndex, turn))
                {
                    Debug.Log($"Gemini: ô {move.cellIndex}, hướng {move.direction} - {move.explanation}");
                    return (move.cellIndex, move.direction);
                }
            }
        }
        catch { }
        
        // Fallback nếu parse lỗi
        Debug.LogWarning("Gemini response invalid, using fallback");
        return _fallbackAI.MakeMove(board, turn, true, true);
    }
    
    private bool IsValidMove(int[] board, int cellIndex, PlayerTurn turn)
    {
        int start = turn == PlayerTurn.P1 ? 0 : 6;
        int end = start + 5;
        return cellIndex >= start && cellIndex < end && board[cellIndex] > 0;
    }
}
```

## Bước 5: Cập nhật AIManager

```csharp
// Thêm vào AIManager.cs

// Thêm enum
public enum AIDifficulty
{
    Easy,
    Medium,
    Hard,
    Gemini  // NEW
}

// Cập nhật SetAIDifficulty
public void SetAIDifficulty(AIDifficulty difficulty)
{
    _currentAI = difficulty switch
    {
        AIDifficulty.Easy => new RandomAI(),
        AIDifficulty.Medium => new GreedyAI(),
        AIDifficulty.Hard => new MinimaxAI(),
        AIDifficulty.Gemini => new GeminiAI(geminiConfig), // NEW
        _ => new RandomAI()
    };
    
    Debug.Log($"AI set to: {difficulty}");
}

// Thêm field
[SerializeField] private GeminiConfig geminiConfig;
```

## Bước 6: Cập nhật AIConfig

```csharp
// Thêm vào AIConfig.cs

[Header("Gemini Settings")]
public bool geminiEnabled = true;
public GeminiConfig geminiConfig;
```

## Bước 7: Setup trong Unity

1. **Tạo GeminiConfig asset:**
   - Right-click trong Project → Create → Game → Gemini Config
   - Điền API key

2. **Gán vào AIManager:**
   - Chọn GameObject có AIManager
   - Kéo GeminiConfig vào field

3. **Test:**
   - Chọn difficulty "Gemini" trong game
   - Xem Console log để debug

## Xử lý Async trong Unity

Unity không hỗ trợ `async/await` trực tiếp trong coroutine. Có 2 cách:

### Cách 1: UniTask (khuyến nghị)
```bash
# Install via Package Manager
https://github.com/Cysharp/UniTask.git
```

### Cách 2: Wrapper coroutine
```csharp
public IEnumerator MakeAIMoveCoroutine(...)
{
    var task = _geminiAI.MakeMoveAsync(...);
    
    while (!task.IsCompleted)
        yield return null;
    
    if (task.Exception != null)
        // Handle error
    else
        onMoveDecided(task.Result.cellIndex, task.Result.direction);
}
```

## Tiếp theo

Đọc [PROMPT_ENGINEERING.md](PROMPT_ENGINEERING.md) để tối ưu prompt.
