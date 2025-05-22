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

namespace RoastMyCode.Tests.InputHandling
{
    [TestClass]
    public class CodeInputTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;
        // Removing unused field: private Mock<AIService> _mockAIService;

        [TestInitialize]
        public void Setup()
        {
            // Setup configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns("dummy-api-key");
            _mockConfiguration.Setup(x => x["OpenAI:ApiKey"]).Returns("dummy-api-key");
            _mockConfiguration.Setup(x => x["OpenAI:Model"]).Returns("gpt-3.5-turbo");
        }

        [TestMethod]
        [Description("QA-1/BA-1: Test empty code input handling")]
        public async Task EmptyCodeInput_ShouldHandleGracefully()
        {
            // Arrange
            var emptyCode = string.Empty;
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();
            
            // Setup mock response
            var expectedResponse = "Even your empty code has issues. Amazing achievement!";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(emptyCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
        }

        [TestMethod]
        [Description("QA-1/BA-1: Test very large code input handling")]
        public async Task VeryLargeCodeInput_ShouldHandleGracefully()
        {
            // Arrange
            // Generate 10k+ lines of code
            var largeCode = GenerateLargeCodeSample();
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var expectedResponse = "10,000 lines? Was your keyboard stuck? Quality over quantity, my friend.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(largeCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
        }

        [TestMethod]
        [Description("QA-1/BA-1: Test code input with special characters")]
        public async Task SpecialCharactersInCode_ShouldHandleGracefully()
        {
            // Arrange
            // Code with special characters
            var codeWithSpecialChars = "function test() {\n" +
                                      "  // Special characters: áéíóú ñ € £ ¥ © ®\n" +
                                      "  console.log(\"Hello world!\");\n" +
                                      "}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var expectedResponse = "Using special characters in comments? Trying to impress me? The code is still boring.";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(codeWithSpecialChars, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
        }

        [TestMethod]
        [Description("QA-1/BA-1: Test mixed-language code snippets")]
        public async Task MixedLanguageCode_ShouldHandleGracefully()
        {
            // Arrange
            // Code with multiple languages
            var mixedLanguageCode = "// Python code\n" +
                                   "def hello():\n" +
                                   "    print(\"Hello world!\")\n\n" +
                                   "// JavaScript code\n" +
                                   "function hello() {\n" +
                                   "    console.log(\"Hello world!\");\n" +
                                   "}\n\n" +
                                   "// C# code\n" +
                                   "public void Hello() {\n" +
                                   "    Console.WriteLine(\"Hello world!\");\n" +
                                   "}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var expectedResponse = "Can't decide on a language? Commitment issues in your code too?";
            var mockResponse = SetupMockResponse(expectedResponse);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(mixedLanguageCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResponse, result);
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
