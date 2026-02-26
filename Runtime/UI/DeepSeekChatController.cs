using System;
using System.Collections.Generic;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController
    {
        private readonly IDeepSeekStreamingApi streamingApi;
        private readonly IDeepSeekApi deepSeekApi;
        private readonly List<ChatMessage> history = new();
        private readonly Action<ChatMessage, bool> onMessageUpdate;
        private readonly Action<string> onStreamingUpdate;
        private readonly string selectedModelName;
        private readonly bool useStreaming;

        private string currentStreamContent = "";

        public DeepSeekChatController(
            IDeepSeekApi api,
            IDeepSeekStreamingApi streaming,
            string modelName,
            Action<ChatMessage, bool> messageCallback,
            Action<string> streamingCallback,
            bool useStreaming)
        {
            this.deepSeekApi = api ?? throw new ArgumentNullException(nameof(api));
            this.streamingApi = streaming ?? throw new ArgumentNullException(nameof(streaming));
            this.selectedModelName = modelName;
            this.onMessageUpdate = messageCallback;
            this.onStreamingUpdate = streamingCallback;
            this.useStreaming = useStreaming;
        }

        public DeepSeekChatController(
            IDeepSeekApi api,
            string modelName,
            Action<ChatMessage, bool> messageCallback,
            Action<string> streamingCallback,
            bool useStreaming)
            : this(api, new DeepSeekStreamingApi(), modelName, messageCallback, streamingCallback, useStreaming)
        {
        }

        public void SendUserMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                Debug.LogWarning("User message is empty.");
                return;
            }

            var userChat = ChatMessageFactory.CreateUserMessage(userMessage);
            history.Add(userChat);
            TrimHistoryIfNeeded();
            onMessageUpdate?.Invoke(userChat, true);

            var request = ChatCompletionRequest.Builder()
                .WithModel(selectedModelName)
                .WithMessages(history)
                .WithStreaming(useStreaming)
                .Build();

            if (useStreaming)
            {
                HandleStreamingResponse(request);
            }
            else
            {
                HandleFullResponse(request);
            }
        }

        private void HandleStreamingResponse(ChatCompletionRequest request)
        {
            currentStreamContent = "";

            var aiMessage = ChatMessageFactory.CreateStreamingPlaceholder();
            onMessageUpdate?.Invoke(aiMessage, false);

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
                    Debug.LogError($"DeepSeek streaming error: {error}");

                    if (!string.IsNullOrEmpty(currentStreamContent))
                    {
                        currentStreamContent += $"\n\n❌ Streaming Error: {error}";
                    }
                    else
                    {
                        currentStreamContent = $"❌ Streaming Error: {error}";
                    }

                    onStreamingUpdate?.Invoke(currentStreamContent);

                    var errorChatMessage = ChatMessageFactory.CreateAssistantMessage(currentStreamContent);
                    UpdateOrAddLastAssistantMessage(errorChatMessage);
                },
                () =>
                {
                    var completedMessage = ChatMessageFactory.CreateAssistantMessage(currentStreamContent);
                    UpdateOrAddLastAssistantMessage(completedMessage);
                    Debug.Log($"Streaming completed. Added response to history. History count: {history.Count}");
                });
        }

        private void UpdateOrAddLastAssistantMessage(ChatMessage message)
        {
            if (history.Count > 0 &&
                history[history.Count - 1].IsAssistantMessage &&
                string.IsNullOrEmpty(history[history.Count - 1].content))
            {
                history[history.Count - 1] = message;
            }
            else
            {
                history.Add(message);
            }
        }

        private async void HandleFullResponse(ChatCompletionRequest request)
        {
            var result = await deepSeekApi.CreateChatCompletionAsync(request);

            result.Match(
                onSuccess: response =>
                {
                    if (response.choices != null && response.choices.Length > 0)
                    {
                        var aiMessage = response.choices[0].message;
                        history.Add(aiMessage);
                        onMessageUpdate?.Invoke(aiMessage, false);
                    }
                    else
                    {
                        Debug.LogWarning("No response choices received from DeepSeek API.");
                        var errorMessage = ChatMessageFactory.CreateErrorMessage("No response received from DeepSeek API.");
                        history.Add(errorMessage);
                        onMessageUpdate?.Invoke(errorMessage, false);
                    }
                },
                onFailure: error =>
                {
                    Debug.LogError($"Error while sending message to DeepSeek API: {error}");
                    var errorMessage = ChatMessageFactory.CreateErrorMessage(error);
                    history.Add(errorMessage);
                    onMessageUpdate?.Invoke(errorMessage, false);
                });
        }

        private void TrimHistoryIfNeeded()
        {
            if (HistoryTrimmer.TrimIfNeeded(history, ApiConstants.DEFAULT_MAX_HISTORY_MESSAGES, ApiConstants.DEFAULT_HISTORY_TRIM_COUNT))
            {
                Debug.Log($"Chat history trimmed. Current size: {history.Count}");
            }
        }

        public void CancelCurrentStream()
        {
            streamingApi.CancelStream();
        }

        public void ClearHistory()
        {
            history.Clear();
            Debug.Log("Chat history cleared manually");
        }

        public int GetHistoryCount()
        {
            return history.Count;
        }
    }
}
