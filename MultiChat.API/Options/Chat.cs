﻿namespace MultiChat.API.Options
{
    public record Chat
    {
        public required string MaxConversationTokens { get; init; }

        public required string CacheSimilarityScore { get; init; }
    }
}
