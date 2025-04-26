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
        private readonly bool useStreaming;

        public DeepSeekChatController(IDeepSeekApi api, string modelName, Action<ChatMessage, bool> addMessageCallback, bool useStreaming)
        {
            this.api = api;
            this.selectedModelName = modelName;
            this.onMessageUpdate = addMessageCallback;
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

                    // STEP 1: Create one empty UI message first
                    var instance = UnityEngine.Object.Instantiate(receivedMessagePrefab, messageContainer);
                    var textComponent = instance.GetComponentInChildren<TMP_Text>();

                    if (textComponent == null)
                    {
                        Debug.LogError("No TMP_Text found on receivedMessagePrefab!");
                        return;
                    }

                    string streamedContent = "";

                    // STEP 2: Typing simulation
                    foreach (char c in aiMessage.content)
                    {
                        streamedContent += c;
                        textComponent.text = streamedContent;

                        await Task.Delay(30); // Typing speed per character
                    }

                    // STEP 3: Save final full message to history
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
