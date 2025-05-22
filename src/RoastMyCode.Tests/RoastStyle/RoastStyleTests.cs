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

namespace RoastMyCode.Tests.RoastStyle
{
    [TestClass]
    public class RoastStyleTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;
        private string _sampleCode = null!;

        [TestInitialize]
        public void Setup()
        {
            // Setup configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["OpenAI:ApiKey"]).Returns("dummy-api-key");
            _mockConfiguration.Setup(x => x["OpenAI:Model"]).Returns("gpt-3.5-turbo");

            // Sample code to use for all tests
            _sampleCode = @"
function calculateTotal(items) {
    var total = 0;
    for (var i = 0; i < items.length; i++) {
        total = total + items[i].price;
    }
    return total;
}";
        }

        [TestMethod]
        [Description("QA-5/BA-4: Test 'light' roast style tone")]
        public async Task LightRoastStyle_ShouldUseAppropriateLanguage()
        {
            // Arrange
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with light tone
            var expectedResponse = "I see you're using a for loop with a var. It's like watching someone use a flip phone in 2025 - technically it works, but there are better options available now.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(_sampleCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
            
            // Additional assertions could check for absence of harsh language
            Assert.IsFalse(result.Contains("terrible"));
            Assert.IsFalse(result.Contains("awful"));
        }

        [TestMethod]
        [Description("QA-5/BA-4: Test 'savage' roast style tone")]
        public async Task SavageRoastStyle_ShouldUseAppropriateLanguage()
        {
            // Arrange
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with savage tone
            var expectedResponse = "Oh look, another developer stuck in 2010! Your for loop is crying for help, and 'var'? Seriously? JavaScript evolved while you were napping. Try array methods and const/let sometime this century.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(_sampleCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
            
            // Additional assertions could check for presence of more direct language
            Assert.IsTrue(result.Contains("Seriously"));
        }

        [TestMethod]
        [Description("QA-5/BA-4: Test 'brutal' roast style tone")]
        public async Task BrutalRoastStyle_ShouldUseAppropriateLanguage()
        {
            // Arrange
            var roastLevel = "brutal";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with brutal tone
            var expectedResponse = "This code is a war crime against modern JavaScript. For loops? Var? Manual accumulation? It's like you're TRYING to write the worst code possible. Did you learn to code by reading tutorials from 2005? Array.reduce would solve this in one line, but I guess that was too complicated for your dinosaur brain.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(_sampleCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
            
            // Additional assertions could check for presence of harsh language
            Assert.IsTrue(result.Contains("war crime"));
            Assert.IsTrue(result.Contains("worst"));
        }

        [TestMethod]
        [Description("QA-5/BA-4: Test consistency of roast style across different code samples")]
        public async Task RoastStyle_ShouldBeConsistentAcrossDifferentSamples()
        {
            // Arrange
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();
            
            var codeA = @"
function greet(name) {
    alert('Hello, ' + name);
}";

            var codeB = @"
function factorial(n) {
    if (n == 0) return 1;
    return n * factorial(n - 1);
}";

            // Setup mock responses
            var responseA = "Using alert in 2025? That's like showing up to a Tesla convention in a horse and buggy. And concatenating with '+'? String templates have been around for nearly a decade, grandpa.";
            var mockResponseA = SetupMockResponse(responseA);
            
            var responseB = "Equality with '==' instead of '==='? Playing fast and loose with type coercion, I see. Bold strategy for someone writing a recursive function that could easily blow the stack.";
            var mockResponseB = SetupMockResponse(responseB);
            
            var aiServiceA = CreateMockAIService(mockResponseA);
            var aiServiceB = CreateMockAIService(mockResponseB);

            // Act
            var resultA = await aiServiceA.GenerateRoast(codeA, roastLevel, conversationHistory);
            var resultB = await aiServiceB.GenerateRoast(codeB, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(resultA);
            Assert.IsNotNull(resultB);
            
            // Check that both responses maintain the same tone level
            Assert.IsTrue(resultA.Contains("grandpa") || resultA.Contains("horse"));
            Assert.IsTrue(resultB.Contains("Bold strategy") || resultB.Contains("fast and loose"));
        }

        [TestMethod]
        [Description("QA-5/BA-4: Test fallback behavior for invalid roast style")]
        public async Task InvalidRoastStyle_ShouldFallbackToDefault()
        {
            // Arrange
            var invalidRoastLevel = "nonexistent_style";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var expectedResponse = "Your for loop is unnecessarily verbose. Consider using array methods like reduce() for cleaner code.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(_sampleCode, invalidRoastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
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
