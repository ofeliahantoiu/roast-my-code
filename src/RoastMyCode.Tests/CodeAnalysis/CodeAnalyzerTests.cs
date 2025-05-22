using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoastMyCode.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text;
using RoastMyCode.Tests.Mocks;

namespace RoastMyCode.Tests.CodeAnalysis
{
    [TestClass]
    public class CodeAnalyzerTests
    {
        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;

        [TestInitialize]
        public void Setup()
        {
            // Setup configuration mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["OpenAI:ApiKey"]).Returns("dummy-api-key");
            _mockConfiguration.Setup(x => x["OpenAI:Model"]).Returns("gpt-3.5-turbo");

            // Setup HttpMessageHandler mock
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        }

        [TestMethod]
        [Description("QA-2/BA-2: Test code analysis with well-structured code")]
        public async Task AnalyzeWellStructuredCode_ShouldReturnExpectedRoast()
        {
            // Arrange
            var wellStructuredCode = @"
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    public int Subtract(int a, int b)
    {
        return a - b;
    }
}";

            // Setup mock response
            var mockResponse = SetupMockResponse(
                "You call this a calculator? It can't even multiply! " +
                "I've seen more functionality in a Hello World app!");

            var aiService = CreateAIServiceWithMockedHttp(mockResponse);
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();

            // Act
            var result = await aiService.GenerateRoast(wellStructuredCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("calculator"));
        }

        [TestMethod]
        [Description("QA-2/BA-2: Test code analysis with poorly structured code")]
        public async Task AnalyzePoorlyStructuredCode_ShouldReturnExpectedRoast()
        {
            // Arrange
            var poorlyStructuredCode = @"
function doStuff(){
var x=1;
var y =2;
var z= x+y;
console.log(z);
if(z>0){console.log(""positive"");}}";

            // Setup mock response
            var mockResponse = SetupMockResponse(
                "Your indentation is as consistent as my sleep schedule. " +
                "And those variable names! x, y, z? Did you fall asleep on your keyboard?");

            var aiService = CreateAIServiceWithMockedHttp(mockResponse);
            var roastLevel = "brutal";
            var conversationHistory = new List<ChatMessage>();

            // Act
            var result = await aiService.GenerateRoast(poorlyStructuredCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("indentation"));
        }

        [TestMethod]
        [Description("QA-2/BA-2: Test code analysis with no clear purpose")]
        public async Task AnalyzeCodeWithoutClearPurpose_ShouldReturnExpectedRoast()
        {
            // Arrange
            var codeWithoutPurpose = @"
// Just some random code
var x = [];
for (var i = 0; i < 10; i++) {
    x.push(i);
}
var sum = 0;
for (var j = 0; j < x.length; j++) {
    sum += x[j];
}
console.log(sum);";

            // Setup mock response
            var mockResponse = SetupMockResponse(
                "This code is like a movie without a plot. What exactly are you trying to accomplish? " +
                "Computing the sum of 0-9? Ever heard of a simple math formula?");

            var aiService = CreateAIServiceWithMockedHttp(mockResponse);
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Act
            var result = await aiService.GenerateRoast(codeWithoutPurpose, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("code is like"));
        }

        [TestMethod]
        [Description("QA-2/BA-2: Test error handling during code analysis")]
        public async Task HandleErrorDuringCodeAnalysis_ShouldReturnErrorMessage()
        {
            // Arrange
            var code = "function test() { console.log('Hello'); }";

            // Setup mock response for error
            var mockResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

            var aiService = CreateAIServiceWithMockedHttp(mockResponse);
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("rate limit"));
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

        private AIService CreateAIServiceWithMockedHttp(HttpResponseMessage response)
        {
            // Create a mock HttpClient with our response
            var mockHandler = new MockHttpMessageHandler(_ => response);
            var httpClient = new HttpClient(mockHandler);
            
            // Create our mockable service with the injected HttpClient
            return new MockAIService(_mockConfiguration.Object, httpClient);
        }
    }
}
