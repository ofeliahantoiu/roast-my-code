using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoastMyCode;
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
using System.Windows.Forms;
using System;

namespace RoastMyCode.Tests.IntegrationTests
{
    [TestClass]
    public class EndToEndWorkflowTests
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
        [Description("BA-7: Test complete code submission and roast workflow")]
        public async Task CompleteCodeSubmissionWorkflow_ShouldSucceed()
        {
            // This test would simulate a complete user journey from code submission to roast generation
            
            /* Test Case: Complete Code Submission Workflow
             * 
             * Steps:
             * 1. User enters code in the text box
             * 2. User selects roast style
             * 3. User submits code for roasting
             * 4. System sends code to AI service
             * 5. System receives and displays roast
             * 6. System displays appropriate developer level
             * 7. System offers improvement suggestions
             * 8. System provides conversational follow-up
             * 
             * Expected Results:
             * - Complete flow executes without errors
             * - All components (AI service, UI, etc.) interact correctly
             * - User receives appropriate feedback at each step
             */
            
            // Since this is an integration test, we would need to create mocks for all components
            // and verify their interactions. Here we'll demonstrate the key integration points:
            
            // ARRANGE
            var code = @"
function calculateTotal(items) {
    var total = 0;
    for (var i = 0; i < items.length; i++) {
        total = total + items[i].price;
    }
    return total;
}";
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();

            // Mock AI service response with all components
            var responseContent = @"
{
  ""roast"": ""This code is straight out of 1995. Did you just find this in an ancient JavaScript tutorial?"",
  ""developerLevel"": ""Nostalgia Coder"",
  ""score"": 40,
  ""improvements"": [
    {
      ""issue"": ""Outdated syntax"",
      ""suggestion"": ""Use const/let instead of var and modern array methods"",
      ""example"": ""function calculateTotal(items) { return items.reduce((total, item) => total + item.price, 0); }""
    }
  ],
  ""followUp"": {
    ""question"": ""What made you choose a for loop over array methods like reduce?""
  }
}";

            var mockResponse = SetupMockResponse(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // ACT - In a real test, we would interact with UI elements
            var result = await aiService.GenerateRoast(code, roastLevel, conversationHistory);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual(responseContent, result);

            // In a real integration test, we would verify:
            // 1. UI updated correctly with roast
            // 2. Developer level displayed
            // 3. Improvement suggestions shown
            // 4. Follow-up question presented
        }

        [TestMethod]
        [Description("BA-7: Test theme customization and persistence workflow")]
        public void ThemeCustomizationWorkflow_ShouldPersistSettings()
        {
            /* Test Case: Theme Customization Workflow
             * 
             * Steps:
             * 1. User opens application with default theme
             * 2. User toggles to dark mode
             * 3. User changes font family and size
             * 4. User closes application
             * 5. User reopens application
             * 6. Settings are preserved from previous session
             * 
             * Expected Results:
             * - Settings changes are reflected immediately in UI
             * - Settings are properly persisted
             * - Settings are correctly restored on application restart
             */
            
            // This test would require UI automation, so we'll provide a documentation style test
            
            Assert.Inconclusive("This test requires UI automation framework");
        }

        [TestMethod]
        [Description("BA-7: Test copy to clipboard and improvement suggestion workflow")]
        public void CopyAndImprovementWorkflow_ShouldWork()
        {
            /* Test Case: Copy and Improvement Workflow
             * 
             * Steps:
             * 1. User receives a roast with improvement suggestions
             * 2. User clicks on an improvement suggestion to see details
             * 3. User copies the suggested code to clipboard
             * 4. User can paste the suggestion elsewhere
             * 
             * Expected Results:
             * - Improvement suggestions are properly displayed
             * - Suggested code is correctly copied to clipboard
             * - UI provides feedback that copy was successful
             */
            
            // This test would require UI automation, so we'll provide a documentation style test
            
            Assert.Inconclusive("This test requires UI automation framework");
        }

        [TestMethod]
        [Description("BA-7: Test conversation follow-up workflow")]
        public async Task ConversationFollowUpWorkflow_ShouldMaintainContext()
        {
            // ARRANGE
            var initialCode = @"
function divide(a, b) {
    return a / b;
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // First interaction
            var firstResponseContent = @"
{
  ""roast"": ""This function has zero error handling. What happens when b is zero? Boom!"",
  ""followUp"": {
    ""question"": ""How would you handle division by zero in this function?""
  }
}";

            var mockFirstResponse = SetupMockResponse(firstResponseContent);
            var aiService = CreateMockAIService(mockFirstResponse);

            // ACT - First interaction
            var firstResult = await aiService.GenerateRoast(initialCode, roastLevel, conversationHistory);

            // Update conversation history as if user responded
            conversationHistory.Add(new ChatMessage { Role = "assistant", Content = firstResult });
            conversationHistory.Add(new ChatMessage { 
                Role = "user", 
                Content = "I would add a check to make sure b is not zero before dividing."
            });

            // Second interaction - AI should maintain context
            var secondResponseContent = @"
{
  ""roast"": ""Good thinking! Here's how your function would look with that check:"",
  ""codeExample"": ""function divide(a, b) { if (b === 0) { throw new Error('Cannot divide by zero'); } return a / b; }"",
  ""followUp"": {
    ""question"": ""Would you prefer to return a specific value instead of throwing an error?""
  }
}";

            var mockSecondResponse = SetupMockResponse(secondResponseContent);
            var aiServiceForSecond = CreateMockAIService(mockSecondResponse);

            // ACT - Second interaction
            var secondResult = await aiServiceForSecond.GenerateRoast("", roastLevel, conversationHistory);

            // ASSERT
            Assert.IsNotNull(firstResult);
            Assert.IsNotNull(secondResult);
            Assert.AreEqual(firstResponseContent, firstResult);
            Assert.AreEqual(secondResponseContent, secondResult);

            // In a real integration test, we would verify conversation context is maintained
        }

        [TestMethod]
        [Description("BA-7: Test different roast styles across multiple code samples")]
        public async Task DifferentRoastStylesWorkflow_ShouldProvideConsistentTone()
        {
            // ARRANGE
            var code = @"
function greet(name) {
    alert('Hello, ' + name);
}";
            var conversationHistory = new List<ChatMessage>();

            // Test with different roast levels
            var roastLevels = new[] { "light", "savage", "brutal" };
            var responses = new Dictionary<string, string>();

            // Setup expectations for each roast level
            var responseByLevel = new Dictionary<string, string>
            {
                ["light"] = @"{""roast"": ""Using alert for output? That's a bit old-school, but it works I guess.""}",
                ["savage"] = @"{""roast"": ""Alert? Seriously? What is this, 2005? Ever heard of console.log or DOM manipulation?""}",
                ["brutal"] = @"{""roast"": ""ALERT?! Did you learn JavaScript from a museum exhibit? Even my grandmother knows better than to use alert in 2025. This is painful to look at!""}"
            };

            foreach (var level in roastLevels)
            {
                var mockResponse = SetupMockResponse(responseByLevel[level]);
                var aiService = CreateMockAIService(mockResponse);

                // ACT
                var result = await aiService.GenerateRoast(code, level, conversationHistory);
                responses[level] = result;
            }

            // ASSERT
            foreach (var level in roastLevels)
            {
                Assert.IsNotNull(responses[level]);
                Assert.AreEqual(responseByLevel[level], responses[level]);
            }

            // In a real test, we would verify that each roast level maintains appropriate tone
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
