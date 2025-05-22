using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RoastMyCode.Tests.TestUtils
{
    /// <summary>
    /// Enhanced mock HTTP message handler for simulating various API responses and error conditions
    /// </summary>
    public class EnhancedMockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, MockResponse> _responseMap = new Dictionary<string, MockResponse>();
        private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _dynamicResponses = new Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>>();
        private readonly List<RequestLog> _requestLogs = new List<RequestLog>();
        private readonly Random _random = new Random();
        
        // Configurable behavior
        public bool SimulateNetworkLatency { get; set; } = false;
        public int MinLatencyMs { get; set; } = 100;
        public int MaxLatencyMs { get; set; } = 500;
        public double ErrorRate { get; set; } = 0.0; // 0.0 to 1.0
        public double RateLimitChance { get; set; } = 0.0; // 0.0 to 1.0
        
        /// <summary>
        /// Add a mock response for a specific URL
        /// </summary>
        public void AddMockResponse(string url, HttpStatusCode statusCode, string content, string contentType = "application/json")
        {
            _responseMap[url] = new MockResponse
            {
                StatusCode = statusCode,
                Content = content,
                ContentType = contentType
            };
        }
        
        /// <summary>
        /// Add a dynamic response generator for a specific URL
        /// </summary>
        public void AddDynamicResponse(string url, Func<HttpRequestMessage, HttpResponseMessage> responseGenerator)
        {
            _dynamicResponses[url] = responseGenerator;
        }
        
        /// <summary>
        /// Add a mock OpenAI response for a specific URL
        /// </summary>
        public void AddMockOpenAIResponse(string url, string responseContent)
        {
            var openAIResponse = new
            {
                id = $"chatcmpl-{Guid.NewGuid():N}",
                @object = "chat.completion",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                model = "gpt-4",
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = responseContent
                        },
                        finish_reason = "stop"
                    }
                },
                usage = new
                {
                    prompt_tokens = 100,
                    completion_tokens = 50,
                    total_tokens = 150
                }
            };
            
            AddMockResponse(url, HttpStatusCode.OK, JsonSerializer.Serialize(openAIResponse));
        }
        
        /// <summary>
        /// Add a mock OpenAI error response
        /// </summary>
        public void AddMockOpenAIErrorResponse(string url, string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var errorResponse = new
            {
                error = new
                {
                    message = errorMessage,
                    type = "invalid_request_error",
                    param = string.Empty,
                    code = "invalid_api_key"
                }
            };
            
            AddMockResponse(url, statusCode, JsonSerializer.Serialize(errorResponse));
        }
        
        /// <summary>
        /// Add a mock rate limit exceeded response
        /// </summary>
        public void AddMockRateLimitResponse(string url)
        {
            var rateLimitResponse = new
            {
                error = new
                {
                    message = "Rate limit exceeded",
                    type = "rate_limit_exceeded",
                    param = string.Empty,
                    code = "rate_limit_exceeded"
                }
            };
            
            AddMockResponse(url, HttpStatusCode.TooManyRequests, JsonSerializer.Serialize(rateLimitResponse));
        }
        
        /// <summary>
        /// Get the request logs
        /// </summary>
        public IReadOnlyList<RequestLog> GetRequestLogs()
        {
            return _requestLogs.AsReadOnly();
        }
        
        /// <summary>
        /// Clear the request logs
        /// </summary>
        public void ClearRequestLogs()
        {
            _requestLogs.Clear();
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log the request
            var requestLog = new RequestLog
            {
                Url = request.RequestUri?.ToString() ?? "",
                Method = request.Method.ToString(),
                Timestamp = DateTime.Now
            };
            
            if (request.Content != null)
            {
                requestLog.Content = await request.Content.ReadAsStringAsync();
            }
            
            _requestLogs.Add(requestLog);
            
            // Simulate network latency if enabled
            if (SimulateNetworkLatency)
            {
                int delayMs = _random.Next(MinLatencyMs, MaxLatencyMs);
                await Task.Delay(delayMs, cancellationToken);
            }
            
            // Randomly generate errors if error rate is set
            if (ErrorRate > 0 && _random.NextDouble() < ErrorRate)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Simulated server error", Encoding.UTF8, "application/json")
                };
            }
            
            // Randomly generate rate limit errors if chance is set
            if (RateLimitChance > 0 && _random.NextDouble() < RateLimitChance)
            {
                var rateLimitResponse = new
                {
                    error = new
                    {
                        message = "Rate limit exceeded",
                        type = "rate_limit_exceeded",
                        param = "",
                        code = "rate_limit_exceeded"
                    }
                };
                
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Content = new StringContent(JsonSerializer.Serialize(rateLimitResponse), Encoding.UTF8, "application/json")
                };
            }
            
            // Get the URL to match against our response map
            string url = request.RequestUri?.ToString() ?? "";
            
            // Check if we have a dynamic response generator for this URL
            if (_dynamicResponses.TryGetValue(url, out var responseGenerator))
            {
                return responseGenerator(request);
            }
            
            // Check if we have a mock response for this URL
            if (_responseMap.TryGetValue(url, out var mockResponse))
            {
                return new HttpResponseMessage(mockResponse.StatusCode)
                {
                    Content = new StringContent(mockResponse.Content, Encoding.UTF8, mockResponse.ContentType)
                };
            }
            
            // If no mock response is found, return a 404
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("No mock response found for this URL", Encoding.UTF8, "application/json")
            };
        }
    }
    
    /// <summary>
    /// Mock response data
    /// </summary>
    public class MockResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/json";
    }
    
    /// <summary>
    /// Request log entry
    /// </summary>
    public class RequestLog
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
