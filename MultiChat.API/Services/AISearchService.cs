using Azure.AI.OpenAI;
using Azure;
using MultiChat.API.Models;

namespace MultiChat.API.Services;

public class AISearchService
{
    private readonly string _AISearchEndpoint = String.Empty;
    private readonly string _AISearchIndex = String.Empty;

    //private readonly OpenAIClient _client;



    /// Creates a new instance of the service.
    /// This constructor will validate credentials and create a HTTP client instance.

    public AISearchService(string endpoint, string key, string completionDeploymentName, string embeddingDeploymentName, string Speech2TextDeploymentName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNullOrEmpty(completionDeploymentName);
        ArgumentNullException.ThrowIfNullOrEmpty(embeddingDeploymentName);
        ArgumentNullException.ThrowIfNullOrEmpty(Speech2TextDeploymentName);

        _AISearchEndpoint = completionDeploymentName;
        _AISearchIndex = embeddingDeploymentName;
       

       // _client = new(new Uri(endpoint), new AzureKeyCredential(key));
    }
}


/*using Azure.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;



namespace sksrch_consol
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create a new instance of the SemanticKernel class
            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    deploymentName: Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME"),
                    endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                    apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
                .Build();

            var azureSearchExtensionConfiguration = new AzureSearchChatExtensionConfiguration
            {
                SearchEndpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_ENDPOINT")),
                Authentication = new OnYourDataApiKeyAuthenticationOptions(Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_API_KEY")),
                IndexName = Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_INDEX")
            };

            var chatExtensionsOptions = new AzureChatExtensionsOptions { Extensions = { azureSearchExtensionConfiguration } };
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var executionSettings = new OpenAIPromptExecutionSettings { AzureChatExtensionsOptions = chatExtensionsOptions };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


            var result = await kernel.InvokePromptAsync("What is the price of watermelon?");

            Console.WriteLine(result);


        }
    }
}*/