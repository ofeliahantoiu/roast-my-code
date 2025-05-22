using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RoastMyCode.Tests.TestUtils
{
    /// <summary>
    /// Helper utility for executing specific test categories
    /// </summary>
    public static class TestExecutionHelper
    {
        /// <summary>
        /// Run all tests in a specific category
        /// </summary>
        /// <param name="category">The test category to run</param>
        /// <returns>Test execution results</returns>
        public static TestExecutionResults RunTestsByCategory(string category)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new TestExecutionResults
            {
                Category = category,
                StartTime = DateTime.Now
            };
            
            try
            {
                // Get all test methods with the specified category
                var testMethods = GetTestMethodsByCategory(category);
                results.TotalTests = testMethods.Count;
                
                // Run each test method
                foreach (var method in testMethods)
                {
                    var result = RunTestMethod(method);
                    results.TestResults.Add(result);
                    
                    if (result.Passed)
                    {
                        results.PassedTests++;
                    }
                    else
                    {
                        results.FailedTests++;
                    }
                }
            }
            catch (Exception ex)
            {
                results.ExecutionError = ex.Message;
            }
            
            stopwatch.Stop();
            results.ExecutionTime = stopwatch.Elapsed;
            results.EndTime = DateTime.Now;
            
            return results;
        }
        
        /// <summary>
        /// Run tests for a specific user story
        /// </summary>
        /// <param name="userStoryNumber">The user story number (1-9)</param>
        /// <returns>Test execution results</returns>
        public static TestExecutionResults RunTestsByUserStory(int userStoryNumber)
        {
            // Map user story to test categories
            var categories = MapUserStoryToCategories(userStoryNumber);
            
            var stopwatch = Stopwatch.StartNew();
            var results = new TestExecutionResults
            {
                Category = $"User Story {userStoryNumber}",
                StartTime = DateTime.Now
            };
            
            try
            {
                var allTestMethods = new List<MethodInfo>();
                
                // Get all test methods for each category
                foreach (var category in categories)
                {
                    allTestMethods.AddRange(GetTestMethodsByCategory(category));
                }
                
                // Remove duplicates
                allTestMethods = allTestMethods.Distinct().ToList();
                results.TotalTests = allTestMethods.Count;
                
                // Run each test method
                foreach (var method in allTestMethods)
                {
                    var result = RunTestMethod(method);
                    results.TestResults.Add(result);
                    
                    if (result.Passed)
                    {
                        results.PassedTests++;
                    }
                    else
                    {
                        results.FailedTests++;
                    }
                }
            }
            catch (Exception ex)
            {
                results.ExecutionError = ex.Message;
            }
            
            stopwatch.Stop();
            results.ExecutionTime = stopwatch.Elapsed;
            results.EndTime = DateTime.Now;
            
            return results;
        }
        
        /// <summary>
        /// Generate a test report in Markdown format
        /// </summary>
        /// <param name="results">The test execution results</param>
        /// <returns>Markdown report</returns>
        public static string GenerateMarkdownReport(TestExecutionResults results)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"# Test Execution Report: {results.Category}");
            sb.AppendLine();
            sb.AppendLine($"**Date:** {results.StartTime:yyyy-MM-dd}");
            sb.AppendLine($"**Time:** {results.StartTime:HH:mm:ss} - {results.EndTime:HH:mm:ss}");
            sb.AppendLine($"**Duration:** {results.ExecutionTime.TotalSeconds:F2} seconds");
            sb.AppendLine();
            sb.AppendLine("## Summary");
            sb.AppendLine();
            sb.AppendLine($"- **Total Tests:** {results.TotalTests}");
            sb.AppendLine($"- **Passed:** {results.PassedTests}");
            sb.AppendLine($"- **Failed:** {results.FailedTests}");
            sb.AppendLine($"- **Pass Rate:** {results.PassRate:P2}");
            
            if (!string.IsNullOrEmpty(results.ExecutionError))
            {
                sb.AppendLine();
                sb.AppendLine("## Execution Error");
                sb.AppendLine();
                sb.AppendLine($"```");
                sb.AppendLine(results.ExecutionError);
                sb.AppendLine($"```");
            }
            
            sb.AppendLine();
            sb.AppendLine("## Test Results");
            sb.AppendLine();
            sb.AppendLine("| Test | Result | Duration | Error |");
            sb.AppendLine("|------|--------|----------|-------|");
            
            foreach (var result in results.TestResults)
            {
                var status = result.Passed ? "✅ Pass" : "❌ Fail";
                var error = result.Passed ? "" : result.ErrorMessage;
                sb.AppendLine($"| {result.TestName} | {status} | {result.ExecutionTime.TotalMilliseconds:F0}ms | {error} |");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Save test results to a file
        /// </summary>
        /// <param name="results">The test execution results</param>
        /// <param name="filePath">The file path to save to</param>
        public static void SaveTestResults(TestExecutionResults results, string filePath)
        {
            var report = GenerateMarkdownReport(results);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(filePath, report);
        }
        
        private static List<MethodInfo> GetTestMethodsByCategory(string category)
        {
            var testMethods = new List<MethodInfo>();
            var assembly = Assembly.GetExecutingAssembly();
            
            foreach (var type in assembly.GetTypes())
            {
                // Check if the type has the TestClass attribute
                if (type.GetCustomAttributes(typeof(TestClassAttribute), true).Length > 0)
                {
                    foreach (var method in type.GetMethods())
                    {
                        // Check if the method has the TestMethod attribute
                        if (method.GetCustomAttributes(typeof(TestMethodAttribute), true).Length > 0)
                        {
                            // Check if the method has the TestCategory attribute with the specified category
                            var testCategoryAttributes = method.GetCustomAttributes(typeof(TestCategoryAttribute), true);
                            
                            foreach (TestCategoryAttribute attr in testCategoryAttributes)
                            {
                                if (attr.TestCategories != null && attr.TestCategories.Contains(category))
                                {
                                    testMethods.Add(method);
                                    break;
                                }
                            }
                            
                            // Also check description for BA/QA task references
                            var descriptionAttributes = method.GetCustomAttributes(typeof(DescriptionAttribute), true);
                            
                            foreach (DescriptionAttribute attr in descriptionAttributes)
                            {
                                if (attr.Description != null && attr.Description.Contains(category))
                                {
                                    testMethods.Add(method);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            return testMethods;
        }
        
        private static TestResult RunTestMethod(MethodInfo method)
        {
            var result = new TestResult
            {
                TestName = $"{method.DeclaringType?.Name ?? "UnknownClass"}.{method.Name}"
            };
            
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Create an instance of the test class
                if (method.DeclaringType == null)
                {
                    throw new InvalidOperationException("Cannot create instance of null type");
                }
                var instance = Activator.CreateInstance(method.DeclaringType);
                
                // Call the test method
                method.Invoke(instance, null);
                
                result.Passed = true;
            }
            catch (Exception ex)
            {
                result.Passed = false;
                result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            }
            
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
            
            return result;
        }
        
        private static List<string> MapUserStoryToCategories(int userStoryNumber)
        {
            // Map user stories to test categories based on our mapping
            switch (userStoryNumber)
            {
                case 1: // Paste code into a text box
                    return new List<string> { "QA-1", "BA-1" };
                
                case 2: // Analyse code and generate roast
                    return new List<string> { "QA-2", "BA-2" };
                
                case 3: // Clean and minimalistic UI
                    return new List<string> { "BA-3", "BA-12" };
                
                case 4: // Assign humorous 'developer level'
                    return new List<string> { "QA-3", "BA-5" };
                
                case 5: // Fix/suggest improvements post-roast
                    return new List<string> { "QA-4", "BA-9" };
                
                case 6: // Roast style selection
                    return new List<string> { "QA-5", "BA-4" };
                
                case 7: // Customize appearance of roast output
                    return new List<string> { "QA-6", "BA-8", "BA-11" };
                
                case 8: // Copy roast to clipboard
                    return new List<string> { "QA-7", "BA-6" };
                
                case 9: // Conversational AI follow-up
                    return new List<string> { "QA-8", "BA-10" };
                
                case 10: // Additional tasks
                    return new List<string> { "BA-7", "BA-13", "BA-14", "BA-15" };
                
                default:
                    return new List<string>();
            }
        }
    }
    
    /// <summary>
    /// Results of a test execution
    /// </summary>
    public class TestExecutionResults
    {
        public string Category { get; set; } = string.Empty;
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string ExecutionError { get; set; } = string.Empty;
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();
        
        public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests : 0;
    }
    
    /// <summary>
    /// Result of a single test execution
    /// </summary>
    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
