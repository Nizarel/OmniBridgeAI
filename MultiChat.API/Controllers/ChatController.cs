using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using MultiChat.API.Models;
using MultiChat.API.Services;
using SkiaSharp;


namespace MultiChat.API.Controllers
{
    /// <summary>
    /// Controller for managing chat sessions and messages.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Retrieves all chat sessions.
        /// </summary>
        /// <returns>A list of chat sessions.</returns>
        [HttpGet("sessions")]
        public async Task<ActionResult<List<Session>>> GetAllSessions()
        {
            var sessions = await _chatService.GetAllChatSessionsAsync();
            return Ok(sessions);
        }

        /// <summary>
        /// Retrieves a specific chat session by its ID.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <returns>The chat session if found; otherwise, NotFound.</returns>
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<Session>> GetSession(string sessionId)
        {
            try
            {
                var session = await _chatService.GetChatSessionsAsync(sessionId);
                if (session == null || session.Count == 0) // session.Count == 0
                {
                    return NotFound();
                }
                return Ok(session);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <returns>The created chat session.</returns>
        //[HttpPost("session")]
        [HttpPost("session/{sessionId}")]
        public async Task<ActionResult<Session>> AddSession(string sessionId)
        {
            var session = await _chatService.CreateNewChatSessionAsync(sessionId);
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }

/*        [HttpPost]
        public async Task<ActionResult<Session>> CreateSession([FromBody] ChatRequest request)
        {
            // Add 'await' operator to await the non-blocking API call
            var session = await _chatService.CreateNewChatSessionAsync(request.SessionId);
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }*/

        /// <summary>
        /// Renames an existing chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session to rename.</param>
        /// <param name="newName">The new name for the chat session.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpPut("session/{sessionId}/rename")]
        public async Task<IActionResult> RenameSession(string sessionId, [FromBody] string newName)
        {
            await _chatService.RenameChatSessionAsync(sessionId, newName);
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session to delete.</param>
        /// <returns>An IActionResult indicating success or failure.</returns>
        [HttpDelete("session/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            await _chatService.DeleteChatSessionAsync(sessionId);
            return NoContent();
        }

        /// <summary>
        /// Processes a chat request to get a chat completion.
        /// </summary>
        /// <param name="request">The chat request containing session ID and prompt text.</param>
        /// <returns>The chat completion message.</returns>
        [HttpPost]
        public async Task<IActionResult> GetChatCompletion([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.SessionId) || string.IsNullOrEmpty(request.PromptText))
            {
                return BadRequest("Invalid request");
            }

            try
            {
                var message = await _chatService.GetChatCompletionAsync(request.SessionId, request.PromptText);
                return Ok(message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        /// <summary>
        /// Creates a message in a specific chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <param name="promptText">The prompt text for the message.</param>
        /// <returns>The created message.</returns>
        [HttpPost("session/{sessionId}/message")]
        public async Task<ActionResult<Message>> CreateMessage(string sessionId, [FromBody] string promptText)
        {
            var message = await _chatService.GetChatCompletionAsync(sessionId, promptText);
            return Ok(message);
        }


        // ...


        [HttpPost("image")]
        public async Task<IActionResult> ImageCompletion([FromBody] imgChatRequest request)
        {
            if (request.imageFile == null)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                byte[] imageArray = Stream2Base64Async(request.imageFile);

                // Convert image data to base64 string  
                string base64Image = Convert.ToBase64String(imageArray);

                var message = await _chatService.GetImage2TextCompletionAsync(request.SessionId, request.PromptText, base64Image);

                return Ok(message);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<Message>> AnalyzeImage([FromForm] ChatRequest request)
        {
            if (request.imageFile == null || request.imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                byte[] imageArray = Stream2Base64Async(request.imageFile.OpenReadStream());

                // Convert image data to base64 string  
                string base64Image = Convert.ToBase64String(imageArray);

                // Prepare the request to Azure OpenAI GPT-4  
                // var prompt = $"{request.PromptText}: {base64Image}";

                var message = await _chatService.GetImage2TextCompletionAsync(request.SessionId, request.PromptText, base64Image);


                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all messages for a specific chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <returns>A list of messages for the chat session.</returns>
        [HttpGet("session/{sessionId}/messages")]
        public async Task<ActionResult<List<Message>>> GetMessages(string sessionId)
        {
            var messages = await _chatService.GetChatSessionMessagesAsync(sessionId);
            return Ok(messages);
        }

        /// <summary>
        /// Summarizes the name of a chat session.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session to summarize.</param>
        /// <returns>The summarized name of the chat session.</returns>
        [HttpPost("session/{sessionId}/summarize")]
        public async Task<ActionResult<string>> SummarizeSessionName(string sessionId)
        {
            var newName = await _chatService.SummarizeChatSessionNameAsync(sessionId);
            return Ok(newName);
        }


        private static byte[] Stream2Base64Async(Stream stream)
        {
            using var skData = SKData.Create(stream);
            using var skCodec = SKCodec.Create(skData);
            using var originalBitmap = SKBitmap.Decode(skCodec);

            SKImageInfo resizedInfo = new(640, 480);
            using var resizedBitmap = originalBitmap.Resize(resizedInfo, SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
 
            return data.ToArray();
        }
/*        private static ReadOnlyMemory<byte> ConvertImageToReadOnlyMemory(Image image)
        {
            using var memoryStream = new MemoryStream();
            // Ensure the image is in the correct format (e.g., RGB)
            image.Mutate(x => x.AutoOrient());

            // Save the image to the MemoryStream, using JPEG format
            image.SaveAsJpeg(memoryStream, new JpegEncoder());

            // Optionally, reset the position of the MemoryStream to the beginning
            memoryStream.Position = 0;

            // Convert the MemoryStream's buffer to ReadOnlyMemory<byte>
            return new ReadOnlyMemory<byte>(memoryStream.ToArray());
        }*/
    }
}
