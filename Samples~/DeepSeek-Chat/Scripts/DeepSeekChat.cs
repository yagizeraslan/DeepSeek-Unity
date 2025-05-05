using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        private DeepSeekChatController controller;
        private TMP_Text activeStreamingText;

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

            // Create controller with latest settings
            controller = new DeepSeekChatController(
                new DeepSeekApi(config),
                GetSelectedModelName(),
                AddFullMessageToUI,
                AppendStreamingCharacter,
                useStreaming
            );

            controller.SendUserMessage(inputField.text);
            inputField.text = ""; // Clear input
            inputField.ActivateInputField(); // Focus input again
        }

        private void AddFullMessageToUI(ChatMessage message, bool isUser)
        {
            var prefab = isUser ? sentMessagePrefab : receivedMessagePrefab;
            var instance = Instantiate(prefab, messageContainer);
            var textComponent = instance.GetComponentInChildren<TMP_Text>();

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
    }
}
