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
        private readonly Action<string> onStreamingUpdate;
        private readonly string selectedModelName;
        private readonly bool useStreaming;

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> messageCallback, Action<string> streamingCallback, bool useStreaming)
        {
            this.api = api;
            this.selectedModelName = modelName;
            this.onMessageUpdate = messageCallback;
            this.onStreamingUpdate = streamingCallback;
            this.useStreaming = useStreaming;
        }

        public async void SendUserMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                Debug.LogWarning("User message is empty.");
                return;
            }

            var userChat = new ChatMessage
            {
                role = "user",
                content = userMessage
            };
            history.Add(userChat);
            onMessageUpdate?.Invoke(userChat, true);

            var request = new ChatCompletionRequest
            {
                model = selectedModelName,
                messages = history
            };

            if (useStreaming)
            {
                await SendStreamingResponse(request);
            }
            else
            {
                await SendFullResponse(request);
            }
        }

        private async Task SendFullResponse(ChatCompletionRequest request)
        {
            try
            {
                var response = await api.CreateChatCompletion(request);

                if (response != null && response.choices != null && response.choices.Length > 0)
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

        private async Task SendStreamingResponse(ChatCompletionRequest request)
        {
            try
            {
                var response = await api.CreateChatCompletion(request);

                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    var aiMessage = response.choices[0].message;
                    string streamedContent = "";

                    onMessageUpdate?.Invoke(new ChatMessage
                    {
                        role = "assistant",
                        content = streamedContent
                    }, false);

                    foreach (char c in aiMessage.content)
                    {
                        streamedContent += c;
                        onStreamingUpdate?.Invoke(streamedContent);

                        await Task.Delay(30); // Typing speed per character
                    }

                    history.Add(aiMessage);
                }
                else
                {
                    Debug.LogWarning("No response choices received for streaming from DeepSeek API.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during streaming response from DeepSeek API: {ex.Message}");
            }
        }
    }
}
