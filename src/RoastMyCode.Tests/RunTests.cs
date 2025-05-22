using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoastMyCode.Tests.TestUtils;
using System;
using System.IO;
using System.Linq;

namespace RoastMyCode.Tests
{
    [TestClass]
    public class RunTests
    {
        [TestMethod]
        public void RunAllTests()
        {
            // This is a manual test method that can be run to execute all tests
            // and generate a report using our test utilities
            
            Console.WriteLine("Running all tests...");
            
            // Create a test execution helper
            var results = TestExecutionHelper.RunTestsByCategory("QA-1");
            
            // Print results
            Console.WriteLine($"Total tests: {results.TotalTests}");
            Console.WriteLine($"Passed: {results.PassedTests}");
            Console.WriteLine($"Failed: {results.FailedTests}");
            Console.WriteLine($"Pass rate: {results.PassRate:P2}");
            
            // Generate a report
            var report = TestExecutionHelper.GenerateMarkdownReport(results);
            
            // Save the report to a file
            var reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestReport.md");
            var directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(reportPath, report);
            
            Console.WriteLine($"Report saved to: {reportPath}");
            
            // Assert that all tests passed
            Assert.AreEqual(results.TotalTests, results.PassedTests, "Not all tests passed");
        }
        
        [TestMethod]
        public void TestDataGeneratorDemo()
        {
            // Generate sample code for different languages and complexity levels
            var csharpSimple = TestDataGenerator.GetSampleCode(CodeLanguage.CSharp, CodeComplexity.Simple);
            var jsModerate = TestDataGenerator.GetSampleCode(CodeLanguage.JavaScript, CodeComplexity.Moderate);
            var pythonComplex = TestDataGenerator.GetSampleCode(CodeLanguage.Python, CodeComplexity.Complex, true);
            
            // Generate a conversation history
            var history = TestDataGenerator.GenerateConversationHistory(4);
            
            // Print the generated data
            Console.WriteLine("C# Simple Sample:");
            Console.WriteLine(csharpSimple);
            Console.WriteLine();
            
            Console.WriteLine("JavaScript Moderate Sample:");
            Console.WriteLine(jsModerate);
            Console.WriteLine();
            
            Console.WriteLine("Python Complex Sample with Errors:");
            Console.WriteLine(pythonComplex);
            Console.WriteLine();
            
            Console.WriteLine("Conversation History:");
            foreach (var message in history)
            {
                Console.WriteLine($"{message.Role}: {message.Content}");
            }
            
            // Verify the data was generated
            Assert.IsFalse(string.IsNullOrEmpty(csharpSimple));
            Assert.IsFalse(string.IsNullOrEmpty(jsModerate));
            Assert.IsFalse(string.IsNullOrEmpty(pythonComplex));
            Assert.AreEqual(4, history.Count);
        }
        
        [TestMethod]
        public void EnhancedMockHttpHandlerDemo()
        {
            // Create an enhanced mock HTTP handler
            var mockHandler = new EnhancedMockHttpMessageHandler();
            
            // Configure the mock handler
            mockHandler.SimulateNetworkLatency = true;
            mockHandler.MinLatencyMs = 50;
            mockHandler.MaxLatencyMs = 200;
            mockHandler.ErrorRate = 0.1; // 10% chance of error
            
            // Add some mock responses
            mockHandler.AddMockOpenAIResponse("https://api.openai.com/v1/chat/completions", 
                "This code looks like it was written by a sleep-deprived squirrel.");
            
            mockHandler.AddMockRateLimitResponse("https://api.openai.com/v1/rate-limited");
            
            // Create a dynamic response
            mockHandler.AddDynamicResponse("https://api.openai.com/v1/dynamic", request => 
            {
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent($"Received request at: {DateTime.Now}")
                };
            });
            
            // Create an HTTP client with the mock handler
            var httpClient = new System.Net.Http.HttpClient(mockHandler);
            
            // Make a request
            var response = httpClient.GetAsync("https://api.openai.com/v1/chat/completions").Result;
            
            // Print the response
            var content = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine("Response:");
            Console.WriteLine(content);
            
            // Check the request logs
            var logs = mockHandler.GetRequestLogs();
            Console.WriteLine($"Request count: {logs.Count}");
            
            // Verify the response
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(content.Contains("squirrel"));
        }
        
        [TestMethod]
        public void TestCoordinationUtilityDemo()
        {
            // Create a test coordination utility
            var tempPath = Path.Combine(Path.GetTempPath(), "test_feedback.json");
            var coordinator = new TestCoordinationUtility(tempPath);
            
            // Submit some test feedback
            coordinator.SubmitFeedbackAsync(new TestFeedback
            {
                TesterName = "John Doe",
                Feature = "Code Input",
                Description = "The code input box doesn't show line numbers",
                Priority = FeedbackPriority.Medium,
                Environment = "Windows 11, Chrome"
            }).Wait();
            
            coordinator.SubmitFeedbackAsync(new TestFeedback
            {
                TesterName = "Jane Smith",
                Feature = "Roast Generation",
                Description = "Roast takes too long to generate",
                Priority = FeedbackPriority.High,
                Environment = "macOS, Safari"
            }).Wait();
            
            // Get all feedback
            var allFeedback = coordinator.GetAllFeedbackAsync().Result;
            
            // Get high priority feedback
            var highPriorityFeedback = coordinator.GetFeedbackByPriorityAsync(FeedbackPriority.High).Result;
            
            // Generate a summary report
            var summary = coordinator.GenerateSummaryReportAsync().Result;
            
            // Print the results
            Console.WriteLine($"Total feedback: {allFeedback.Count}");
            Console.WriteLine($"High priority feedback: {highPriorityFeedback.Count}");
            Console.WriteLine($"Feedback by feature: {string.Join(", ", summary.FeedbackByFeature.Select(kv => $"{kv.Key}: {kv.Value}"))}");
            
            // Clean up
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            
            // Verify the results
            Assert.AreEqual(2, allFeedback.Count);
            Assert.AreEqual(1, highPriorityFeedback.Count);
            Assert.AreEqual(2, summary.FeedbackByFeature.Count);
        }
    }
}
