using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;
//using Azure.AI.OpenAI.Models;
using System;

namespace MultiChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAIController : ControllerBase
    {
        const string apiKey = "API_KEY";
        const string endpoint = "ENDPOINT";
        const string modelId = "MODEL_ID";

        [HttpPost]
        [Route("generateresponse")]
        public async Task<IActionResult> GenerateResponse([FromBody] string prompt)
        {
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

            var chatCompletionsOptions = new ChatCompletionsOptions
            {
                DeploymentName  = modelId,
                Temperature = (float)0.5,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };

            chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(prompt));
            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

            return Ok(response.Value.Choices[0].Message.Content);

        }
    }
}