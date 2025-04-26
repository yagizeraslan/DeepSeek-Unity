using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController
    {
        private readonly IDeepSeekApi api;
        private readonly List<ChatMessage> history = new();
        private readonly Action<ChatMessage, bool> onMessageUpdate;
        private readonly string selectedModelName;

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> addMessageCallback)
        {
            this.api = api;
            this.selectedModelName = modelName;
            this.onMessageUpdate = addMessageCallback;
        }

        public async void SendUserMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                Debug.LogWarning("User message is empty.");
                return;
            }

            // Add user message to history and UI
            var userChat = new ChatMessage
            {
                role = "user",
                content = userMessage
            };
            history.Add(userChat);
            onMessageUpdate?.Invoke(userChat, true);

            // Prepare the request
            var request = new ChatCompletionRequest
            {
                model = selectedModelName,
                messages = history
            };

            try
            {
                // Send request to DeepSeek API
                var response = await api.CreateChatCompletion(request);

                if (response.choices != null && response.choices.Length > 0)
                {
                    var aiMessage = response.choices[0].message;
                    history.Add(aiMessage);
                    onMessageUpdate?.Invoke(aiMessage, false);
                }
                else
                {
                    Debug.LogWarning("No response choices received from DeepSeek API.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while sending message to DeepSeek API: {ex.Message}");
            }
        }
    }
}