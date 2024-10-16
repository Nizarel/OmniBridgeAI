﻿namespace MultiChat.API.Options;

public record OpenAi
{
    public required string Endpoint { get; init; }

    public required string Key { get; init; }

    public required string CompletionDeploymentName { get; init; }

    public required string EmbeddingDeploymentName { get; init; }
}