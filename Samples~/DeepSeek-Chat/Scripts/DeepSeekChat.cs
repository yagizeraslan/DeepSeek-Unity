using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChat : MonoBehaviour
    {
        [SerializeField] private DeepSeekSettings config;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private RectTransform sentMessagePrefab, receivedMessagePrefab;
        [SerializeField] private Transform messageContainer;

        private DeepSeekChatController controller;

        void Start()
        {
            controller = new DeepSeekChatController(new DeepSeekApi(config), AddMessageToUI);
            sendButton.onClick.AddListener(() => controller.SendUserMessage(inputField.text));
        }

        private void AddMessageToUI(ChatMessage message, bool isUser)
        {
            RectTransform prefab = isUser ? sentMessagePrefab : receivedMessagePrefab;
            Instantiate(prefab, messageContainer).GetComponentInChildren<TMP_Text>().text = message.content;
        }
    }
}
