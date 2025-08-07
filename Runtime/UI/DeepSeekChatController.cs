using System;
using System.Collections.Generic;
using UnityEngine;

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
            this.streamingApi = new DeepSeekStreamingApi();
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
                messages = history.ToArray(),
                stream = useStreaming
            };

            if (useStreaming)
            {
                currentStreamContent = "";

                // Create placeholder AI message in UI BEFORE starting the stream
                var aiMessage = new ChatMessage
                {
                    role = "assistant",
                    content = "" // start with empty
                };
                onMessageUpdate?.Invoke(aiMessage, false); // 👈 this instantiates the UI prefab and assigns activeStreamingText

                streamingApi.CreateChatCompletionStream(
                    request,
                    deepSeekApi.ApiKey,
                    partialToken =>
                    {
                        currentStreamContent += partialToken;
                        onStreamingUpdate?.Invoke(currentStreamContent);
                    },
                    error =>
                    {
                        // Handle streaming errors by showing them to the user
                        Debug.LogError($"DeepSeek streaming error: {error}");
                        string errorMessage = $"❌ Streaming Error: {error}";
                        
                        // If we have current streaming content, append error to it
                        if (!string.IsNullOrEmpty(currentStreamContent))
                        {
                            currentStreamContent += $"\n\n{errorMessage}";
                        }
                        else
                        {
                            currentStreamContent = errorMessage;
                        }
                        
                        onStreamingUpdate?.Invoke(currentStreamContent);
                        
                        // Add the error message to history so it doesn't get lost
                        var errorChatMessage = new ChatMessage
                        {
                            role = "assistant",
                            content = currentStreamContent
                        };
                        // Replace the empty message we added earlier
                        if (history.Count > 0 && history[history.Count - 1].role == "assistant" && 
                            string.IsNullOrEmpty(history[history.Count - 1].content))
                        {
                            history[history.Count - 1] = errorChatMessage;
                        }
                        else
                        {
                            history.Add(errorChatMessage);
                        }
                    });
            }
            else
            {
                HandleFullResponse(request);
            }
        }

        private async void HandleFullResponse(ChatCompletionRequest request)
        {
            try
            {
                var awaitedResponse = await deepSeekApi.CreateChatCompletion(request);

                if (awaitedResponse != null && awaitedResponse.choices != null && awaitedResponse.choices.Length > 0)
                {
                    var aiMessage = awaitedResponse.choices[0].message;
                    history.Add(aiMessage);
                    onMessageUpdate?.Invoke(aiMessage, false);
                }
                else
                {
                    Debug.LogWarning("No response choices received from DeepSeek API.");
                    
                    // Add error message to UI
                    var errorMessage = new ChatMessage
                    {
                        role = "assistant",
                        content = "❌ Error: No response received from DeepSeek API."
                    };
                    history.Add(errorMessage);
                    onMessageUpdate?.Invoke(errorMessage, false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while sending message to DeepSeek API: {ex.Message}");
                
                // Add error message to UI
                var errorMessage = new ChatMessage
                {
                    role = "assistant",
                    content = $"❌ Error: {ex.Message}"
                };
                history.Add(errorMessage);
                onMessageUpdate?.Invoke(errorMessage, false);
            }
        }
    }
}