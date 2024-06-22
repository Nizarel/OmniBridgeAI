using Azure;
using Azure.AI.OpenAI;
using MultiChat.API.Models;

namespace MultiChat.API.Services;

public class OpenAiService
{
    private readonly string _completionDeploymentName = String.Empty;
    private readonly string _embeddingDeploymentName = String.Empty;
    private readonly OpenAIClient _client;


    /// System prompt to send with user prompts to instruct the model for chat session
    private readonly string _systemPrompt = @"
        You are an AI assistant that helps people find information.
        Provide concise answers that are polite and professional." + Environment.NewLine;

   
    /// System prompt to send with user prompts to instruct the model for summarization
    private readonly string _summarizePrompt = @"
        Summarize this prompt in one or two words to use as a label in a button on a web page.
        Do not use any punctuation." + Environment.NewLine;


    /// Creates a new instance of the service.
    /// This constructor will validate credentials and create a HTTP client instance.

    public OpenAiService(string endpoint, string key, string completionDeploymentName, string embeddingDeploymentName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNullOrEmpty(completionDeploymentName);
        ArgumentNullException.ThrowIfNullOrEmpty(embeddingDeploymentName);

        _completionDeploymentName = completionDeploymentName;
        _embeddingDeploymentName = embeddingDeploymentName;

        _client = new(new Uri(endpoint), new AzureKeyCredential(key));
    }


    /// Sends a prompt to the deployed OpenAI LLM model and returns the response.

    public async Task<(string completion, int tokens)> GetChatCompletionAsync(string sessionId, List<Message> conversation)
    {

        //Serialize the conversation to a string to send to OpenAI
        string conversationString = string.Join(Environment.NewLine, conversation.Select(m => m.Prompt + " " + m.Completion));

        ChatCompletionsOptions options = new()
        {
            DeploymentName = _completionDeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(_systemPrompt),
                new ChatRequestUserMessage(conversationString)
            },
            User = sessionId,
            MaxTokens = 1000,
            Temperature = 0.2f,
            NucleusSamplingFactor = 0.7f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        Response<ChatCompletions> completionsResponse = await _client.GetChatCompletionsAsync(options);

        ChatCompletions completions = completionsResponse.Value;

        string completion = completions.Choices[0].Message.Content;
        int tokens = completions.Usage.CompletionTokens;


        return (completion, tokens);
    }

 
    /// Sends the existing conversation to the OpenAI model and returns a two word summary.

    public async Task<string> SummarizeAsync(string sessionId, string conversationText)
    {

        ChatRequestSystemMessage systemMessage = new(_summarizePrompt);
        ChatRequestUserMessage userMessage = new(conversationText);

        ChatCompletionsOptions options = new()
        {
            DeploymentName = _completionDeploymentName,
            Messages = {
                systemMessage,
                userMessage
            },
            User = sessionId,
            MaxTokens = 200,
            Temperature = 0.0f,
            NucleusSamplingFactor = 1.0f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        Response<ChatCompletions> completionsResponse = await _client.GetChatCompletionsAsync(options);

        ChatCompletions completions = completionsResponse.Value;

        string completionText = completions.Choices[0].Message.Content;

        return completionText;
    }


    /// Generates embeddings from the deployed OpenAI embeddings model and returns an array of vectors.

    public async Task<float[]> GetEmbeddingsAsync(string input)
    {

        float[] embedding = new float[0];

        EmbeddingsOptions options = new EmbeddingsOptions(_embeddingDeploymentName, new List<string> { input });

        var response = await _client.GetEmbeddingsAsync(options);

        Embeddings embeddings = response.Value;

        embedding = embeddings.Data[0].Embedding.ToArray();

        return embedding;
    }

    public async Task<(string completion, int tokens)> GetImage2TextAsync(string sessionId, List<Message> conversation, Stream jpegImageStream, string rawImageUri)
    {

        //Serialize the conversation to a string to send to OpenAI
        string conversationString = string.Join(Environment.NewLine, conversation.Select(m => m.Prompt + " " + m.Completion));

        ChatCompletionsOptions options = new()
        {
            DeploymentName = _completionDeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(_systemPrompt),
                new ChatRequestUserMessage(
                        new ChatMessageTextContentItem(conversationString),
                        new ChatMessageImageContentItem(new Uri(rawImageUri)),
                        new ChatMessageImageContentItem(jpegImageStream, "image/jpg", ChatMessageImageDetailLevel.Low)),

                            },
            User = sessionId,
            MaxTokens = 1000,
            Temperature = 0.2f,
            NucleusSamplingFactor = 0.7f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        Response<ChatCompletions> completionsResponse = await _client.GetChatCompletionsAsync(options);

        ChatCompletions completions = completionsResponse.Value;

        string completion = completions.Choices[0].Message.Content;
        int tokens = completions.Usage.CompletionTokens;


        return (completion, tokens);
    }

}
