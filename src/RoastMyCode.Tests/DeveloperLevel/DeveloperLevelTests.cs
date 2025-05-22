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
        
        [TestMethod]
        [Description("QA-3/BA-5: Test developer level for code with syntax errors")]
        public async Task CodeWithSyntaxErrors_ShouldStillAssignLevel()
        {
            // Arrange
            var codeWithErrors = @"
function calculateSum(a, b) {
    // Missing semicolon
    let result = a + b
    // Missing closing brace for if statement
    if (result > 10 {
        console.log('Result is greater than 10');
    }
    return result;
}";
            var roastLevel = "savage";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var responseContent = @"
{""developerLevel"": ""Syntax Struggler"", 
""score"": 5, 
""message"": ""Your code has so many syntax errors it's like you're trying to communicate with the computer in interpretive dance. Spoiler alert: computers don't appreciate art.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(codeWithErrors, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Syntax Struggler"));
            Assert.IsTrue(result.Contains("5"));
        }
        
        [TestMethod]
        [Description("QA-3/BA-5: Test developer level for code with mixed paradigms")]
        public async Task MixedParadigmCode_ShouldReceiveSpecificFeedback()
        {
            // Arrange
            var mixedCode = @"
// Mixing functional and OOP paradigms
class DataProcessor {
    constructor(data) {
        this.data = data;
    }
    
    // OOP style method
    processData() {
        return this.data.map(item => this.transformItem(item));
    }
    
    // Functional style method
    transformItem(item) {
        return item * 2;
    }
}

// Procedural style outside the class
function processAllData(dataArray) {
    let results = [];
    for (let i = 0; i < dataArray.length; i++) {
        results.push(dataArray[i] * 2);
    }
    return results;
}";
            var roastLevel = "brutal";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var responseContent = @"
{""developerLevel"": ""Paradigm Juggler"", 
""score"": 45, 
""message"": ""You're mixing programming paradigms like you're making a cocktail. Pick a lane! Your code looks like it has multiple personality disorder.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(mixedCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Paradigm Juggler"));
            Assert.IsTrue(result.Contains("45"));
        }
        
        [TestMethod]
        [Description("QA-3/BA-5: Test developer level for extremely complex code")]
        public async Task ExtremelyComplexCode_ShouldReceiveHighestLevel()
        {
            // Arrange
            var complexCode = @"
// Advanced design patterns and algorithms
class EventEmitter {
    constructor() {
        this.events = new Map();
    }

    on(event, listener) {
        if (!this.events.has(event)) {
            this.events.set(event, []);
        }
        this.events.get(event).push(listener);
        return () => this.off(event, listener);
    }

    off(event, listener) {
        if (!this.events.has(event)) return;
        const listeners = this.events.get(event);
        const index = listeners.indexOf(listener);
        if (index !== -1) listeners.splice(index, 1);
    }

    emit(event, ...args) {
        if (!this.events.has(event)) return;
        const listeners = this.events.get(event);
        listeners.forEach(listener => listener(...args));
    }

    once(event, listener) {
        const remove = this.on(event, (...args) => {
            remove();
            listener(...args);
        });
        return remove;
    }
}";
            var roastLevel = "light";
            var conversationHistory = new List<ChatMessage>();

            // Setup mock response
            var responseContent = @"
{""developerLevel"": ""Design Pattern Virtuoso"", 
""score"": 95, 
""message"": ""I'm impressed. Your implementation of the observer pattern is elegant and efficient. The use of closures for the unsubscribe functionality is particularly clever.""}";
            
            var mockResponse = SetupMockResponseWithDeveloperLevel(responseContent);
            var aiService = CreateMockAIService(mockResponse);

            // Act
            var result = await aiService.GenerateRoast(complexCode, roastLevel, conversationHistory);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Design Pattern Virtuoso"));
            Assert.IsTrue(result.Contains("95"));
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
