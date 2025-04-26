using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    [System.Serializable]
    public class ChatCompletionRequest
    {
        public string model;
        public List<ChatMessage> messages;
    }
}
