using RentApi.Models.DTO;
using System.Text.Json;
using RentApi.Services;
namespace RentApi.Models; // 記得換成你的命名空間

public class MatchService
{
    private readonly GeminiService _gemini;

    // 透過相依性注入 (DI) 引入剛剛建好的 Gemini 引擎
    public MatchService(GeminiService gemini)
    {
        _gemini = gemini;
    }

    public async Task<List<MatchResult>> CalculateBatchScoresAsync(object user, IEnumerable<object> houses)
    {
        var finalResults = new List<MatchResult>();
        var housesForAi = new List<object>();


        var userJson = JsonSerializer.Serialize(user);
        using var userDoc = JsonDocument.Parse(userJson);
        var userRoot = userDoc.RootElement;

        bool isSmoking = userRoot.TryGetProperty("isSmoking", out var s) && s.GetBoolean();
        bool hasPet = userRoot.TryGetProperty("hasPet", out var p) && p.GetBoolean();

        foreach (var house in houses)
        {
            var houseJson = JsonSerializer.Serialize(house);
            using var houseDoc = JsonDocument.Parse(houseJson);
            var houseRoot = houseDoc.RootElement;


            int currentHouseId = 0;
            if (houseRoot.TryGetProperty("Id", out var idProp) ||
                houseRoot.TryGetProperty("id", out idProp) ||
                houseRoot.TryGetProperty("houseId", out idProp))
            {
                currentHouseId = idProp.GetInt32();
            }

            bool allowSmoking = houseRoot.TryGetProperty("allowSmoking", out var a) && a.GetBoolean();
            bool allowPet = houseRoot.TryGetProperty("allowPet", out var ap) && ap.GetBoolean();


            if (isSmoking && !allowSmoking)
            {
                finalResults.Add(new MatchResult
                {
                    HouseId = currentHouseId,
                    Score = 0,
                    Basis = "【系統初步篩選未通過】",
                    Risk = "房屋嚴格禁菸，但租客有抽菸習慣，條件不符。",
                    Suggestion = "建議尋找其他允許抽菸的租屋物件。"
                });
                continue;
            }


            if (hasPet && !allowPet)
            {
                finalResults.Add(new MatchResult
                {
                    HouseId = currentHouseId,
                    Score = 0,
                    Basis = "【系統初步篩選未通過】",
                    Risk = "房屋禁止飼養寵物，但租客有飼養寵物，條件不符。",
                    Suggestion = "建議尋找其他寵物友善的租屋物件。"
                });
                continue;
            }


            housesForAi.Add(house);
        }


        if (!housesForAi.Any())
        {
            return finalResults;
        }



        var housesForAiJson = JsonSerializer.Serialize(housesForAi);

       
        foreach (var house in housesForAi)
        {
           
            var houseJson = JsonSerializer.Serialize(house);

           
            using var hDoc = JsonDocument.Parse(houseJson);
            int currentHouseId = 0;
            if (hDoc.RootElement.TryGetProperty("Id", out var hIdProp) ||
                hDoc.RootElement.TryGetProperty("id", out hIdProp) ||
                hDoc.RootElement.TryGetProperty("houseId", out hIdProp))
            {
                currentHouseId = hIdProp.GetInt32();
            }
            string currentHouseName = hDoc.RootElement.TryGetProperty("name", out var nProp) ? nProp.GetString() : "此精選房源";

            return finalResults;
        }
        catch (Exception ex)
        {

            Console.WriteLine($" 成功攔截 503 或 JSON 錯誤：{ex.Message}");

            foreach (var house in housesForAi)
            {
                
                var prompt = $$"""
                你是一位專業的「共居生活租賃媒合評分 AI 秘書」。
                我將提供一位【租客資料】以及【單一間房屋資料】。
                請幫我評估該租客與此房屋的生活契合度。

                【評分標準請嚴格遵守】：
                - 90~100分：作息與習慣完美契合，且無任何衝突。
                - 70~89分：有些微差異但不影響居住。
                - 0~69分：有明顯衝突。

                【寫作風格與禁忌】
                1. 請扮演專業、有溫度的真人房產秘書，文筆必須自然流暢。
                2. 絕對禁止在回覆中出現任何「英文變數名稱」、「屬性名稱」或「程式碼數值」。
                3. 請將資料轉化為人類自然語言。

                【輸出格式要求】
                請「只」回傳一個標準的 JSON 物件 (Object)。禁止包含任何 Markdown 語法（如 ```json）。

                JSON 格式範例：
                {
                  "score": 85,
                  "basis": "該物件為獨立套房，租客與現有室友作息高度契合。",
                  "risk": "現有資料未規範禁菸，可能需再與房東確認。",
                  "suggestion": "建議簽約前務必與房東確認相關限制，避免後續居住糾紛。"
                }


                finalResults.Add(new MatchResult
                {
                    HouseId = currentHouseId,
                    Score = 80,
                    Basis = $"【系統安全防禦機制】已成功連結房源「{currentHouseName}」之基礎結構。",
                    Risk = "由於目前 AI 秘書正在處理這間房子的配對需求時遇到亂流，暫時無法評估深度習慣衝突。",
                    Suggestion = "建議您先將此物件加入追蹤，或直接聯絡房東進行實地看房與習慣確認！"
                });
            }

            
            await Task.Delay(3000);
        }

        return finalResults;
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