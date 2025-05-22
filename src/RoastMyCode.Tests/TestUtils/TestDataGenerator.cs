using System;
using System.Collections.Generic;

namespace RoastMyCode.Tests.TestUtils
{
    /// <summary>
    /// Generates test data for various test scenarios
    /// </summary>
    public static class TestDataGenerator
    {
        private static readonly Random _random = new Random();
        
        /// <summary>
        /// Get a sample code snippet for testing
        /// </summary>
        /// <param name="language">Programming language</param>
        /// <param name="complexity">Code complexity level</param>
        /// <param name="hasErrors">Whether the code should contain errors</param>
        /// <returns>A code snippet</returns>
        public static string GetSampleCode(CodeLanguage language, CodeComplexity complexity, bool hasErrors = false)
        {
            switch (language)
            {
                case CodeLanguage.CSharp:
                    return GetCSharpSample(complexity, hasErrors);
                case CodeLanguage.JavaScript:
                    return GetJavaScriptSample(complexity, hasErrors);
                case CodeLanguage.Python:
                    return GetPythonSample(complexity, hasErrors);
                case CodeLanguage.Java:
                    return GetJavaSample(complexity, hasErrors);
                default:
                    return GetCSharpSample(complexity, hasErrors);
            }
        }
        
        /// <summary>
        /// Get a random roast style
        /// </summary>
        /// <returns>Roast style name</returns>
        public static string GetRandomRoastStyle()
        {
            var styles = new[] { "light", "savage", "brutal" };
            return styles[_random.Next(styles.Length)];
        }
        
        /// <summary>
        /// Get a random developer level
        /// </summary>
        /// <returns>Developer level name</returns>
        public static string GetRandomDeveloperLevel()
        {
            var levels = new[] { "Beginner", "Intermediate", "Advanced" };
            return levels[_random.Next(levels.Length)];
        }
        
        /// <summary>
        /// Generate a conversation history for testing
        /// </summary>
        /// <param name="messageCount">Number of messages to generate</param>
        /// <returns>List of chat messages</returns>
        public static List<ChatMessage> GenerateConversationHistory(int messageCount)
        {
            var history = new List<ChatMessage>();
            
            for (int i = 0; i < messageCount; i++)
            {
                var role = i % 2 == 0 ? "user" : "assistant";
                var content = GetRandomMessage(role);
                
                history.Add(new ChatMessage
                {
                    Role = role,
                    Content = content
                });
            }
            
            return history;
        }
        
        private static string GetCSharpSample(CodeComplexity complexity, bool hasErrors)
        {
            switch (complexity)
            {
                case CodeComplexity.Simple:
                    return hasErrors
                        ? "public class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello, World!\") // Missing semicolon\n    }\n}"
                        : "public class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello, World!\");\n    }\n}";
                
                case CodeComplexity.Moderate:
                    return hasErrors
                        ? "public class Calculator\n{\n    public int Add(int a, int b)\n    {\n        return a + b;\n    }\n    \n    public int Subtract(int a, int b)\n    {\n        return a - b\n    }\n    \n    public int Multiply(int a, int b)\n    {\n        return a * b;\n    }\n    \n    public int Divide(int a, int b)\n    {\n        return a / b; // No null check\n    }\n}"
                        : "public class Calculator\n{\n    public int Add(int a, int b)\n    {\n        return a + b;\n    }\n    \n    public int Subtract(int a, int b)\n    {\n        return a - b;\n    }\n    \n    public int Multiply(int a, int b)\n    {\n        return a * b;\n    }\n    \n    public int Divide(int a, int b)\n    {\n        if (b == 0)\n            throw new DivideByZeroException();\n        return a / b;\n    }\n}";
                
                case CodeComplexity.Complex:
                    return hasErrors
                        ? "public class DataProcessor\n{\n    private readonly List<string> _data;\n    \n    public DataProcessor(List<string> data)\n    {\n        _data = data;\n    }\n    \n    public IEnumerable<string> ProcessData()\n    {\n        var result = new List<string>();\n        \n        foreach (var item in _data)\n        {\n            if (string.IsNullOrEmpty(item))\n                continue;\n            \n            var processed = item.Trim().ToUpper();\n            result.Add(processed);\n        }\n        \n        return result\n    }\n    \n    public Dictionary<string, int> CountOccurrences()\n    {\n        var counts = new Dictionary<string, int>();\n        \n        foreach (var item in _data)\n        {\n            if (counts.ContainsKey(item))\n                counts[item]++;\n            else\n                counts.Add(item, 1);\n        }\n        \n        return counts;\n    }\n}"
                        : "public class DataProcessor\n{\n    private readonly List<string> _data;\n    \n    public DataProcessor(List<string> data)\n    {\n        _data = data ?? new List<string>();\n    }\n    \n    public IEnumerable<string> ProcessData()\n    {\n        var result = new List<string>();\n        \n        foreach (var item in _data)\n        {\n            if (string.IsNullOrEmpty(item))\n                continue;\n            \n            var processed = item.Trim().ToUpper();\n            result.Add(processed);\n        }\n        \n        return result;\n    }\n    \n    public Dictionary<string, int> CountOccurrences()\n    {\n        var counts = new Dictionary<string, int>();\n        \n        foreach (var item in _data)\n        {\n            if (string.IsNullOrEmpty(item))\n                continue;\n                \n            if (counts.ContainsKey(item))\n                counts[item]++;\n            else\n                counts.Add(item, 1);\n        }\n        \n        return counts;\n    }\n}";
                
                default:
                    return "// Sample code";
            }
        }
        
        private static string GetJavaScriptSample(CodeComplexity complexity, bool hasErrors)
        {
            switch (complexity)
            {
                case CodeComplexity.Simple:
                    return hasErrors
                        ? "function greet(name) {\n    console.log(\"Hello, \" + name)\n} // Missing semicolon\n\ngreet(\"World\");"
                        : "function greet(name) {\n    console.log(\"Hello, \" + name);\n}\n\ngreet(\"World\");";
                
                case CodeComplexity.Moderate:
                    return hasErrors
                        ? "class Calculator {\n    add(a, b) {\n        return a + b;\n    }\n    \n    subtract(a, b) {\n        return a - b\n    }\n    \n    multiply(a, b) {\n        return a * b;\n    }\n    \n    divide(a, b) {\n        return a / b; // No null check\n    }\n}\n\nconst calc = new Calculator();\nconsole.log(calc.add(5, 3));"
                        : "class Calculator {\n    add(a, b) {\n        return a + b;\n    }\n    \n    subtract(a, b) {\n        return a - b;\n    }\n    \n    multiply(a, b) {\n        return a * b;\n    }\n    \n    divide(a, b) {\n        if (b === 0) {\n            throw new Error(\"Cannot divide by zero\");\n        }\n        return a / b;\n    }\n}\n\nconst calc = new Calculator();\nconsole.log(calc.add(5, 3));";
                
                default:
                    return "// Sample JavaScript code";
            }
        }
        
        private static string GetPythonSample(CodeComplexity complexity, bool hasErrors)
        {
            switch (complexity)
            {
                case CodeComplexity.Simple:
                    return hasErrors
                        ? "def greet(name)\n    print(f\"Hello, {name}\")\n\ngreet(\"World\")"
                        : "def greet(name):\n    print(f\"Hello, {name}\")\n\ngreet(\"World\")";
                
                case CodeComplexity.Moderate:
                    return hasErrors
                        ? "class Calculator:\n    def add(self, a, b):\n        return a + b\n    \n    def subtract(self, a, b):\n        return a - b\n    \n    def multiply(self, a, b)\n        return a * b\n    \n    def divide(self, a, b):\n        return a / b  # No zero check\n\ncalc = Calculator()\nprint(calc.add(5, 3))"
                        : "class Calculator:\n    def add(self, a, b):\n        return a + b\n    \n    def subtract(self, a, b):\n        return a - b\n    \n    def multiply(self, a, b):\n        return a * b\n    \n    def divide(self, a, b):\n        if b == 0:\n            raise ValueError(\"Cannot divide by zero\")\n        return a / b\n\ncalc = Calculator()\nprint(calc.add(5, 3))";
                
                default:
                    return "# Sample Python code";
            }
        }
        
        private static string GetJavaSample(CodeComplexity complexity, bool hasErrors)
        {
            switch (complexity)
            {
                case CodeComplexity.Simple:
                    return hasErrors
                        ? "public class HelloWorld {\n    public static void main(String[] args) {\n        System.out.println(\"Hello, World!\")\n    }\n}"
                        : "public class HelloWorld {\n    public static void main(String[] args) {\n        System.out.println(\"Hello, World!\");\n    }\n}";
                
                default:
                    return "// Sample Java code";
            }
        }
        
        private static string GetRandomMessage(string role)
        {
            if (role == "user")
            {
                var userMessages = new[]
                {
                    "Can you roast this code?",
                    "What do you think of my implementation?",
                    "How can I improve this function?",
                    "Is this code efficient?",
                    "What's wrong with my approach here?"
                };
                
                return userMessages[_random.Next(userMessages.Length)];
            }
            else
            {
                var assistantMessages = new[]
                {
                    "Your code looks like it was written by a sleep-deprived squirrel.",
                    "I've seen more elegant code in a first-year student's homework.",
                    "This implementation is actually quite good, but there are a few improvements you could make.",
                    "Well, at least it works... technically.",
                    "Did you intentionally make this as complicated as possible?"
                };
                
                return assistantMessages[_random.Next(assistantMessages.Length)];
            }
        }
    }
    
    /// <summary>
    /// Programming languages for test data
    /// </summary>
    public enum CodeLanguage
    {
        CSharp,
        JavaScript,
        Python,
        Java
    }
    
    /// <summary>
    /// Code complexity levels
    /// </summary>
    public enum CodeComplexity
    {
        Simple,
        Moderate,
        Complex
    }
    
    /// <summary>
    /// Chat message for conversation history
    /// </summary>
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
