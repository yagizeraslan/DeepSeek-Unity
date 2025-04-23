using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekChat : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private ScrollRect chatScroll;
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        [Header("DeepSeek API Settings")]
        [Tooltip("Your DeepSeek API Key")]
        [SerializeField] private string apiKey = "YOUR-DEEPSEEK-API-KEY";
        
        [Tooltip("Enable or disable response streaming")]
        [SerializeField] private bool useStreaming = true;
        
        [Tooltip("Select which DeepSeek model to use")]
        [SerializeField] private DeepSeekModel modelType = DeepSeekModel.DeepSeekV3;
        
        [Tooltip("Temperature for response generation (0.0-1.0)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float temperature = 0.7f;

        [Header("Initial Settings")]
        [Tooltip("System prompt to set assistant behavior")]
        [SerializeField] private string initialPrompt = "Act as a helpful assistant.";

        private float contentHeight;
        private DeepSeekApi deepSeekApi;

        private List<ChatMessage> messages = new List<ChatMessage>();
        
        // Reference to the current response text component
        private TextMeshProUGUI currentResponseText;
        // Reference to the current message item
        private RectTransform currentResponseItem;

        private void Start()
        {
            // Initialize API with the provided key
            deepSeekApi = new DeepSeekApi(apiKey);
            
            sendButton.onClick.AddListener(SendMessage);
            
            // Add system message at initialization
            messages.Add(new ChatMessage
            {
                role = "system",
                content = initialPrompt
            });
        }

        private void AppendMessage(ChatMessage message, bool isUser)
        {
            if (isUser || currentResponseItem == null)
            {
                // For user messages or new assistant responses, create a new item
                chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

                var item = Instantiate(isUser ? sent : received, chatScroll.content);
                item.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = message.content;
                item.anchoredPosition = new Vector2(0, -contentHeight);
                LayoutRebuilder.ForceRebuildLayoutImmediate(item);
                contentHeight += item.sizeDelta.y;
                chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
                chatScroll.verticalNormalizedPosition = 0;

                // If this is a new assistant response, save references for streaming updates
                if (!isUser)
                {
                    currentResponseItem = item;
                    currentResponseText = item.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                }
            }
            else
            {
                // For streaming updates to an existing assistant response
                currentResponseText.text = message.content;
                
                // Recalculate height for the updated content
                LayoutRebuilder.ForceRebuildLayoutImmediate(currentResponseItem);
                
                // Update content height and scroll position
                contentHeight = 0;
                for (int i = 0; i < chatScroll.content.childCount; i++)
                {
                    var child = chatScroll.content.GetChild(i) as RectTransform;
                    contentHeight += child.sizeDelta.y;
                }
                chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
                chatScroll.verticalNormalizedPosition = 0;
            }
        }

        private void HandleStreamingResponse(ChatMessage partialMessage, bool isDone)
        {
            // Update UI with the partial response
            AppendMessage(partialMessage, false);
            
            // If streaming is complete, add the final message to our history
            if (isDone)
            {
                messages.Add(partialMessage);
                currentResponseItem = null;
                currentResponseText = null;
            }
        }

        private async void SendMessage()
        {
            string userText = inputField.text.Trim();
            if (string.IsNullOrEmpty(userText))
            {
                return; // Don't send empty messages
            }
            
            var userMessage = new ChatMessage
            {
                role = "user",
                content = userText
            };
            AppendMessage(userMessage, true);
            messages.Add(userMessage);

            var request = new ChatCompletionRequest
            {
                // Use the selected model
                model = modelType.ToModelString(),
                messages = messages,
                temperature = temperature,
                stream = useStreaming // Use the streaming setting
            };

            // Show loading indicator or disable input while waiting
            inputField.interactable = false;
            sendButton.interactable = false;

            try
            {
                // Clear current response references for the new response
                currentResponseItem = null;
                currentResponseText = null;
                
                if (useStreaming)
                {
                    // Start with an empty placeholder response for streaming
                    var initialResponse = new ChatMessage 
                    { 
                        role = "assistant", 
                        content = "" 
                    };
                    AppendMessage(initialResponse, false);
                    
                    // Use streaming API
                    await deepSeekApi.CreateChatCompletionStreaming(request, HandleStreamingResponse);
                }
                else
                {
                    // Use non-streaming API
                    var response = await deepSeekApi.CreateChatCompletion(request);
                    
                    if (response != null && response.choices != null && response.choices.Count > 0)
                    {
                        var assistantMessage = response.choices[0].message;
                        AppendMessage(assistantMessage, false);
                        messages.Add(assistantMessage);
                    }
                    else
                    {
                        // Handle the case where we got a null or empty response
                        var errorMessage = new ChatMessage
                        {
                            role = "assistant",
                            content = "Sorry, I couldn't get a response. Please try again."
                        };
                        AppendMessage(errorMessage, false);
                        messages.Add(errorMessage);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in SendMessage: {ex.Message}");
                // Option: Show error message to user
                var errorMessage = new ChatMessage
                {
                    role = "assistant",
                    content = "Sorry, I couldn't get a response. Please try again."
                };
                AppendMessage(errorMessage, false);
                messages.Add(errorMessage);
            }
            finally
            {
                // Re-enable input controls
                inputField.interactable = true;
                sendButton.interactable = true;
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
    }
}