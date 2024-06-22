/*using Microsoft.AspNetCore.Mvc;
using MultiChat.API.Models;

using MultiChat.API.Services;

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

    // Existing code...

    /// <summary>
    /// Analyzes an image and returns the chat completion message.
    /// </summary>
    /// <param name="imageFile">The image file to analyze.</param>
    /// <param name="request">The chat request containing session ID and prompt text.</param>
    /// <returns>The chat completion message.</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<Message>> AnalyzeImage([FromForm] IFormFile imageFile, [FromBody] ChatRequest request)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest("Image file is required.");
        }

        try
        {
            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                await imageFile.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            // Convert image data to base64 string  
            string base64Image = Convert.ToBase64String(imageData);

            // Prepare the request to Azure OpenAI GPT-4  
            var prompt = $"{request.PromptText}: {base64Image}";

            var message = await _chatService.GetChatCompletionAsync(request.SessionId, prompt);

            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}








*//*using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using MultiChat.API.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MultiChat.API.Controllers;
{
    [ApiController]
    [Route("[controller]")]
    public class ImageAnalysisController : ControllerBase
    {
        private readonly SemanticKernel _semanticKernel;

        public ImageAnalysisController(ISemanticKernel semanticKernel)
        {
            _semanticKernel = semanticKernel;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile, string captionPrm)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                byte[] imageData;
                using (var ms = new MemoryStream())
                {
                    await imageFile.CopyToAsync(ms);
                    imageData = ms.ToArray();
                }

                // Convert image data to base64 string  
                string base64Image = Convert.ToBase64String(imageData);

                // Prepare the request to Azure OpenAI GPT-4  
                var prompt = $"{captionPrm}: {base64Image}";

                var result = await _semanticKernel.TextCompletionAsync(prompt);

                return Ok(new { analysis = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}*/