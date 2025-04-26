namespace YagizEraslan.DeepSeek.Unity
{
    [System.Serializable]
    public class ChatCompletionResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public ChatMessage message;
    }
}
