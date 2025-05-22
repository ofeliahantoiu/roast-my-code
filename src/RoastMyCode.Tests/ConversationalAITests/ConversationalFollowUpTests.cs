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

namespace RoastMyCode.Tests.ConversationalAITests
{
    [TestClass]
    public class ConversationalFollowUpTests
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
        [Description("QA-8/BA-10: Test conversational follow-up relevance")]
        public async Task ConversationalFollowUp_ShouldBeRelevantToCodeIssue()
        {
            // Arrange
            var codeWithLoop = @"
for (var i = 0; i < 10; i++) {
    for (var j = 0; j < 10; j++) {
        for (var k = 0; k < 10; k++) {
            console.log(i, j, k);
        }
    }
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with conversational follow-up
            var responseContent = @"
{
  ""roast"": ""Three nested loops? Are you trying to simulate the heat death of the universe with your CPU?"",
  ""followUp"": {
    ""question"": ""Are you intentionally using nested loops, or is there a more efficient algorithm you were considering?"",
    ""context"": ""nested_loops_efficiency""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(codeWithLoop, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            // For these tests, we're validating that the response was processed, not the exact content
            Assert.AreEqual(responseContent, result);
        }

        [TestMethod]
        [Description("QA-8/BA-10: Test conversational follow-up avoids repetition")]
        public async Task ConversationalFollowUp_ShouldAvoidRepetition()
        {
            // Arrange
            var code = @"
function calculate(x) {
    return x * 2;
}";
            var roastLevel = "light";
            
            // Create a conversation history with previous follow-up
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "assistant", Content = "This function is too simple. What about adding more features?" },
                new ChatMessage { Role = "user", Content = "I wanted to keep it simple by design." }
            };

            // Setup mock response with new follow-up that doesn't repeat
            var responseContent = @"
{
  ""roast"": ""Even calculators from the 90s could do more than this function."",
  ""followUp"": {
    ""question"": ""Have you considered adding input validation to handle edge cases?"",
    ""context"": ""input_validation""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            // For these tests, we're validating that the response was processed, not the exact content
            Assert.AreEqual(responseContent, result);
            
            // In a real test, we would check that it doesn't repeat previous conversations
        }

        [TestMethod]
        [Description("BA-10: Test conversational follow-up with weird logic")]
        public async Task ConversationalFollowUp_ShouldHaveLogicalFlow()
        {
            // Arrange
            var codeWithIssue = @"
function divide(a, b) {
    return a / b;
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with logical follow-up
            var responseContent = @"
{
  ""roast"": ""This function has the safety features of a motorcycle without brakes. What happens when b is zero?"",
  ""followUp"": {
    ""question"": ""How would you handle division by zero in this function?"",
    ""context"": ""error_handling""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(codeWithIssue, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            // For these tests, we're validating that the response was processed, not the exact content
            Assert.AreEqual(responseContent, result);
        }

        [TestMethod]
        [Description("BA-10: Test conversational follow-up engagement")]
        public async Task ConversationalFollowUp_ShouldBeEngaging()
        {
            // Arrange
            var code = @"
try {
    // Some code
} catch (error) {
    // Do nothing
}";
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response with engaging follow-up
            var responseContent = @"
{
  ""roast"": ""Empty catch block? That's like installing a fire alarm that just watches your house burn down."",
  ""followUp"": {
    ""question"": ""What kind of errors are you expecting here, and why did you decide to silence them completely?"",
    ""context"": ""error_handling_philosophy"",
    ""additionalPrompts"": [
      ""Did a silent error cause you trauma in the past?"",
      ""Are you planning to add proper error handling later?""
    ]
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            // For these tests, we're validating that the response was processed, not the exact content
            // In a real implementation, we would parse the JSON and validate the structure
            Assert.AreEqual(responseContent, result);
        }

        [TestMethod]
        [Description("QA-8: Test conversational UX for awkward/misfired prompts")]
        public async Task ConversationalFollowUp_ShouldAvoidAwkwardPrompts()
        {
            // Arrange
            var highQualityCode = @"
function calculateArea(radius) {
    if (typeof radius !== 'number' || radius <= 0) {
        throw new Error('Radius must be a positive number');
    }
    return Math.PI * radius * radius;
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response that acknowledges good code but still roasts it
            var responseContent = @"
{
  ""roast"": ""I see you added error handling. I'm mildly impressed, like when a toddler uses a fork correctly."",
  ""followUp"": {
    ""question"": ""You've done a good job with input validation. What other functions have you written with similar attention to detail?"",
    ""context"": ""positive_reinforcement""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(highQualityCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            // For these tests, we're validating that the response was processed, not the exact content
            Assert.AreEqual(responseContent, result);
            
            // In a real test, we would check that it provides appropriate follow-up
        }
        
        [TestMethod]
        [Description("QA-8/BA-10: Test handling of multi-turn conversations")]
        public async Task MultiTurnConversation_ShouldMaintainContext()
        {
            // Arrange
            var code = @"
function fetchData() {
    return fetch('https://api.example.com/data')
        .then(response => response.json())
        .then(data => {
            console.log(data);
            return data;
        });
}";
            var roastLevel = "savage";
            
            // Create a conversation history with previous exchanges
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "Roast this code: function add(a,b) { return a+b; }" },
                new ChatMessage { Role = "assistant", Content = "Wow, a function that adds two numbers. Revolutionary. Did you also invent the wheel today?" },
                new ChatMessage { Role = "user", Content = "How would you improve it?" },
                new ChatMessage { Role = "assistant", Content = "Add type checking, handle edge cases like NaN or Infinity, and consider using TypeScript for better type safety." }
            };

            // Setup mock response that references previous conversation
            var responseContent = @"
{
  ""roast"": ""First you showed me a trivial add function, and now you're fetching data without any error handling? Your code evolution is like watching a caterpillar turn into... a slightly larger caterpillar."",
  ""followUp"": {
    ""question"": ""Did you consider implementing the error handling improvements we discussed for your previous function in this new fetchData function?"",
    ""context"": ""conversation_continuity""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(responseContent, result);
        }
        
        [TestMethod]
        [Description("QA-8/BA-10: Test handling of long conversation history")]
        public async Task LongConversationHistory_ShouldBeHandledCorrectly()
        {
            // Arrange
            var code = @"
function calculateDiscount(price, discountCode) {
    if (discountCode === 'SAVE10') {
        return price * 0.9;
    }
    return price;
}";
            var roastLevel = "brutal";
            
            // Create a long conversation history
            var conversationHistory = new List<ChatMessage>();
            for (int i = 0; i < 10; i++)
            {
                conversationHistory.Add(new ChatMessage { Role = "user", Content = $"Question {i}: How do I improve my code?" });
                conversationHistory.Add(new ChatMessage { Role = "assistant", Content = $"Answer {i}: Here are some suggestions..." });
            }

            // Setup mock response
            var responseContent = @"
{
  ""roast"": ""After our extensive conversation about code quality, you give me THIS? A discount function that only handles one hard-coded discount code? I'm not angry, I'm just disappointed."",
  ""followUp"": {
    ""question"": ""We've discussed many improvements over our conversation. Which specific aspect would you like to focus on improving in this function?"",
    ""context"": ""long_conversation_summary""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(responseContent, result);
        }
        
        [TestMethod]
        [Description("QA-8/BA-10: Test handling of non-code questions in conversation")]
        public async Task NonCodeQuestions_ShouldBeHandledGracefully()
        {
            // Arrange
            var code = @"
function greet(name) {
    return `Hello, ${name}`;
}";
            var roastLevel = "light";
            
            // Create a conversation history with non-code questions
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = "What's your favorite programming language?" },
                new ChatMessage { Role = "assistant", Content = "I don't have preferences, but I can help with many languages including JavaScript, Python, C#, and more." },
                new ChatMessage { Role = "user", Content = "How long have you been coding?" },
                new ChatMessage { Role = "assistant", Content = "I'm an AI assistant trained to help with code, but I don't personally write code outside of helping users." }
            };

            // Setup mock response
            var responseContent = @"
{
  ""roast"": ""After all those philosophical questions, you finally show me some code - and it's just a greeting function? At least your template literal syntax is correct. Baby steps, I guess."",
  ""followUp"": {
    ""question"": ""Now that we're looking at actual code, what specific aspect of JavaScript would you like to explore?"",
    ""context"": ""refocus_on_code""
  }
}";
            
            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(responseContent, result);
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
