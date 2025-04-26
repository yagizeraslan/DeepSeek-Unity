using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChat : MonoBehaviour
    {
        [Header("DeepSeek Configuration")]
        [SerializeField] private DeepSeekSettings deepSeekAPISettings;

        [SerializeField] private DeepSeekModel modelType = DeepSeekModel.DeepSeek_V3;

        [Header("UI Elements")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private RectTransform sentMessagePrefab;
        [SerializeField] private RectTransform receivedMessagePrefab;
        [SerializeField] private Transform messageContainer;

        private DeepSeekChatController controller;

        private void Start()
        {
            var api = new DeepSeekApi(deepSeekAPISettings);
            controller = new DeepSeekChatController(api, GetSelectedModelName(), AddMessageToUI);


            sendButton.onClick.AddListener(() =>
            {
                controller.SendUserMessage(inputField.text);
            });
        }

        private string GetSelectedModelName()
        {
            switch (modelType)
            {
                case DeepSeekModel.DeepSeek_V3:
                    return "deepseek-chat";
                case DeepSeekModel.DeepSeek_R1:
                    return "deepseek-reasoner";
                default:
                    return "deepseek-chat"; // fallback
            }
        }

        private void AddMessageToUI(ChatMessage message, bool isUser)
        {
            var prefab = isUser ? sentMessagePrefab : receivedMessagePrefab;
            var instance = Instantiate(prefab, messageContainer);
            var textComponent = instance.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message.content;
            }
        }
    }
}