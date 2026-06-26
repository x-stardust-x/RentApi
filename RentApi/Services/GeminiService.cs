using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RentApi.Services; 

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly IConfiguration _config;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
        _apiKey = _config["Gemini:ApiKey"];
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GCP_API_KEY_HERE")
            throw new Exception("Gemini API Key 未設定，請檢查 appsettings 或 user-secrets");

        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        var requestBody = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", _apiKey.Trim());
        request.Content = JsonContent.Create(requestBody);

        var response = await _http.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API 呼叫失敗：{(int)response.StatusCode} {response.ReasonPhrase}，內容：{responseString}"
            );
        }

        using var jsonDoc = JsonDocument.Parse(responseString);

        return jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";
    }

    //public async Task<string> GenerateAsync(string prompt)
    //{
    //    if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GCP_API_KEY_HERE") 
    //    {
    //        throw new Exception("目前AI無法使用");
    //    }
    //    var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey.Trim()}";

    //    Console.WriteLine("準備打給 Google 的網址是：" + url);
    //    Console.WriteLine("ApiKey" + _apiKey);
    //    if(_apiKey == null || _apiKey.Trim() == "")
    //    {
    //        Console.WriteLine("沒有APIKEY");
    //    }

    //    var requestBody = new
    //    {
    //        contents = new[]
    //        {
    //            new { parts = new[] { new { text = prompt } } }
    //        }
    //    };


    //    var response = await _http.PostAsJsonAsync(url, requestBody);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        var errorDetails = await response.Content.ReadAsStringAsync();
    //        Console.WriteLine($" 【Gemini 拒絕連線原因】: {errorDetails}");
    //    }

    //    response.EnsureSuccessStatusCode();

    //    var responseString = await response.Content.ReadAsStringAsync();


    //    using var jsonDoc = JsonDocument.Parse(responseString);
    //    var root = jsonDoc.RootElement;

    //    var aiTextResponse = root.GetProperty("candidates")[0]
    //                             .GetProperty("content")
    //                             .GetProperty("parts")[0]
    //                             .GetProperty("text").GetString();

    //    return aiTextResponse!;
    //}
}