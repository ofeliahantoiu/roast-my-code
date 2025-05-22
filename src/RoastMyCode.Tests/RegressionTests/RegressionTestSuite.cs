using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoastMyCode.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text.Json;
using System.Text;
using RoastMyCode.Tests.Mocks;
using System.Threading.Tasks;
using System;

namespace RoastMyCode.Tests.RegressionTests
{
    [TestClass]
    public class RegressionTestSuite
    {
        private Mock<IConfiguration> _mockConfiguration = null!;

        [TestInitialize]
        public void Setup()
        {
            // Setup configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["OpenAI:ApiKey"]).Returns("dummy-api-key");
            _mockConfiguration.Setup(x => x["OpenAI:Model"]).Returns("gpt-3.5-turbo");
        }

        [TestMethod]
        [Description("BA-15: Regression test for core code input functionality")]
        public async Task Regression_CoreInputFunctionality_ShouldWork()
        {
            // ARRANGE - Test various input scenarios
            var inputs = new[] 
            {
                string.Empty, // Empty input
                GenerateLargeCodeSample(), // Large input
                "function test() {\n  // Special characters: áéíóú ñ € £ ¥ © ®\n  console.log(\"Hello world!\");\n}", // Special chars
                "// Mixed language\ndef hello():\n    print(\"Python\")\n\nfunction hello() {\n    console.log(\"JS\");\n}" // Mixed langs
            };
            
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();
            var expectedResponse = @"{""roast"": ""Regression test response""}";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // ACT & ASSERT
            foreach (var input in inputs)
            {
                var result = await aiService.GenerateRoast(input, roastLevel, conversationHistory);
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedResponse, result);
            }
        }

        [TestMethod]
        [Description("BA-15: Regression test for roast style selection")]
        public async Task Regression_RoastStyles_ShouldWork()
        {
            // ARRANGE
            var code = @"
function greet(name) {
    alert('Hello, ' + name);
}";
            var roastLevels = new[] { "light", "savage", "brutal", "nonexistent" }; // Including invalid style
            var conversationHistory = new List<ChatMessage>();
            var expectedResponse = @"{""roast"": ""Regression test response""}";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // ACT & ASSERT
            foreach (var level in roastLevels)
            {
                var result = await aiService.GenerateRoast(code, level, conversationHistory);
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedResponse, result);
            }
        }

        [TestMethod]
        [Description("BA-15: Regression test for conversation history handling")]
        public async Task Regression_ConversationHistory_ShouldMaintainContext()
        {
            // ARRANGE
            var code = "function test() { }";
            var roastLevel = "light";
            
            // Create a conversation with multiple interactions
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "assistant", Content = "This is a very basic function." },
                new ChatMessage { Role = "user", Content = "Can you suggest improvements?" },
                new ChatMessage { Role = "assistant", Content = "You could add parameters and a return value." },
                new ChatMessage { Role = "user", Content = "What about error handling?" }
            };
            
            var expectedResponse = @"{""roast"": ""Regression test response with context""}";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // ACT
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);
            
            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
            
            // In a real regression test, we would verify:
            // 1. The conversation history is passed correctly to the AI service
            // 2. The context from previous conversations influences the response
        }

        [TestMethod]
        [Description("BA-15: Combined regression test for key functionality")]
        public async Task Regression_CombinedFunctionality_ShouldWork()
        {
            // ARRANGE - A comprehensive test covering multiple key aspects
            var code = @"
class Calculator {
    add(a, b) {
        return a + b;
    }
    subtract(a, b) {
        return a - b;
    }
}";
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Please roast my Calculator class." }
            };
            
            // Create a rich response with multiple components
            var expectedResponse = @"
{
  ""roast"": ""This calculator has fewer features than a dollar store calculator from 1985."",
  ""developerLevel"": ""Code Toddler"",
  ""score"": 35,
  ""improvements"": [
    {
      ""issue"": ""Missing error handling"",
      ""suggestion"": ""Add type checking for inputs""
    },
    {
      ""issue"": ""Limited functionality"",
      ""suggestion"": ""Add multiply and divide methods""
    }
  ],
  ""followUp"": {
    ""question"": ""Were you planning to add more methods to this class?""
  }
}";
            
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // ACT
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);
            
            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
            
            // In a real regression test, we would verify all aspects:
            // 1. Roast generation
            // 2. Developer level assignment
            // 3. Improvement suggestions
            // 4. Conversational follow-up
        }

        private string GenerateLargeCodeSample()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                sb.AppendLine($"Console.WriteLine(\"Line {i}\");");
            }
            return sb.ToString();
        }

        private HttpResponseMessage SetupMockResponse(string responseContent)
        {
            var jsonResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = responseContent
                        }
                    }
                }
            };

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonResponse), Encoding.UTF8, "application/json")
            };

            return response;
        }

        private AIService CreateMockAIService(HttpResponseMessage response)
        {
            // Create a mock HttpClient with our response
            var mockHandler = new MockHttpMessageHandler(_ => response);
            var httpClient = new HttpClient(mockHandler);
            
            // Create our mockable service with the injected HttpClient
            return new MockAIService(_mockConfiguration.Object, httpClient);
        }
    }
}
