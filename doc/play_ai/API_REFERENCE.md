# Gemini API Reference

## API Endpoint

```
POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={API_KEY}
```

## Models

| Model | Speed | Quality | Cost | Recommended |
|-------|-------|---------|------|-------------|
| gemini-1.5-flash | Nhanh | Tốt | Rẻ | ✅ Game AI |
| gemini-1.5-pro | Chậm | Rất tốt | Đắt | Analysis |
| gemini-1.0-pro | Trung bình | Tốt | Trung bình | Backup |

## Request Format

```json
{
  "contents": [
    {
      "role": "user",
      "parts": [
        {
          "text": "Your prompt here"
        }
      ]
    }
  ],
  "generationConfig": {
    "temperature": 0.7,
    "maxOutputTokens": 256,
    "topP": 0.95,
    "topK": 40
  },
  "safetySettings": [
    {
      "category": "HARM_CATEGORY_HARASSMENT",
      "threshold": "BLOCK_NONE"
    }
  ]
}
```

## Response Format

```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          {
            "text": "AI response here"
          }
        ],
        "role": "model"
      },
      "finishReason": "STOP",
      "index": 0
    }
  ],
  "usageMetadata": {
    "promptTokenCount": 100,
    "candidatesTokenCount": 50,
    "totalTokenCount": 150
  }
}
```

## Unity Implementation

### GeminiService.cs

```csharp
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;
    
    public GeminiService(string apiKey, string model = "gemini-1.5-flash")
    {
        _apiKey = apiKey;
        _model = model;
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
    }
    
    public async Task<GeminiResult> GenerateAsync(string prompt, float temperature = 0.7f)
    {
        string url = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";
        
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = temperature,
                maxOutputTokens = 256
            }
        };
        
        string json = JsonUtility.ToJson(requestBody);
        
        using var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 10;
        
        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            return new GeminiResult
            {
                Success = false,
                Error = request.error
            };
        }
        
        try
        {
            var response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
            return new GeminiResult
            {
                Success = true,
                Text = response.candidates[0].content.parts[0].text,
                TokensUsed = response.usageMetadata?.totalTokenCount ?? 0
            };
        }
        catch (Exception e)
        {
            return new GeminiResult
            {
                Success = false,
                Error = e.Message
            };
        }
    }
}

public class GeminiResult
{
    public bool Success;
    public string Text;
    public string Error;
    public int TokensUsed;
}
```

## Error Handling

### HTTP Status Codes

| Code | Meaning | Action |
|------|---------|--------|
| 200 | Success | Parse response |
| 400 | Bad request | Check prompt format |
| 401 | Unauthorized | Check API key |
| 429 | Rate limited | Wait and retry |
| 500 | Server error | Retry or fallback |

### Error Response

```json
{
  "error": {
    "code": 400,
    "message": "Invalid argument",
    "status": "INVALID_ARGUMENT"
  }
}
```

### Retry Logic

```csharp
public async Task<GeminiResult> GenerateWithRetry(string prompt, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var result = await GenerateAsync(prompt);
        
        if (result.Success)
            return result;
        
        if (result.Error.Contains("429")) // Rate limited
        {
            await Task.Delay(1000 * (i + 1)); // Exponential backoff
            continue;
        }
        
        break; // Other errors, don't retry
    }
    
    return new GeminiResult { Success = false, Error = "Max retries exceeded" };
}
```

## Rate Limits

| Model | RPM (requests/min) | TPM (tokens/min) |
|-------|-------------------|------------------|
| Flash | 60 | 1,000,000 |
| Pro | 60 | 120,000 |

## Cost Calculation

```csharp
public class CostCalculator
{
    // Prices per 1M tokens (USD)
    private const float FLASH_INPUT = 0.075f;
    private const float FLASH_OUTPUT = 0.30f;
    
    public float CalculateCost(int inputTokens, int outputTokens)
    {
        return (inputTokens * FLASH_INPUT + outputTokens * FLASH_OUTPUT) / 1_000_000f;
    }
    
    // Estimate per game (20 moves)
    // Input: ~500 tokens/move = 10,000 tokens
    // Output: ~50 tokens/move = 1,000 tokens
    // Cost: ~$0.001 per game
}
```

## Security Best Practices

### 1. Không hardcode API key

```csharp
// ❌ Bad
private string apiKey = "AIza...";

// ✅ Good - Load from config
private string apiKey = config.apiKey;
```

### 2. Validate trên server (nếu có)

```csharp
// Client gửi move request
// Server validate với Gemini
// Server trả về kết quả
```

### 3. Rate limiting phía client

```csharp
private float _lastRequestTime;
private const float MIN_INTERVAL = 1f;

public async Task<GeminiResult> GenerateThrottled(string prompt)
{
    float elapsed = Time.time - _lastRequestTime;
    if (elapsed < MIN_INTERVAL)
        await Task.Delay((int)((MIN_INTERVAL - elapsed) * 1000));
    
    _lastRequestTime = Time.time;
    return await GenerateAsync(prompt);
}
```

## Testing

### cURL Test

```bash
curl -X POST \
  "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=YOUR_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "contents": [{
      "parts": [{"text": "Ô Quan: Board [5,5,5,5,5,10,5,5,5,5,5,10], P1 turn. Reply JSON: {cellIndex, direction}"}]
    }]
  }'
```

### Unity Test

```csharp
[Test]
public async Task TestGeminiConnection()
{
    var service = new GeminiService("YOUR_API_KEY");
    var result = await service.GenerateAsync("Say hello");
    
    Assert.IsTrue(result.Success);
    Assert.IsNotEmpty(result.Text);
}
```

## Tiếp theo

Quay lại [README.md](README.md) để xem tổng quan.
