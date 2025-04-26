using System;
using System.Collections.Generic;
using UnityEngine;

#if DEEPSEEK_HAS_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController
    {
        private readonly DeepSeekStreamingApi streamingApi;
        private readonly DeepSeekApi deepSeekApi;
        private readonly List<ChatMessage> history = new();
        private readonly Action<ChatMessage, bool> onMessageUpdate;
        private readonly Action<string> onStreamingUpdate;
        private readonly string selectedModelName;
        private readonly bool useStreaming;

        private string currentStreamContent = "";

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> messageCallback, Action<string> streamingCallback, bool useStreaming)
        {
            var concreteApi = api as DeepSeekApi;
            if (concreteApi == null)
            {
                Debug.LogError("DeepSeekChatController requires DeepSeekApi instance, not just IDeepSeekApi interface!");
            }
            this.deepSeekApi = concreteApi;
            this.streamingApi = new DeepSeekStreamingApi(concreteApi.ApiKey);
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

#if DEEPSEEK_HAS_UNITASK
            if (useStreaming)
            {
                HandleStreamingResponse(request).Forget();
            }
            else
            {
                HandleFullResponse(request).Forget();
            }
#else
            HandleFullResponse(request);
#endif
        }

#if DEEPSEEK_HAS_UNITASK
        private async UniTaskVoid HandleFullResponse(ChatCompletionRequest request)
#else
        private void HandleFullResponse(ChatCompletionRequest request)
#endif
        {
            try
            {
#if DEEPSEEK_HAS_UNITASK
                var awaitedResponse = await deepSeekApi.CreateChatCompletion(request);
#else
                var awaitedResponse = deepSeekApi.CreateChatCompletion(request).Result;
#endif

                if (awaitedResponse != null && awaitedResponse.choices != null && awaitedResponse.choices.Length > 0)
                {
                    var aiMessage = awaitedResponse.choices[0].message;
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

#if DEEPSEEK_HAS_UNITASK
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
#endif
    }
}