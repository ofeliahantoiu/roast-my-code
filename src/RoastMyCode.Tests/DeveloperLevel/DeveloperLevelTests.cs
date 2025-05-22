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

namespace RoastMyCode.Tests.DeveloperLevel
{
    [TestClass]
    public class DeveloperLevelTests
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
        [Description("QA-3/BA-5: Test developer level assignment for beginner code")]
        public async Task BeginnerCode_ShouldAssignAppropriateLevel()
        {
            // Arrange
            var beginnerCode = @"
function calculateTotal() {
    var a = 10;
    var b = 20;
    var total = a + b;
    return total;
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with beginner level
            var responseContent = @"
{""developerLevel"": ""Novice Coder"", 
""score"": 15, 
""message"": ""You're clearly new at this. Your variable names are generic and you're doing simple arithmetic. May I suggest a calculator app instead?""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(beginnerCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Novice Coder"));
            Assert.IsTrue(result.Contains("15"));
        }

        [TestMethod]
        [Description("QA-3/BA-5: Test developer level assignment for intermediate code")]
        public async Task IntermediateCode_ShouldAssignAppropriateLevel()
        {
            // Arrange
            var intermediateCode = @"
class Calculator {
    constructor() {
        this.result = 0;
    }
    
    add(a, b) {
        this.result = a + b;
        return this;
    }
    
    subtract(a, b) {
        this.result = a - b;
        return this;
    }
    
    getResult() {
        return this.result;
    }
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with intermediate level
            var responseContent = @"
{""developerLevel"": ""Code Apprentice"", 
""score"": 45, 
""message"": ""Well, you've discovered classes and method chaining. Congratulations on graduating from spaghetti code to slightly organized spaghetti code.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(intermediateCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Code Apprentice"));
            Assert.IsTrue(result.Contains("45"));
        }

        [TestMethod]
        [Description("QA-3/BA-5: Test developer level assignment for advanced code")]
        public async Task AdvancedCode_ShouldAssignAppropriateLevel()
        {
            // Arrange
            var advancedCode = @"
const memoize = (fn) => {
  const cache = new Map();
  return (...args) => {
    const key = JSON.stringify(args);
    if (cache.has(key)) return cache.get(key);
    const result = fn(...args);
    cache.set(key, result);
    return result;
  };
};

const fibonacci = memoize((n) => {
  if (n <= 1) return n;
  return fibonacci(n - 1) + fibonacci(n - 2);
});";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with advanced level
            var responseContent = @"
{""developerLevel"": ""Function Whisperer"", 
""score"": 75, 
""message"": ""Ooh, memoization and recursion in the same snippet? Trying to impress with your big brain energy? I bet you also tell people you 'code in functional style'.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(advancedCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Function Whisperer"));
            Assert.IsTrue(result.Contains("75"));
        }

        [TestMethod]
        [Description("QA-3/BA-5: Test developer level thresholds for edge cases")]
        public async Task EdgeCaseCode_ShouldAssignCorrectLevel()
        {
            // Arrange
            var borderlineCode = @"
// This code is intentionally at the border between beginner and intermediate
class Counter {
    constructor(initialCount = 0) {
        this.count = initialCount;
    }
    
    increment() {
        this.count++;
        return this.count;
    }
    
    decrement() {
        this.count--;
        return this.count;
    }
    
    reset() {
        this.count = 0;
        return this.count;
    }
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with borderline level
            var responseContent = @"
{""developerLevel"": ""Code Tinkerer"", 
""score"": 30, 
""message"": ""You're on the edge of being a real programmer. It's like watching a toddler take their first steps - adorable, but with lots of room for improvement.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(borderlineCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Code Tinkerer"));
            Assert.IsTrue(result.Contains("30"));
        }

        private HttpResponseMessage SetupMockResponseWithDeveloperLevel(string developerLevelContent)
        {
            // Create a mock response that includes the developer level information
            var jsonResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = developerLevelContent
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
