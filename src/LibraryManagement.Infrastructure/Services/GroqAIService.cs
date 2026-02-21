using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LibraryManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Infrastructure.Services;

public class GroqAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly ILogger<GroqAIService> _logger;

    public GroqAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqAIService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AI:Groq:ApiKey"];
        _model = configuration["AI:Groq:Model"] ?? "llama-3.1-8b-instant";
        _logger = logger;

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

    public async Task<string> GenerateBookDescriptionAsync(string title, string author, CancellationToken cancellationToken = default)
    {
        var prompt = $"Write a creative book description (2-3 sentences) for: \"{title}\" by {author}.\n" +
            "This is for a library catalog - create an engaging, fictional description.\n" +
            "RESPOND WITH ONLY THE DESCRIPTION TEXT.\n" +
            "NO disclaimers, NO \"I couldn't find\", NO meta-commentary. Just the description.";
        
        var result = await CallGroqAsync(prompt, cancellationToken);
        if (result is null) 
            return $"A book titled \"{title}\" by {author}.";

        // Clean up any preamble the AI might add
        var cleaned = result.Trim();
        
        // Remove common AI preambles
        var preambles = new[] {
            "here's a possible description:",
            "here is a possible description:",
            "here's a description:",
            "here is a description:",
            "description:",
            "here's the description:",
            "here is the description:"
        };
        
        foreach (var preamble in preambles)
        {
            var idx = cleaned.IndexOf(preamble, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                cleaned = cleaned.Substring(idx + preamble.Length).Trim();
                break;
            }
        }

        // Remove quotes if the entire response is quoted
        if (cleaned.StartsWith("\"") && cleaned.EndsWith("\"") && cleaned.Length > 2)
        {
            cleaned = cleaned.Substring(1, cleaned.Length - 2);
        }

        return string.IsNullOrWhiteSpace(cleaned) 
            ? $"A book titled \"{title}\" by {author}." 
            : cleaned;
    }

    public async Task<List<string>> GetBookRecommendationsAsync(List<string> previousBooks, CancellationToken cancellationToken = default)
    {
        var bookList = string.Join(", ", previousBooks.Select(b => $"\"{b}\""));
        var prompt = $"Based on these books: {bookList}, recommend 5 similar books.\n" +
            "RESPOND WITH ONLY A JSON ARRAY of book titles. Example: [\"Book 1\", \"Book 2\"]\n" +
            "NO explanations, NO markdown, NO other text. ONLY the JSON array.";

        var result = await CallGroqAsync(prompt, cancellationToken);
        if (result is null) return [];

        return ExtractJsonArray(result);
    }

    public async Task<List<string>> CategorizeBookAsync(string title, string author, string? description, List<string>? existingCategories = null, CancellationToken cancellationToken = default)
    {
        var existingList = existingCategories?.Count > 0 
            ? $"Available categories: [{string.Join(", ", existingCategories.Take(30).Select(c => $"\"{c}\""))}]. Use these when they fit. "
            : "";

        var prompt = $"Task: Categorize this book into 1-3 categories.\n" +
            $"Book: \"{title}\" by {author}\n" +
            (string.IsNullOrEmpty(description) ? "" : $"Description: {description}\n") +
            $"{existingList}" +
            "RESPOND WITH ONLY A JSON ARRAY. Example: [\"Fiction\", \"Mystery\"]\n" +
            "NO explanations, NO markdown, NO other text. ONLY the JSON array.";

        var result = await CallGroqAsync(prompt, cancellationToken);
        if (result is null) return [];

        return ExtractJsonArray(result);
    }

    private static List<string> ExtractJsonArray(string input)
    {
        var cleaned = input.Trim();
        
        // Remove markdown code blocks
        if (cleaned.StartsWith("```"))
        {
            var lines = cleaned.Split('\n');
            cleaned = string.Join("\n", lines.Skip(1).Take(lines.Length - 2)).Trim();
        }

        // Try to find JSON array in the response
        var startIdx = cleaned.IndexOf('[');
        var endIdx = cleaned.LastIndexOf(']');
        
        if (startIdx >= 0 && endIdx > startIdx)
        {
            var jsonPart = cleaned.Substring(startIdx, endIdx - startIdx + 1);
            try
            {
                return JsonSerializer.Deserialize<List<string>>(jsonPart) ?? [];
            }
            catch
            {
                // Fall through to return empty
            }
        }

        return [];
    }

    public async Task<string> SmartSearchAsync(string naturalLanguageQuery, CancellationToken cancellationToken = default)
    {
        var prompt = "Extract search terms from this library search query. Be BRIEF - use only essential words. " +
            $"Query: \"{naturalLanguageQuery}\" " +
            "Return JSON with these optional fields (use only what's mentioned, keep values SHORT): " +
            "\"author\" (just the name, e.g. \"Stephen Hawking\"), " +
            "\"genre\" (one word, e.g. \"fiction\"), " +
            "\"keywords\" (1-2 key words only). " +
            "Do NOT include title unless user asks for a specific book title. " +
            "Example: {\"author\": \"Hawking\", \"genre\": \"science\"} " +
            "Only return JSON, no markdown.";

        return await CallGroqAsync(prompt, cancellationToken) ?? "{}";
    }

    public async Task<List<string>> MatchBooksFromCatalogAsync(List<string> userBooks, List<string> catalogBooks, CancellationToken cancellationToken = default)
    {
        if (catalogBooks.Count == 0) return [];

        var userList = string.Join(", ", userBooks.Take(5).Select(b => $"\"{b}\""));
        var catalogList = string.Join(", ", catalogBooks.Take(20).Select(b => $"\"{b}\""));

        var prompt = $"User has read: {userList}.\n" +
            $"From this catalog: [{catalogList}], pick up to 5 books they might enjoy.\n" +
            "RESPOND WITH ONLY A JSON ARRAY of exact titles from the catalog.\n" +
            "Example: [\"Title1\", \"Title2\"]\n" +
            "NO explanations, NO markdown, NO other text. ONLY the JSON array.";

        var result = await CallGroqAsync(prompt, cancellationToken);
        if (result is null) return [];

        return ExtractJsonArray(result);
    }

    private async Task<string?> CallGroqAsync(string prompt, CancellationToken cancellationToken)
    {
        if (!IsAvailable)
        {
            _logger.LogWarning("Groq AI service called but no API key configured");
            return null;
        }

        try
        {
            var url = "https://api.groq.com/openai/v1/chat/completions";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 500
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Groq API error {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: cancellationToken);
            return json?.Choices?.FirstOrDefault()?.Message?.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq AI service");
            return null;
        }
    }
}

internal class GroqResponse
{
    [JsonPropertyName("choices")]
    public List<GroqChoice>? Choices { get; set; }
}

internal class GroqChoice
{
    [JsonPropertyName("message")]
    public GroqMessage? Message { get; set; }
}

internal class GroqMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
