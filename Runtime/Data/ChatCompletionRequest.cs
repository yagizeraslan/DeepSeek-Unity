using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    [System.Serializable]
    public class ChatCompletionRequest
    {
        public string model;
        public ChatMessage[] messages;
        public float temperature = 0.7f;
        public bool stream;
    }
}
