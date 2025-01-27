using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace DeepSeek
{
    public class DeepSeekChat : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private ScrollRect chatScroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float contentHeight;
        private DeepSeekApi deepSeekApi = new DeepSeekApi("YOUR_API_KEY");

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string initialPrompt = "Act as a helpful assistant.";

        private void Start()
        {
            sendButton.onClick.AddListener(SendMessage);
        }

        private void AppendMessage(ChatMessage message, bool isUser)
        {
            chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(isUser ? sent : received, chatScroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -contentHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            contentHeight += item.sizeDelta.y;
            chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            chatScroll.verticalNormalizedPosition = 0;
        }

        private async void SendMessage()
        {
            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = inputField.text
            };
            AppendMessage(userMessage, true);
            messages.Add(userMessage);

            var request = new ChatCompletionRequest
            {
                Model = "deepseek-r1",
                Messages = messages,
                Temperature = 0.7f
            };

            var response = await deepSeekApi.CreateChatCompletion(request);
            if (response?.Choices != null && response.Choices.Count > 0)
            {
                var assistantMessage = response.Choices[0].Message;
                messages.Add(assistantMessage);
                AppendMessage(assistantMessage, false);
            }
            else
            {
                Debug.LogWarning("No response from DeepSeek.");
            }

            inputField.text = "";
        }
    }
}
