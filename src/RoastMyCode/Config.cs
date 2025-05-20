using System;
using System.IO;

namespace RoastMyCode
{
    /// <summary>
    /// Handles configuration and environment variables for the application
    /// </summary>
    public static class Config
    {
        private static string _apiKey;

        /// <summary>
        /// Gets the OpenAI API key from the .env file
        /// </summary>
        public static string OpenAIApiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    LoadApiKey();
                }
                return _apiKey;
            }
        }

        /// <summary>
        /// Loads the API key from the .env file
        /// </summary>
        private static void LoadApiKey()
        {
            try
            {
                string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
                if (!File.Exists(envPath))
                {
                    throw new FileNotFoundException("The .env file was not found. Please create one with your OpenAI API key.");
                }

                string[] lines = File.ReadAllLines(envPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                    {
                        _apiKey = line.Substring("OPENAI_API_KEY=".Length).Trim();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_apiKey))
                {
                    throw new Exception("OPENAI_API_KEY not found in .env file.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load API key from .env file.", ex);
            }
        }
    }
} 