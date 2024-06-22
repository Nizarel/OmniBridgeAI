namespace MultiChat.API.Models
{
    public class ChatRequest
    {
        public string SessionId { get; set; }
        public string? PromptText { get; set; }
        public IFormFile? imageFile { get; set; }
    }
}
