using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Omnibridge.App.Plugins.FlightTrackerPlugin;
using Omnibridge.App.Plugins.WeatherPlugin;
using Omnibridge.App.Plugins.PlaceSuggestionsPlugin;
using static System.Environment;



namespace Omnibridge.App
{
    internal class Program
    {

        static async Task Main(string[] args)

        {
            // Create a kernel with the Azure OpenAI chat completion service

            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion("Omni4", "https://omnimx.openai.azure.com/", "1785964a862544409c2941ce5f142bc2");
            /*GetEnvironmentVariable("AOI_ENDPOINT_SWDN")!,
            GetEnvironmentVariable("AOI_KEY_SWDN")!);*/
            #pragma warning disable SKEXP0050
            builder.Plugins.AddFromType<TimePlugin>();

            //builder.Plugins.AddFromObject(new FlightTrackerPlugin(GetEnvironmentVariable("AVIATIONSTACK_KEY")!), nameof(FlightTrackerPlugin));
            builder.Plugins.AddFromObject(new FlightTrackerPlugin("4d5926944f0831f30c1f61f6624274bf"), nameof(FlightTrackerPlugin));

            //builder.Plugins.AddFromObject(new WeatherPlugin(GetEnvironmentVariable("WEATHERAPI_KEY")!), nameof(WeatherPlugin));
            builder.Plugins.AddFromObject(new WeatherPlugin("196802c3c1db4e6cad805554242605"), nameof(WeatherPlugin));

            //builder.Plugins.AddFromObject(new PlaceSuggestionsPlugin(GetEnvironmentVariable("AZUREMAPS_SUBSCRIPTION_KEY")!), nameof(PlaceSuggestionsPlugin));
            builder.Plugins.AddFromObject(new PlaceSuggestionsPlugin("6HAwpsmvqxhvTsZ7bf93dfiWvQD8x6Kl64XgrljmIxAKj56EnQ9lJQQJ99AEACYeBjFQu2YMAAAgAZMPVBDu"), nameof(PlaceSuggestionsPlugin));
            
            // Build the kernel
            var kernel = builder.Build();

            // Create chat history
            ChatHistory history = [];
            //history.AddSystemMessage(@"You're a virtual assistant that helps people find information.");
            history.AddSystemMessage(@"You're a virtual assistant responsible for only flight tracking, weather updates and finding out
the right places within Morocco after inquiring about the proximity or city. You should not talk anything outside of your scope.
Your response should be very concise and to the point. For each correct answer, you will get some $10 from me as a reward.
Be nice with people.");

            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Start the conversation
            while (true)
            {
                // Get user input
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("User > ");
                history.AddUserMessage(Console.ReadLine()!);

                // Enable auto function calling
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    MaxTokens = 700,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // Get the response from the AI
                var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                               history,
                               executionSettings: openAIPromptExecutionSettings,
                               kernel: kernel);


                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nAssistant > ");

                string combinedResponse = string.Empty;
                await foreach (var message in response)
                {
                    //Write the response to the console
                    Console.Write(message);
                    combinedResponse += message;
                }

                Console.WriteLine();

                // Add the message from the agent to the chat history
                history.AddAssistantMessage(combinedResponse);
            }
        }
    }
}