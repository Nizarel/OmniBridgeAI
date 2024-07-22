
namespace MultiChat.API.Models
{
    public class imgChatRequest
    {
        public string SessionId { get; set; }
        public string? PromptText { get; set; }
        public string? imageFile { get; set; }
    }
}

