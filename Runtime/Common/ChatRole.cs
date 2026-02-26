namespace YagizEraslan.DeepSeek.Unity
{
    public enum ChatRole
    {
        User,
        Assistant,
        System
    }

    public static class ChatRoleExtensions
    {
        public static string ToApiString(this ChatRole role)
        {
            return role switch
            {
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                ChatRole.System => "system",
                _ => "user"
            };
        }

        public static ChatRole FromApiString(string roleString)
        {
            return roleString?.ToLowerInvariant() switch
            {
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "system" => ChatRole.System,
                _ => ChatRole.User
            };
        }
    }
}
