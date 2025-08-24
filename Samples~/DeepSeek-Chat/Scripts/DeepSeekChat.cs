using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChat : MonoBehaviour
    {
        [Header("DeepSeek Configuration")]
        [SerializeField] private DeepSeekSettings config;
        [SerializeField] private DeepSeekModel modelType = DeepSeekModel.DeepSeek_V3;
        [SerializeField] private bool useStreaming = false;

        [Header("UI Elements")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private RectTransform sentMessagePrefab;
        [SerializeField] private RectTransform receivedMessagePrefab;
        [SerializeField] private Transform messageContainer;

        [Header("Memory Management")]
        [SerializeField] private int maxUIMessages = 100;
        [SerializeField] private int trimToMessages = 70;

        private DeepSeekChatController controller;
        private TMP_Text activeStreamingText;
        private bool controllerInitialized = false;
        private readonly List<GameObject> messageGameObjects = new();

        private void Start()
        {
            sendButton.onClick.AddListener(SendMessage);

            // Allow Enter key to send message
            inputField.onSubmit.AddListener(text =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    SendMessage();
                }
            });
        }

        private string GetSelectedModelName()
        {
            return modelType.ToModelString();
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(inputField.text)) return;

            // Initialize controller only once
            if (!controllerInitialized)
            {
                controller = new DeepSeekChatController(
                    new DeepSeekApi(config),
                    GetSelectedModelName(),
                    AddFullMessageToUI,
                    AppendStreamingCharacter,
                    useStreaming
                );
                controllerInitialized = true;
            }

            controller.SendUserMessage(inputField.text);
            inputField.text = ""; // Clear input
            inputField.ActivateInputField(); // Focus input again
        }

        private void AddFullMessageToUI(ChatMessage message, bool isUser)
        {
            var prefab = isUser ? sentMessagePrefab : receivedMessagePrefab;
            var instance = Instantiate(prefab, messageContainer);
            var textComponent = instance.GetComponentInChildren<TMP_Text>();

            // Add to tracking list
            messageGameObjects.Add(instance.gameObject);
            
            // Trim old UI messages if needed
            TrimUIMessagesIfNeeded();

            if (textComponent != null)
            {
                if (!isUser && useStreaming)
                {
                    textComponent.text = "";
                    activeStreamingText = textComponent;
                }
                else
                {
                    textComponent.text = message.content;
                    activeStreamingText = null;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messageContainer);
        }

        private void AppendStreamingCharacter(string partialContent)
        {
            if (activeStreamingText != null)
            {
                activeStreamingText.text = partialContent;
            }
            else
            {
                Debug.LogWarning("[UI] activeStreamingText is null — cannot update streaming content.");
            }
        }

        private void TrimUIMessagesIfNeeded()
        {
            if (messageGameObjects.Count > maxUIMessages)
            {
                int messagesToRemove = messageGameObjects.Count - trimToMessages;
                
                for (int i = 0; i < messagesToRemove; i++)
                {
                    if (messageGameObjects[i] != null)
                    {
                        DestroyImmediate(messageGameObjects[i]);
                    }
                }
                
                messageGameObjects.RemoveRange(0, messagesToRemove);
                Debug.Log($"UI messages trimmed. Removed {messagesToRemove} old message GameObjects. Current count: {messageGameObjects.Count}");
            }
        }

        public void ClearChat()
        {
            if (controller != null)
            {
                controller.ClearHistory();
            }
            
            // Clear UI messages using tracked GameObjects
            for (int i = 0; i < messageGameObjects.Count; i++)
            {
                if (messageGameObjects[i] != null)
                {
                    DestroyImmediate(messageGameObjects[i]);
                }
            }
            messageGameObjects.Clear();
            
            activeStreamingText = null;
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            sendButton?.onClick.RemoveListener(SendMessage);
            if (inputField != null)
            {
                inputField.onSubmit.RemoveAllListeners();
            }
        }
    }
}
