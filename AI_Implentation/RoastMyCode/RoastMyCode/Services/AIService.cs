using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RoastMyCode.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelName;

        public AIService()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _apiKey = configuration["OpenRouter:ApiKey"] 
                ?? throw new InvalidOperationException("OpenRouter API key not found. Please set the OpenRouter:ApiKey configuration value.");
            _modelName = configuration["OpenRouter:Model"] ?? "anthropic/claude-3-opus-20240229";
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/yourusername/RoastMyCode");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "RoastMyCode");
        }

        public async Task<string> GenerateRoast(string code, string roastLevel)
        {
            try
            {
                string toneInstruction = roastLevel.ToLower() switch
                {
                    "light" => "Be gentle and constructive, like a friendly mentor pointing out issues with a smile.",
                    "savage" => "Be more direct and witty, like a senior developer who's seen it all and isn't afraid to call out bad practices.",
                    "brutal" => "Be merciless and sarcastic, like a code reviewer who's had one too many cups of coffee and zero patience.",
                    _ => "Be constructive but humorous."
                };

                var prompt = $@"You are a code reviewer with a sense of humor. Analyze the following code and provide a {roastLevel.ToLower()} roast.
                {toneInstruction}
                
                Code to roast:
                {code}

                Rules:
                1. Keep the response under 3 sentences.
                2. Focus on the most critical or unique issue in the code.
                3. Make it funny, creative, and specific to the code.
                4. Avoid generic or repetitive intros (e.g., do NOT start with 'oh boy' or 'where do I even start').
                5. {roastLevel.ToLower()} tone must be evident.";

                var requestBody = new
                {
                    model = _modelName,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a code reviewer with a sense of humor. Your job is to roast code in a funny but informative way. Keep responses short and punchy." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 70,
                    temperature = 1.0
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "Sorry, we've hit the API rate limit. Please wait a few minutes before trying again.";
                    }
                    return $"Error: {responseContent}";
                }

                try
                {
                    var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (responseObject.TryGetProperty("choices", out var choices) &&
                        choices.GetArrayLength() > 0 &&
                        choices[0].TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProp))
                    {
                        return contentProp.GetString() ?? "No response generated.";
                    }
                    else
                    {
                        return $"Unexpected response format: {responseContent}";
                    }
                }
                catch (Exception parseEx)
                {
                    return $"Failed to parse response: {parseEx.Message}\nRaw response: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("429"))
                {
                    return "Sorry, we've hit the API rate limit. Please wait a few minutes before trying again.";
                }
                return $"An error occurred: {ex.Message}";
            }
        }
    }
} 