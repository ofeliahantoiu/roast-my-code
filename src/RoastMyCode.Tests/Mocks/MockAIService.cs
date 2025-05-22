using Microsoft.Extensions.Configuration;
using RoastMyCode.Services;
using System.Net.Http;
using System.Reflection;

namespace RoastMyCode.Tests.Mocks
{
    public class MockAIService : AIService
    {
        public MockAIService(IConfiguration configuration, HttpClient httpClient) 
            : base(configuration)
        {
            // Use reflection to replace the private HttpClient with our mock
            typeof(AIService)
                .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(this, httpClient);
        }
    }
}
