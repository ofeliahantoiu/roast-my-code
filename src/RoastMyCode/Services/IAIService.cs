using System.Collections.Generic;
using System.Drawing;

namespace RoastMyCode.Services
{
    public interface IAIService
    {
        Task<string> GenerateRoast(string prompt, string roastLevel, List<ChatMessage> conversationHistory);
        Task<string> ProcessFiles(Dictionary<string, string> files, string roastLevel);
        Task<string> RoastImage(Image image, string roastLevel, List<ChatMessage> conversationHistory);
    }
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
