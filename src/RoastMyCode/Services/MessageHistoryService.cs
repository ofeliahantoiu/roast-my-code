using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoastMyCode.Services
{
    public class MessageHistoryService : IDisposable
    {
        private const string HistoryFileName = "message_history.json";
        private readonly string _historyPath;
        private bool _disposed = false;

        public MessageHistoryService()
        {
            _historyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "RoastMyCode", HistoryFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(_historyPath)!);
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            var history = await LoadHistoryAsync();
            history.Add(message);
            await SaveHistoryAsync(history);
        }

        public async Task<List<ChatMessage>> LoadHistoryAsync()
        {
            if (!File.Exists(_historyPath))
                return new List<ChatMessage>();

            try
            {
                var json = await File.ReadAllTextAsync(_historyPath);
                return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading message history: {ex.Message}");
                return new List<ChatMessage>();
            }
        }

        private async Task SaveHistoryAsync(List<ChatMessage> history)
        {
            try
            {
                var json = JsonSerializer.Serialize(history);
                await File.WriteAllTextAsync(_historyPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving message history: {ex.Message}");
            }
        }

        public Task ClearHistoryAsync()
        {
            try
            {
                if (File.Exists(_historyPath))
                    File.Delete(_historyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing message history: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
