using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RentApi.Models; 

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Gemini:ApiKey"];
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GCP_API_KEY_HERE") 
        {
            throw new Exception("目前AI無法使用");
        }
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey.Trim()}";

        Console.WriteLine("準備打給 Google 的網址是：" + url);
        
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

       
        var response = await _http.PostAsJsonAsync(url, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            Console.WriteLine($" 【Gemini 拒絕連線原因】: {errorDetails}");
        }

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

       
        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;

        var aiTextResponse = root.GetProperty("candidates")[0]
                                 .GetProperty("content")
                                 .GetProperty("parts")[0]
                                 .GetProperty("text").GetString();

        return aiTextResponse!;
    }
}