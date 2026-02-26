namespace YagizEraslan.DeepSeek.Unity
{
    [System.Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;

        public ChatMessage() { }

        public ChatMessage(ChatRole chatRole, string messageContent)
        {
            role = chatRole.ToApiString();
            content = messageContent ?? string.Empty;
        }

        public ChatRole GetRole() => ChatRoleExtensions.FromApiString(role);

        public bool IsUserMessage => GetRole() == ChatRole.User;
        public bool IsAssistantMessage => GetRole() == ChatRole.Assistant;
        public bool IsSystemMessage => GetRole() == ChatRole.System;
    }
}
