using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YagizEraslan.DeepSeek.Unity.API;
using YagizEraslan.DeepSeek.Unity.Data;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChatController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private GameObject sentMessagePrefab;
        [SerializeField] private GameObject receivedMessagePrefab;
        [SerializeField] private Transform messageContainer;

        private List<ChatMessage> history = new List<ChatMessage>();
        private IDeepSeekApi api;
        private string selectedModelName;
        private Action<ChatMessage, bool> addMessageCallback;
        private Action<string> onStreamUpdate;
        private bool useStreaming;

        private DeepSeekStreamingApi streamingApi;

        public DeepSeekChatController(
            IDeepSeekApi api,
            string modelName,
            Action<ChatMessage, bool> addMessageCallback,
            Action<string> onStreamUpdate,
            bool useStreaming = false)
        {
            this.api = api;
            this.selectedModelName = modelName;
            this.addMessageCallback = addMessageCallback;
            this.onStreamUpdate = onStreamUpdate;
            this.useStreaming = useStreaming;
        }

        private void Awake()
        {
            streamingApi = gameObject.AddComponent<DeepSeekStreamingApi>();
        }

        public void SendUserMessage(string messageText)
        {
            if (string.IsNullOrEmpty(messageText)) return;

            ChatMessage userMessage = new ChatMessage("user", messageText);
            addMessageCallback?.Invoke(userMessage, true);
            history.Add(userMessage);

            ChatCompletionRequest request = new ChatCompletionRequest
            {
                model = selectedModelName,
                messages = history.ToArray(),
                stream = useStreaming
            };

            if (useStreaming)
            {
                streamingApi.CreateChatCompletionStream(request, api.GetApiKey(), HandleStreamUpdate);
            }
            else
            {
                StartCoroutine(SendNonStreamingRequest(request));
            }
        }

        private IEnumerator<UnityEngine.WaitForSeconds> SendNonStreamingRequest(ChatCompletionRequest request)
        {
            var task = api.CreateChatCompletion(request);
            while (!task.IsCompleted) yield return new WaitForSeconds(0.1f);

            var response = task.Result;
            if (response != null && response.choices != null && response.choices.Length > 0)
            {
                string reply = response.choices[0].message.content;
                ChatMessage assistantMessage = new ChatMessage("assistant", reply);
                addMessageCallback?.Invoke(assistantMessage, false);
                history.Add(assistantMessage);
            }
        }

        private void HandleStreamUpdate(string newToken)
        {
            onStreamUpdate?.Invoke(newToken);
        }
    }
}