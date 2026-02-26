namespace YagizEraslan.DeepSeek.Unity
{
    public static class ChatMessageFactory
    {
        public static ChatMessage CreateUserMessage(string content)
            => new ChatMessage(ChatRole.User, content);

        public static ChatMessage CreateAssistantMessage(string content)
            => new ChatMessage(ChatRole.Assistant, content);

        public static ChatMessage CreateSystemMessage(string content)
            => new ChatMessage(ChatRole.System, content);

        public static ChatMessage CreateStreamingPlaceholder()
            => new ChatMessage(ChatRole.Assistant, string.Empty);

        public static ChatMessage CreateErrorMessage(string errorContent)
            => new ChatMessage(ChatRole.Assistant, $"❌ Error: {errorContent}");

        public static ChatMessage CreateStreamingErrorMessage(string errorContent)
            => new ChatMessage(ChatRole.Assistant, $"❌ Streaming Error: {errorContent}");
    }
}
