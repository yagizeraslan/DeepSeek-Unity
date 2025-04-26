using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController
    {
        private readonly DeepSeekStreamingApi streamingApi;
        private readonly DeepSeekApi deepSeekApi; // For normal non-streaming fallback
        private readonly List<ChatMessage> history = new();
        private readonly Action<ChatMessage, bool> onMessageUpdate;
        private readonly Action<string> onStreamingUpdate;
        private readonly string selectedModelName;
        private readonly bool useStreaming;

        private string currentStreamContent = "";

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> messageCallback, Action<string> streamingCallback, bool useStreaming)
        {
            this.deepSeekApi = api as DeepSeekApi; // Casting to real implementation
            this.streamingApi = new DeepSeekStreamingApi(api.GetApiKey()); // Provide API key
            this.selectedModelName = modelName;
            this.onMessageUpdate = messageCallback;
            this.onStreamingUpdate = streamingCallback;
            this.useStreaming = useStreaming;
        }

        public void SendUserMessage(string userMessage)
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
                HandleStreamingResponse(request).Forget(); // Start async, no blocking
            }
            else
            {
                HandleFullResponse(request).Forget();
            }
        }

        private async UniTaskVoid HandleFullResponse(ChatCompletionRequest request)
        {
            try
            {
                var response = await deepSeekApi.CreateChatCompletion(request);

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

        private async UniTaskVoid HandleStreamingResponse(ChatCompletionRequest request)
        {
            currentStreamContent = "";

            try
            {
                await streamingApi.StreamChatCompletionAsync(
                    request,
                    (partialToken) =>
                    {
                        currentStreamContent += partialToken;
                        onStreamingUpdate?.Invoke(currentStreamContent);
                    },
                    () =>
                    {
                        var finalMessage = new ChatMessage
                        {
                            role = "assistant",
                            content = currentStreamContent
                        };
                        history.Add(finalMessage);
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during streaming response from DeepSeek API: {ex.Message}");
            }
        }
    }
}