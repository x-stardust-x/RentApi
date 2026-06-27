using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace RentApi.Models;

public class GeminiService
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;

    public GeminiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["Gemini:ApiKey"];
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var cleanApiKey = CleanApiKey(_apiKey);

        if (string.IsNullOrWhiteSpace(cleanApiKey))
        {
            throw new Exception("Gemini API Key 未設定");
        }

        throw new Exception(
    $"目前後端讀到的 Gemini Key 前 12 碼：{cleanApiKey[..Math.Min(12, cleanApiKey.Length)]}，長度：{cleanApiKey.Length}"
);

        Console.WriteLine($"Gemini Key Preview: {cleanApiKey[..Math.Min(12, cleanApiKey.Length)]}");
        Console.WriteLine($"Gemini Key Length: {cleanApiKey.Length}");

        //DebugApiKey(cleanApiKey);

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
        request.Headers.TryAddWithoutValidation("x-goog-api-key", cleanApiKey);
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

    private static string CleanApiKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        return new string(value
            .Where(c => c >= 0x21 && c <= 0x7E)
            .ToArray());
    }

    private static void DebugApiKey(string apiKey)
    {
        Console.WriteLine($"Gemini Key Length: {apiKey.Length}");
        Console.WriteLine($"Gemini Key Preview: {apiKey[..Math.Min(10, apiKey.Length)]}");
    }
}




//using Microsoft.Extensions.Configuration;
//using System.Net.Http.Json;
//using System.Text.Json;

//namespace RentApi.Models;

//public class GeminiService
//{
//    private readonly HttpClient _http;
//    private readonly string? _apiKey;

//    public GeminiService(HttpClient http, IConfiguration config)
//    {
//        _http = http;
//        _apiKey = config["Gemini:ApiKey"];
//    }

//    public async Task<string> GenerateAsync(string prompt)
//    {
//        var apiKey = _apiKey?.Trim();

//        if (string.IsNullOrWhiteSpace(apiKey))
//        {
//            throw new Exception("Gemini API Key 未設定");
//        }

//        var cleanApiKey = new string(apiKey.Where(c => c <= 127).ToArray()).Trim();
//        DebugApiKey(apiKey);

//        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

//        var requestBody = new
//        {
//            contents = new[]
//            {
//                new
//                {
//                    parts = new[]
//                    {
//                        new { text = prompt }
//                    }
//                }
//            },
//            generationConfig = new
//            {
//                responseMimeType = "application/json"
//            }
//        };

//        using var request = new HttpRequestMessage(HttpMethod.Post, url);
//        request.Headers.TryAddWithoutValidation("x-goog-api-key", cleanApiKey);
//        request.Content = JsonContent.Create(requestBody);

//        var response = await _http.SendAsync(request);
//        var responseString = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//        {
//            throw new HttpRequestException(
//                $"Gemini API 呼叫失敗：{(int)response.StatusCode} {response.ReasonPhrase}，內容：{responseString}"
//            );
//        }

//        using var jsonDoc = JsonDocument.Parse(responseString);

//        return jsonDoc.RootElement
//            .GetProperty("candidates")[0]
//            .GetProperty("content")
//            .GetProperty("parts")[0]
//            .GetProperty("text")
//            .GetString() ?? "{}";
//    }

//    private static void DebugApiKey(string apiKey)
//    {
//        Console.WriteLine($"Gemini Key Length: {apiKey.Length}");
//        Console.WriteLine($"Gemini Key Preview: {apiKey[..Math.Min(10, apiKey.Length)]}");

//        for (int i = 0; i < apiKey.Length; i++)
//        {
//            var code = (int)apiKey[i];
//            Console.WriteLine($"[{i}] '{apiKey[i]}' U+{code:X4}");
//        }
//    }
//}