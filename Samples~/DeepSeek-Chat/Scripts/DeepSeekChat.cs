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
        [SerializeField] private int maxUIMessages = ApiConstants.DEFAULT_MAX_UI_MESSAGES;
        [SerializeField] private int trimToMessages = ApiConstants.DEFAULT_UI_TRIM_COUNT;

        private DeepSeekChatController controller;
        private TMP_Text activeStreamingText;
        private bool controllerInitialized = false;
        private readonly List<GameObject> messageGameObjects = new();

        private void Start()
        {
            sendButton.onClick.AddListener(SendMessage);

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
            inputField.text = "";
            inputField.ActivateInputField();
        }

        private void AddFullMessageToUI(ChatMessage message, bool isUser)
        {
            var prefab = isUser ? sentMessagePrefab : receivedMessagePrefab;
            var instance = Instantiate(prefab, messageContainer);
            var textComponent = instance.GetComponentInChildren<TMP_Text>();

            messageGameObjects.Add(instance.gameObject);

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
                Debug.LogWarning($"{ApiConstants.LOG_PREFIX_CHAT} activeStreamingText is null - cannot update streaming content.");
            }
        }

        private void TrimUIMessagesIfNeeded()
        {
            if (HistoryTrimmer.TrimGameObjectsIfNeeded(messageGameObjects, maxUIMessages, trimToMessages))
            {
                Debug.Log($"{ApiConstants.LOG_PREFIX_CHAT} UI messages trimmed. Current count: {messageGameObjects.Count}");
            }
        }

        public void CancelCurrentStream()
        {
            controller?.CancelCurrentStream();
        }

        public void ClearChat()
        {
            controller?.ClearHistory();

            foreach (var go in messageGameObjects)
            {
                if (go != null)
                    Destroy(go);
            }
            messageGameObjects.Clear();

            activeStreamingText = null;
            Debug.Log($"{ApiConstants.LOG_PREFIX_CHAT} Chat cleared");
        }

        private void OnDestroy()
        {
            sendButton?.onClick.RemoveListener(SendMessage);
            if (inputField != null)
            {
                inputField.onSubmit.RemoveAllListeners();
            }
        }
    }
}
