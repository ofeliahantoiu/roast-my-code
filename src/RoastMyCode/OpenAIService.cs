using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoastMyCode
{
    /// <summary>
    /// Handles communication with the OpenAI API
    /// </summary>
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "https://api.openai.com/v1/chat/completions";

        public OpenAIService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.OpenAIApiKey}");
        }

        /// <summary>
        /// Gets a roast for the provided code
        /// </summary>
        /// <param name="code">The code to roast</param>
        /// <param name="roastLevel">The desired roast level (Light, Savage, or Helpful)</param>
        /// <returns>The AI-generated roast</returns>
        public async Task<string> GetRoastAsync(string code, string roastLevel)
        {
            try
            {
                var prompt = GetPromptForRoastLevel(code, roastLevel);
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a witty programming expert who provides feedback on code." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(ApiEndpoint, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseBody);

                return responseObject?.Choices[0]?.Message?.Content ?? "No response generated.";
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get roast from OpenAI API.", ex);
            }
        }

        private string GetPromptForRoastLevel(string code, string roastLevel)
        {
            return roastLevel.ToLower() switch
            {
                "light" => $"Give a light-hearted, mildly sarcastic roast of this code:\n\n{code}",
                "savage" => $"Give a brutally honest, extremely sarcastic roast of this code:\n\n{code}",
                "helpful" => $"Give constructive criticism with a touch of humor for this code:\n\n{code}",
                _ => $"Give a light-hearted roast of this code:\n\n{code}"
            };
        }
    }

    // Classes for deserializing the OpenAI API response
    public class OpenAIResponse
    {
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }
} 