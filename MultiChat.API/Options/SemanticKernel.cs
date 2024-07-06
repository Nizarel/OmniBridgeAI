namespace MultiChat.API.Options
{
    public record SemanticKernel
    {
        public required string Endpoint { get; init; }

        public required string Key { get; init; }

        public required string CompletionDeploymentName { get; init; }

        public required string EmbeddingDeploymentName { get; init; }
        
        public required string Speech2TextDeploymentName { get; init; }
    }
}
