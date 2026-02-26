using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    public class ChatCompletionRequestBuilder
    {
        private string model;
        private readonly List<ChatMessage> messages = new List<ChatMessage>();
        private float temperature = ApiConstants.DEFAULT_TEMPERATURE;
        private int maxTokens = ApiConstants.DEFAULT_MAX_TOKENS;
        private float topP = ApiConstants.DEFAULT_TOP_P;
        private bool stream;

        public ChatCompletionRequestBuilder WithModel(string modelName)
        {
            model = modelName;
            return this;
        }

        public ChatCompletionRequestBuilder WithModel(DeepSeekModel modelType)
        {
            model = modelType.ToModelString();
            return this;
        }

        public ChatCompletionRequestBuilder WithMessage(ChatMessage message)
        {
            if (message != null)
                messages.Add(message);
            return this;
        }

        public ChatCompletionRequestBuilder WithMessages(IEnumerable<ChatMessage> messageList)
        {
            if (messageList != null)
                messages.AddRange(messageList);
            return this;
        }

        public ChatCompletionRequestBuilder WithUserMessage(string content)
        {
            messages.Add(ChatMessageFactory.CreateUserMessage(content));
            return this;
        }

        public ChatCompletionRequestBuilder WithSystemMessage(string content)
        {
            messages.Add(ChatMessageFactory.CreateSystemMessage(content));
            return this;
        }

        public ChatCompletionRequestBuilder WithAssistantMessage(string content)
        {
            messages.Add(ChatMessageFactory.CreateAssistantMessage(content));
            return this;
        }

        public ChatCompletionRequestBuilder WithTemperature(float temp)
        {
            temperature = temp;
            return this;
        }

        public ChatCompletionRequestBuilder WithMaxTokens(int tokens)
        {
            maxTokens = tokens;
            return this;
        }

        public ChatCompletionRequestBuilder WithTopP(float top)
        {
            topP = top;
            return this;
        }

        public ChatCompletionRequestBuilder WithStreaming(bool enableStreaming = true)
        {
            stream = enableStreaming;
            return this;
        }

        public ChatCompletionRequest Build()
        {
            return new ChatCompletionRequest
            {
                model = model,
                messages = messages.ToArray(),
                temperature = temperature,
                max_tokens = maxTokens,
                top_p = topP,
                stream = stream
            };
        }

        public static ChatCompletionRequestBuilder Create() => new ChatCompletionRequestBuilder();
    }
}
