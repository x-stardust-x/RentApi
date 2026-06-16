using RentApi.Models.DTO;
using System.Text.Json;

using RentApi.Models;
namespace RentApi.Models; // 記得換成你的命名空間

public class MatchService
{
    private readonly GeminiService _gemini;

    // 透過相依性注入 (DI) 引入剛剛建好的 Gemini 引擎
    public MatchService(GeminiService gemini)
    {
        _gemini = gemini;
    }

    public async Task<MatchResult> CalculateScoreAsync(object user, object house)
    {
        // 1. 將前端傳進來的物件轉換成 JSON 字串
        var userJson = JsonSerializer.Serialize(user);
        var houseJson = JsonSerializer.Serialize(house);

        // ==========================================
        // 🚨 第一關：C# 嚴格海選 (Hard Filter)
        // ==========================================
        using var userDoc = JsonDocument.Parse(userJson);
        using var houseDoc = JsonDocument.Parse(houseJson);
        var userRoot = userDoc.RootElement;
        var houseRoot = houseDoc.RootElement;

        // 攔截條件 A：抽菸規範 (假設房子禁菸，但租客抽菸 -> 直接秒殺)
        bool isSmoking = userRoot.TryGetProperty("isSmoking", out var s) && s.GetBoolean();
        bool allowSmoking = houseRoot.TryGetProperty("allowSmoking", out var a) && a.GetBoolean();

        if (isSmoking && !allowSmoking)
        {
            // 系統直接退件，完全不呼叫 AI，省時又省額度！
            return new MatchResult { Score = 0, Reason = "【系統初步篩選未通過】房屋嚴格禁菸，但租客有抽菸習慣，條件不符。" };
        }

        
        bool hasPet = userRoot.TryGetProperty("hasPet", out var p) && p.GetBoolean();
        bool allowPet = houseRoot.TryGetProperty("allowPet", out var ap) && ap.GetBoolean();

        if (hasPet && !allowPet)
        {
            return new MatchResult { Score = 0, Reason = "【系統初步篩選未通過】房屋禁止飼養寵物，條件不符。" };
        }
       
        var prompt = $$"""
            你是一位專業的「共居生活租賃媒合評分 AI」。
            請嚴格根據以下提供的【User (租客生活習慣)】與【House (房屋與室友規範)】資料，計算出兩者的生活契合度。
           
            
            【評分標準請嚴格遵守】：
            - 90~100分：作息與習慣完美契合，且無任何衝突。
            - 70~89分：有些微差異但不影響居住（例如輕量室內運動不吵鬧）。
            - 0~69分：有明顯衝突（如日夜顛倒、違反禁菸/寵物規定）。

            【租客資料】：{JsonSerializer.Serialize(user)}
            【房屋資料】：{JsonSerializer.Serialize(house)}

            【輸出格式要求】
            請「只」回傳一個標準的 JSON 物件，禁止包含任何 Markdown 語法，禁止任何額外說明。

            JSON 格式範例：
            {
              "score": 85,
              "reason": "租客與現有室友作息高度契合。唯一需要留意的是..."
            }

            【輸入資料】
            租客資料:
            {{userJson}}

            房屋與規範資料:
            {{houseJson}}
            """;

        var rawResponse = await _gemini.GenerateAsync(prompt);
        var cleanJson = CleanAiJson(rawResponse);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MatchResult>(cleanJson, options);
            return result ?? new MatchResult { Score = 0, Reason = "系統媒合失敗" };
        }
        catch (JsonException ex)
        {
            return new MatchResult { Score = 50, Reason = $"[解析失敗] 錯誤：{ex.Message}" };
        }
    }
    

    /// <summary>
    /// 輔助方法：清除 AI 可能誤加的 Markdown 標籤 (如 ```json)
    /// </summary>
    private string CleanAiJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "{}";

        var cleaned = input.Trim();
        if (cleaned.StartsWith("```"))
        {
            // 移除開頭的 ```json 或 ```
            int firstNewLine = cleaned.IndexOf('\n');
            if (firstNewLine != -1)
            {
                cleaned = cleaned.Substring(firstNewLine).Trim();
            }
            // 移除結尾的 ```
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
            }
        }
        return cleaned;
    }
}