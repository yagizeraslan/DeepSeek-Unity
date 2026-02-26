namespace YagizEraslan.DeepSeek.Unity
{
    [System.Serializable]
    public class ChatCompletionRequest
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature = ApiConstants.DEFAULT_TEMPERATURE;
        public int max_tokens = ApiConstants.DEFAULT_MAX_TOKENS;
        public float top_p = ApiConstants.DEFAULT_TOP_P;
        public bool stream;

        public static ChatCompletionRequestBuilder Builder() => new ChatCompletionRequestBuilder();
    }
}
