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

      
        using var userDoc = JsonDocument.Parse(userJson);
        using var houseDoc = JsonDocument.Parse(houseJson);
        var userRoot = userDoc.RootElement;
        var houseRoot = houseDoc.RootElement;

        
        bool isSmoking = userRoot.TryGetProperty("isSmoking", out var s) && s.GetBoolean();
        bool allowSmoking = houseRoot.TryGetProperty("allowSmoking", out var a) && a.GetBoolean();

        if (isSmoking && !allowSmoking)
        {
            return new MatchResult
            {
                Score = 0,
                Basis = "【系統初步篩選未通過】",
                Risk = "房屋嚴格禁菸，但租客有抽菸習慣，條件不符。",
                Suggestion = "建議尋找其他允許抽菸的租屋物件。"
            };
        }


        bool hasPet = userRoot.TryGetProperty("hasPet", out var p) && p.GetBoolean();
        bool allowPet = houseRoot.TryGetProperty("allowPet", out var ap) && ap.GetBoolean();

        if (hasPet && !allowPet)
        {
            return new MatchResult
            {
                Score = 0,
                Basis = "【系統初步篩選未通過】",
                Risk = "房屋禁止飼養寵物，但租客有飼養寵物，條件不符。",
                Suggestion = "建議尋找其他寵物友善的租屋物件。"
            };
        }

        var prompt = $$"""
            你是一位專業的「共居生活租賃媒合評分 AI」。
            請嚴格根據以下提供的【User (租客生活習慣)】與【House (房屋與室友規範)】資料，計算出兩者的生活契合度。
            
            【評分標準請嚴格遵守】：
            - 90~100分：作息與習慣完美契合，且無任何衝突。
            - 70~89分：有些微差異但不影響居住。
            - 0~69分：有明顯衝突。

            【寫作風格與禁忌】
            1. 請扮演專業、有溫度的真人房產秘書，文筆必須自然流暢。
            2. 絕對禁止在回覆中出現任何「英文變數名稱」、「屬性名稱」或「程式碼數值」（例如絕對不可以寫出 Pet: true, Smoke: false 等字眼）。
            3. 請將資料轉化為人類自然語言，例如直接說「租客有養寵物及抽菸習慣」即可。

            【輸出格式要求】
            請「只」回傳一個標準的 JSON 物件，禁止包含任何 Markdown 語法（如 ```json）。
            必須嚴格將講評拆解為 basis、risk、suggestion 三個繁體中文欄位。

            JSON 格式範例：
            {
              "score": 85,
              "basis": "該物件為獨立套房，租客與現有室友作息高度契合。",
              "risk": "現有資料未規範禁菸，且租客本身有抽菸習慣，可能需再與房東確認。",
              "suggestion": "建議簽約前務必與房東確認相關限制，避免後續居住糾紛。"
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

            
            return result ?? new MatchResult
            {
                Score = 0,
                Basis = "【系統媒合失敗】",
                Risk = "無法順利取得 AI 分析結果。",
                Suggestion = "請稍後重新整理網頁再試一次。"
            };
        }
        catch (JsonException ex)
        {
            
            return new MatchResult
            {
                Score = 50,
                Basis = "【AI 解析失敗】",
                Risk = $"資料格式錯誤：{ex.Message}",
                Suggestion = "這可能是 AI 伺服器繁忙，請再次點擊配對按鈕。"
            };
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