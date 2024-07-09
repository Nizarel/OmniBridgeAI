namespace MultiChat.API.Models
{
    public class speechChat
    {
        public string SessionId { get; set; }
        public IFormFile? audioFile { get; set; }
    }
}
