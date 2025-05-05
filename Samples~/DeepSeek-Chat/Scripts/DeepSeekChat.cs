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
        private TMP_Text activeStreamingText; // To update during real streaming

        private void Start()
        {
            var api = new DeepSeekApi(config);
            controller = new DeepSeekChatController(api, GetSelectedModelName(), AddFullMessageToUI, AppendStreamingCharacter, useStreaming);

            sendButton.onClick.AddListener(() =>
            {
                controller.SendUserMessage(inputField.text);
            });
        }

        private string GetSelectedModelName()
        {
            return modelType.ToModelString();
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
                Debug.Log($"[UI] Updated streaming content: {partialContent}");
            }
            else
            {
                Debug.LogWarning("[UI] activeStreamingText is null — cannot update streaming content.");
            }
        }

    }
}