using Microsoft.AspNetCore.Mvc;
using MultiChat.API.Models;
using MultiChat.API.Services;

namespace MultiChat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: api/chat/sessions
        [HttpGet("sessions")]
        public async Task<ActionResult<List<Session>>> GetAllSessions(string tenantId, string userId)
        {
            var sessions = await _chatService.GetAllChatSessionsAsync(tenantId, userId);
            return Ok(sessions);
        }

        // POST: api/chat/session
        [HttpPost("session")]
        public async Task<ActionResult<Session>> CreateSession(string tenantId, string userId)
        {
            var session = await _chatService.CreateNewChatSessionAsync(tenantId, userId);
            return CreatedAtAction(nameof(GetSession), new { tenantId, userId, sessionId = session.SessionId }, session);
        }

        // GET: api/chat/session/{sessionId}
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<Session>> GetSession(string tenantId, string userId, string sessionId)
        {
            var session = await _chatService.GetChatSessionMessagesAsync(tenantId, userId, sessionId);
            if (session == null)
            {
                return NotFound();
            }
            return Ok(session);
        }

        // DELETE: api/chat/session/{sessionId}
        [HttpDelete("session/{sessionId}")]
        public async Task<IActionResult> DeleteSession(string tenantId, string userId, string sessionId)
        {
            await _chatService.DeleteChatSessionAsync(tenantId, userId, sessionId);
            return NoContent();
        }

        // POST: api/chat/session/{sessionId}/message
        [HttpPost("session/{sessionId}/message")]
        public async Task<ActionResult<Message>> CreateMessage(string tenantId, string userId, string sessionId, [FromBody] string promptText)
        {
            var message = await _chatService.GetChatCompletionAsync(tenantId, userId, sessionId, promptText);
            return Ok(message);
        }

        // GET: api/chat/session/{sessionId}/messages
        [HttpGet("session/{sessionId}/messages")]
        public async Task<ActionResult<List<Message>>> GetMessages(string tenantId, string userId, string sessionId)
        {
            var messages = await _chatService.GetChatSessionMessagesAsync(tenantId, userId, sessionId);
            return Ok(messages);
        }

        // PUT: api/chat/session/{sessionId}/rename
        [HttpPut("session/{sessionId}/rename")]
        public async Task<IActionResult> RenameSession(string tenantId, string userId, string sessionId, [FromBody] string newName)
        {
            await _chatService.RenameChatSessionAsync(tenantId, userId, sessionId, newName);
            return NoContent();
        }

        // POST: api/chat/session/{sessionId}/summarize
        [HttpPost("session/{sessionId}/summarize")]
        public async Task<ActionResult<string>> SummarizeSessionName(string tenantId, string userId, string sessionId)
        {
            var newName = await _chatService.SummarizeChatSessionNameAsync(tenantId, userId, sessionId);
            return Ok(newName);
        }
    }
}