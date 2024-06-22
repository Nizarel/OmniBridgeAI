using SixLabors.ImageSharp;

namespace MultiChat.API.Models
{
    public class imgChatRequest
    {
        public string SessionId { get; set; }
        public string? PromptText { get; set; }
        public Image? imageFile { get; set; }
    }
}
