using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RentApi.Models; // 記得換成你的命名空間

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("找不到 Gemini API Key！");
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        // 🌟 升級成 2.5 代最新引擎，舊的 1.5 已經退休啦！
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey.Trim()}";

        Console.WriteLine("準備打給 Google 的網址是：" + url);
        // 把我們的 Prompt 裝箱
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        // 發送請求並等待回應
        var response = await _http.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        // 剝開 JSON 外衣，精準拿出 AI 的文字回覆
        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;

        var aiTextResponse = root.GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text").GetString();

        return aiTextResponse!;
    }
}