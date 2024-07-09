using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.AudioToText;
using MultiChat.API.Plugins.PlaceSuggestionsPlugin;
using MultiChat.API.Models;
using Azure.AI.OpenAI;



#pragma warning disable SKEXP0010, SKEXP0001, SKEXP0050

namespace MultiChat.API.Services

{
    public class SemanticKernelService
    {

        readonly Kernel kernel;

        /// System prompt to send with user prompts to instruct the model for chat session
        private readonly string _systemPrompt = @"
        You are an AI assistant that helps people to shop online through WhatsApp chat!
        You should answer questions about products, provide recommendations, and help users to make decisions.";

        /// System prompt to send with user prompts to instruct the model for summarization
        private readonly string _summarizePrompt = @"
        Summarize this text. One to three words maximum length. 
        Plain text only. No punctuation, markup or tags.";

        /// Creates a new instance of the Semantic Kernel.

        /// This constructor will validate credentials and create a Semantic Kernel instance.
  
        public SemanticKernelService(string endpoint, string key, string completionDeploymentName, string embeddingDeploymentName, string Speech2TextDeploymentName)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(endpoint);
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNullOrEmpty(completionDeploymentName);
            ArgumentNullException.ThrowIfNullOrEmpty(embeddingDeploymentName);
            ArgumentNullException.ThrowIfNullOrEmpty(Speech2TextDeploymentName);
            

            // Initialize the Semantic Kernel
            var builder = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(completionDeploymentName, endpoint, key)
                .AddAzureOpenAIAudioToText(Speech2TextDeploymentName, endpoint, key)
                .AddAzureOpenAITextEmbeddingGeneration(embeddingDeploymentName, endpoint, key);

            builder.Plugins.AddFromType<TimePlugin>();

            kernel = builder.Build();


            // Initialize the Semantic Kernel
            /*            kernel = Kernel.CreateBuilder()
                            .AddAzureOpenAIChatCompletion(completionDeploymentName, endpoint, key)
                            .AddAzureOpenAITextEmbeddingGeneration(embeddingDeploymentName, endpoint, key)
                            .Build();

                        kernel.Plugins.AddFromObject(new PlaceSuggestionsPlugin("6HAwpsmvqxhvTsZ7bf93dfiWvQD8x6Kl64XgrljmIxAKj56EnQ9lJQQJ99AEACYeBjFQu2YMAAAgAZMPVBDu"), nameof(PlaceSuggestionsPlugin));
                        #pragma warning disable SKEXP0050
                        kernel.Plugins.AddFromType<TimePlugin>();*/


            //.AddfromObject(new ConversationSummaryPlugin())
            //Add the Summarization plugin
            //kernel.Plugins.AddFromType<ConversationSummaryPlugin>();




        }

        /// Generates a completion using a user prompt with chat history to Semantic Kernel and returns the response.
        public async Task<(string completion, int tokens)> GetChatCompletionAsync(string sessionId, List<Message> chatHistory)
        {
            var skChatHistory = new ChatHistory();
            skChatHistory.AddSystemMessage(_systemPrompt);

            foreach (var message in chatHistory)
            {
                skChatHistory.AddUserMessage(message.Prompt);
                if (message.Completion != string.Empty)
                    skChatHistory.AddAssistantMessage(message.Completion);
            }

            //PromptExecutionSettings settings = new()
            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                ExtensionData = new Dictionary<string, object>()
                {   
                    { "Temperature", 0.2 },
                    { "TopP", 0.7 },
                    { "MaxTokens", 1000  }
                }
                //AutoInvokeKernelFunctions = true // Enable AutoInvokeKernelFunctions
            };

            var result = await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(skChatHistory, settings);

            CompletionsUsage completionUsage = (CompletionsUsage)result.Metadata!["Usage"]!;

            string completion = result.Items[0].ToString()!;
            int tokens = completionUsage.CompletionTokens;

            return (completion, tokens);
        }
        /// Generates Image completion using a user prompt with chat history to Semantic Kernel and returns the response.
        public async Task<(string completion, int tokens)> GetImage2TextAsync(string sessionId, List<Message> chatHistory, string b64imgr)
        {

            var skChatHistory = new ChatHistory();
            skChatHistory.AddSystemMessage(_systemPrompt);

            foreach (var message in chatHistory)
            {
                var collectionItems = new ChatMessageContentItemCollection
                {
                    new TextContent(message.Prompt),
                    new ImageContent(new Uri("data:image/jpeg;" + $"base64,{b64imgr}"))

                };
                
                skChatHistory.AddUserMessage(collectionItems);
                
                if (message.Completion != string.Empty)
                    skChatHistory.AddAssistantMessage(message.Completion);
            }


            PromptExecutionSettings settings = new()
            {
                ExtensionData = new Dictionary<string, object>()
                    {
                        { "Temperature", 0.5 },
                        { "TopP", 0.7 },
                        { "MaxTokens", 1000  }
                    }
            };


            var result = await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(skChatHistory, settings);

            CompletionsUsage completionUsage = (CompletionsUsage)result.Metadata!["Usage"]!;

            string completion = result.Items[0].ToString()!;
            int tokens = completionUsage.CompletionTokens;

            return (completion, tokens);

        }

        /// Generates Audio to text completion using a user prompt with chat history to Semantic Kernel and returns the response.
        public async Task<string> GetAudio2TextAsync(Stream audioFileStream)
        {
            OpenAIAudioToTextExecutionSettings executionSettings = new()
            {
                Language = "en", // The language of the audio data as two-letter ISO-639-1 language code (e.g. 'en' or 'es').
                Prompt = "sample prompt", // An optional text to guide the model's style or continue a previous audio segment.
                                          // The prompt should match the audio language.
                ResponseFormat = "json", // The format to return the transcribed text in.
                                         // Supported formats are json, text, srt, verbose_json, or vtt. Default is 'json'.
                Temperature = 0.3f, // The randomness of the generated text.
                                    // Select a value from 0.0 to 1.0. 0 is the default.
            };

            
            var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream!);
            AudioContent audioContent = new(audioFileBinaryData, "mimeType: null");


            var textContent = await kernel.GetRequiredService<IAudioToTextService>().GetTextContentAsync(audioContent, executionSettings);

            return textContent.Text!;
        }


        /// Generates embeddings from the deployed OpenAI embeddings model using Semantic Kernel.

        public async Task<float[]> GetEmbeddingsAsync(string text)
        {

            var embeddings = await kernel.GetRequiredService<ITextEmbeddingGenerationService>().GenerateEmbeddingAsync(text);

            float[] embeddingsArray = embeddings.ToArray();

            return embeddingsArray;
        }

        /// Sends the existing conversation to the Semantic Kernel and returns a two word summary.

        public async Task<string> SummarizeConversationAsync(string conversation)
        {
            //return await summarizePlugin.SummarizeConversationAsync(conversation, kernel);

            var skChatHistory = new ChatHistory();
            skChatHistory.AddSystemMessage(_summarizePrompt);
            skChatHistory.AddUserMessage(conversation);

            PromptExecutionSettings settings = new()
            {
                ExtensionData = new Dictionary<string, object>()
                    {
                        { "Temperature", 0.0 },
                        { "TopP", 1.0 },
                        { "MaxTokens", 100 }
                    }
            };


            var result = await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(skChatHistory, settings);

            string completion = result.Items[0].ToString()!;

            return completion;
        }
    }
}