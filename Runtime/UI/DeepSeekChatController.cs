using System;
using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController
    {
        private readonly IDeepSeekApi api;
        private readonly List<ChatMessage> history = new();
        private readonly System.Action<ChatMessage, bool> onMessageUpdate;

        private readonly string selectedModelName;

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> addMessageCallback)
        {
            this.api = api;
            this.selectedModelName = modelName;
            this.addMessageCallback = addMessageCallback;
        }

        public async void SendUserMessage(string input)
        {
            var userMessage = new ChatMessage { role = "user", content = input };
            history.Add(userMessage);
            onMessageUpdate?.Invoke(userMessage, true);

            var request = new ChatCompletionRequest
            {
                model = DeepSeekModel.DeepSeek_V3.ToModelString(),
                messages = history
            };

            var response = await api.CreateChatCompletion(request);
            if (response != null && response.choices.Length > 0)
            {
                var assistantMsg = response.choices[0].message;
                history.Add(assistantMsg);
                onMessageUpdate?.Invoke(assistantMsg, false);
            }
        }
    }
}