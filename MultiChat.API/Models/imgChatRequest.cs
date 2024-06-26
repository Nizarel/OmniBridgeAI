
using System.Text.Json.Serialization;
using System.Text.Json;
using SkiaSharp;

namespace MultiChat.API.Models
{
    public class imgChatRequest
    {
        public string SessionId { get; set; }
        public string? PromptText { get; set; }
        public Stream? imageFile { get; set; }
    }
}

