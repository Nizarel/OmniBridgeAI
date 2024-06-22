using Microsoft.AspNetCore.Mvc;
using MultiChat.API.Models;
using MultiChat.API.Options;
using MultiChat.API.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var session = await _chatService.GetChatSessionMessagesAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <returns>The created chat session.</returns>
        [HttpPost("session")]
        public async Task<ActionResult<Session>> CreateSession()
        {
            var session = await _chatService.CreateNewChatSessionAsync();
            return CreatedAtAction(nameof(GetSession), new { sessionId = session.SessionId }, session);
        }

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


        [HttpPost("analyze")]
        public async Task<ActionResult<Message>> AnalyzeImage([FromForm] ChatRequest request)
        {
            if (request.imageFile == null || request.imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                byte[] imageData;
                using (var ms = new MemoryStream())
                {
                    await request.imageFile.CopyToAsync(ms);
                    imageData = ms.ToArray();
                }

                // Convert image data to base64 string  
                string base64Image = Convert.ToBase64String(imageData);

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
    }
}


/*
 using Microsoft.AspNetCore.Mvc;  
using System.Net.Http.Headers;  
using System.Text;  
using System.Text.Json;  
  
namespace OpenAIPictureUploadAPI.Controllers  
{  
    [Route("api/[controller]")]  
    [ApiController]  
    public class UploadController : ControllerBase  
    {  
        private readonly IHttpClientFactory _httpClientFactory;  
        private readonly IConfiguration _configuration;  
  
        public UploadController(IHttpClientFactory httpClientFactory, IConfiguration configuration)  
        {  
            _httpClientFactory = httpClientFactory;  
            _configuration = configuration;  
        }  
  
        [HttpPost("upload")]  
        public async Task<IActionResult> Upload(IFormFile file)  
        {  
            if (file == null || file.Length == 0)  
                return BadRequest("No file uploaded.");  
  
            // Convert the image to base64  
            string base64Image;  
            using (var ms = new MemoryStream())  
            {  
                await file.CopyToAsync(ms);  
                var fileBytes = ms.ToArray();  
                base64Image = Convert.ToBase64String(fileBytes);  
            }  
  
            // Create the prompt for OpenAI  
            var prompt = $"Here is an image: {base64Image}";  
  
            // Prepare the request to OpenAI  
            var httpClient = _httpClientFactory.CreateClient();  
            var requestContent = new  
            {  
                prompt,  
                max_tokens = 100,  
                temperature = 0.7  
            };  
  
            var httpContent = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");  
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);  
  
            var response = await httpClient.PostAsync(_configuration["OpenAI:Endpoint"], httpContent);  
  
            if (!response.IsSuccessStatusCode)  
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());  
  
            var responseContent = await response.Content.ReadAsStringAsync();  
            return Ok(responseContent);  
        }  
    }  
}  




using Microsoft.AspNetCore.Mvc;  
using System.Net.Http.Headers;  
using System.Text;  
using System.Text.Json;  
  
namespace OpenAIPictureUploadAPI.Controllers  
{  
    [Route("api/[controller]")]  
    [ApiController]  
    public class UploadController : ControllerBase  
    {  
        private readonly IHttpClientFactory _httpClientFactory;  
        private readonly IConfiguration _configuration;  
  
        public UploadController(IHttpClientFactory httpClientFactory, IConfiguration configuration)  
        {  
            _httpClientFactory = httpClientFactory;  
            _configuration = configuration;  
        }  
  
        [HttpPost("upload")]  
        public async Task<IActionResult> Upload(IFormFile file)  
        {  
            if (file == null || file.Length == 0)  
                return BadRequest("No file uploaded.");  
  
            // Convert the image to base64  
            string base64Image;  
            using (var ms = new MemoryStream())  
            {  
                await file.CopyToAsync(ms);  
                var fileBytes = ms.ToArray();  
                base64Image = Convert.ToBase64String(fileBytes);  
            }  
  
            // Create the prompt for OpenAI  
            var prompt = $"Here is an image: {base64Image}";  
  
            // Prepare the request to OpenAI  
            var httpClient = _httpClientFactory.CreateClient();  
            var requestContent = new  
            {  
                prompt,  
                max_tokens = 100,  
                temperature = 0.7  
            };  
  
            var httpContent = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");  
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["OpenAI:ApiKey"]);  
  
            var response = await httpClient.PostAsync(_configuration["OpenAI:Endpoint"], httpContent);  
  
            if (!response.IsSuccessStatusCode)  
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());  
  
            var responseContent = await response.Content.ReadAsStringAsync();  
            return Ok(responseContent);  
        }  
    }  
}  

*/