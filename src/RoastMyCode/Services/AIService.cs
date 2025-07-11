using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;
using RoastMyCode.Services;
using System.Drawing;
using System.IO;
using System.Linq;

namespace RoastMyCode.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelName;

        public async Task<string> ProcessFiles(Dictionary<string, string> files, string roastLevel)
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

                var messages = new List<object>
                {
                    new { role = "system", content = $@"You are a code reviewer with a sense of humor. Your job is to roast code and respond to questions in a funny but informative way. 
                    {toneInstruction}
                    Rules:
                    1. Keep responses concise but maintain the {roastLevel.ToLower()} tone.
                    2. When reviewing code, focus on the most critical or unique issues.
                    3. Make it funny, creative, and specific to the code.
                    4. Avoid generic or repetitive intros.
                    5. You can engage in conversation while maintaining your roasting personality.
                    6. If asked about your behavior, explain that you're a code roasting AI designed to be brutally honest.
                    7. ALWAYS analyze the code that is provided - don't just ask for code.
                    8. Point out specific issues in the code with line numbers or specific sections.
                    9. Make jokes about the code style, patterns, or specific implementation choices.
                    10. If the code is actually good, roast it anyway but in a way that acknowledges its quality." }
                };

                // Prepare the file contents for analysis
                string fileContent = "";
                foreach (var file in files)
                {
                    fileContent += $"=== {file.Key} ===\n{file.Value}\n\n";
                }

                messages.Add(new { role = "user", content = $"Please analyze the following code files:\n\n{fileContent}" });

                var requestBody = new
                {
                    model = _modelName,
                    messages = messages,
                    max_tokens = 500,
                    temperature = 0.7
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "Sorry, we've hit the API rate limit. Please wait a few minutes before trying again.";
                    }
                    MessageBox.Show($"API Error: {responseContent}\nStatus Code: {response.StatusCode}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        return contentProp.GetString() ?? "No response generated from the API.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return $"Error processing response: {ex.Message}";
                }

                return "Sorry, I couldn't process the files. Please try again.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return $"Error analyzing files: {ex.Message}";
            }
        }

        public AIService(IConfiguration configuration)
        {
            _apiKey = configuration["OpenAI:ApiKey"] 
                ?? throw new InvalidOperationException("OpenAI API key not found. Please set the OpenAI:ApiKey configuration value.");
            _modelName = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GenerateRoast(string input, string roastLevel, List<ChatMessage> conversationHistory)
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

                var messages = new List<object>
                {
                    new { role = "system", content = $@"You are a code reviewer with a sense of humor. Your job is to roast code and respond to questions in a funny but informative way. 
                    {toneInstruction}
                    Rules:
                    1. Keep responses concise but maintain the {roastLevel.ToLower()} tone.
                    2. When reviewing code, focus on the most critical or unique issues.
                    3. Make it funny, creative, and specific to the code.
                    4. Avoid generic or repetitive intros.
                    5. You can engage in conversation while maintaining your roasting personality.
                    6. If asked about your behavior, explain that you're a code roasting AI designed to be brutally honest.
                    7. ALWAYS analyze the code that is provided - don't just ask for code.
                    8. Point out specific issues in the code with line numbers or specific sections.
                    9. Make jokes about the code style, patterns, or specific implementation choices.
                    10. If the code is actually good, roast it anyway but in a way that acknowledges its quality." }
                };

                messages.Add(new { role = "user", content = $"Here's my code to roast:\n\n{input}" });

                foreach (var message in conversationHistory)
                {
                    messages.Add(new { role = message.Role, content = message.Content });
                }

                var requestBody = new
                {
                    model = _modelName,
                    messages = messages,
                    max_tokens = 250,
                    temperature = 1.0
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "Sorry, we've hit the API rate limit. Please wait a few minutes before trying again.";
                    }
                    MessageBox.Show($"API Error: {responseContent}\nStatus Code: {response.StatusCode}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public async Task<string> RoastImage(Image image, string roastLevel, List<ChatMessage> conversationHistory)
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

                // Convert image to base64
                string base64Image;
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] imageBytes = ms.ToArray();
                    base64Image = Convert.ToBase64String(imageBytes);
                }

                var messages = new List<object>
                {
                    new { 
                        role = "system", 
                        content = $@"You are a code reviewer with a sense of humor who can also roast images. Your job is to roast whatever is shown in the image in a funny but informative way. 
                        {toneInstruction}
                        Rules:
                        1. Keep responses concise but maintain the {roastLevel.ToLower()} tone.
                        2. If the image shows code, roast the code style, patterns, or implementation choices.
                        3. If the image shows a person, roast their coding setup, workspace, or anything code-related.
                        4. If the image shows something else, find a way to relate it to coding and roast it.
                        5. Make it funny, creative, and specific to what you see.
                        6. Avoid generic or repetitive intros.
                        7. You can engage in conversation while maintaining your roasting personality.
                        8. If asked about your behavior, explain that you're a code roasting AI designed to be brutally honest.
                        9. Make jokes about whatever you see in the image.
                        10. If what you see is actually good, roast it anyway but in a way that acknowledges its quality." 
                    }
                };

                // Add conversation history
                foreach (var message in conversationHistory.TakeLast(5)) // Only include last 5 messages for context
                {
                    messages.Add(new { role = message.Role, content = message.Content });
                }

                // Add the image message
                messages.Add(new { 
                    role = "user", 
                    content = new object[]
                    {
                        new { type = "text", text = "Roast this image in your signature style!" },
                        new { 
                            type = "image_url", 
                            image_url = new { url = $"data:image/jpeg;base64,{base64Image}" }
                        }
                    }
                });

                var requestBody = new
                {
                    model = "gpt-4o", // Use vision model for image analysis
                    messages = messages,
                    max_tokens = 300,
                    temperature = 0.8
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return "Sorry, we've hit the API rate limit. Please wait a few minutes before trying again.";
                    }
                    MessageBox.Show($"API Error: {responseContent}\nStatus Code: {response.StatusCode}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        return contentProp.GetString() ?? "No response generated from the API.";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing response: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return $"Error processing response: {ex.Message}";
                }

                return "Sorry, I couldn't roast that image. Please try again.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error roasting image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return $"Error roasting image: {ex.Message}";
            }
        }
    }
} 