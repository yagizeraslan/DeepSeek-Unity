using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek
{
    // Enum for available DeepSeek models
    public enum DeepSeekModel
    {
        DeepSeekR1,
        DeepSeekV3
    }

    // Helper class to convert enum to model string
    public static class DeepSeekModelExtensions
    {
        public static string ToModelString(this DeepSeekModel model)
        {
            return model switch
            {
                DeepSeekModel.DeepSeekR1 => "deepseek-reasoner",
                DeepSeekModel.DeepSeekV3 => "deepseek-chat",
                _ => throw new ArgumentOutOfRangeException(nameof(model), $"Unsupported model: {model}")
            };
        }
    }

    public class DeepSeekApi
    {
        private const string BASE_PATH = "https://api.deepseek.com";
        private Configuration configuration;

        // Define a delegate for streaming responses
        public delegate void OnStreamingResponseDelegate(ChatMessage partialMessage, bool isDone);

        private Configuration Configuration => configuration ?? throw new InvalidOperationException("API Key must be provided.");

        public DeepSeekApi(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API Key is required.", nameof(apiKey));
            }
            configuration = new Configuration(apiKey);
        }

        private byte[] CreatePayload<T>(T request)
        {
            string json = JsonUtility.ToJson(request);
            Debug.Log($"Sending request: {json}");
            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
        {
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);

            using (var webRequest = new UnityWebRequest(path, "POST"))
            {
                var uploadHandler = new UploadHandlerRaw(payload);
                uploadHandler.contentType = "application/json";
                webRequest.uploadHandler = uploadHandler;
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                
                webRequest.SetRequestHeader("Authorization", $"Bearer {Configuration.ApiKey}");
                webRequest.SetRequestHeader("Content-Type", "application/json");

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var responseText = webRequest.downloadHandler.text;
                    Debug.Log($"API Response: {responseText}");
                    
                    return JsonUtility.FromJson<ChatCompletionResponse>(responseText);
                }
                else
                {
                    Debug.LogError($"Request failed: {webRequest.error}");
                    Debug.LogError($"Response code: {webRequest.responseCode}");
                    Debug.LogError($"Response text: {webRequest.downloadHandler?.text ?? "No response"}");
                    return null;
                }
            }
        }

        public async Task CreateChatCompletionStreaming(ChatCompletionRequest request, OnStreamingResponseDelegate onStreamingResponse)
        {
            request.stream = true;
            
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);

            using (var webRequest = new UnityWebRequest(path, "POST"))
            {
                var uploadHandler = new UploadHandlerRaw(payload);
                uploadHandler.contentType = "application/json";
                webRequest.uploadHandler = uploadHandler;
                
                var downloadHandler = new DownloadHandlerBuffer();
                webRequest.downloadHandler = downloadHandler;
                
                webRequest.SetRequestHeader("Authorization", $"Bearer {Configuration.ApiKey}");
                webRequest.SetRequestHeader("Content-Type", "application/json");
                
                var operation = webRequest.SendWebRequest();
                
                var fullContent = "";
                var lastProcessedLength = 0;
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                    
                    // Get current downloaded data
                    string currentData = downloadHandler.text;
                    
                    if (currentData.Length > lastProcessedLength)
                    {
                        // Process only new data
                        string newData = currentData.Substring(lastProcessedLength);
                        lastProcessedLength = currentData.Length;
                        
                        // Split by lines (chunks are separated by newlines)
                        var lines = newData.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("data: "))
                            {
                                string jsonData = line.Substring("data: ".Length);
                                
                                // Handle the special [DONE] marker
                                if (jsonData.Trim() == "[DONE]")
                                {
                                    // We're done streaming
                                    var finalMessage = new ChatMessage
                                    {
                                        role = "assistant",
                                        content = fullContent
                                    };
                                    onStreamingResponse(finalMessage, true);
                                    continue;
                                }
                                
                                try
                                {
                                    var streamResponse = JsonUtility.FromJson<StreamingChatCompletionResponse>(jsonData);
                                    if (streamResponse != null && 
                                        streamResponse.choices != null && 
                                        streamResponse.choices.Count > 0)
                                    {
                                        var delta = streamResponse.choices[0].delta;
                                        if (!string.IsNullOrEmpty(delta.content))
                                        {
                                            fullContent += delta.content;
                                            
                                            // Report the partial message
                                            var partialMessage = new ChatMessage
                                            {
                                                role = "assistant",
                                                content = fullContent
                                            };
                                            onStreamingResponse(partialMessage, false);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"Error parsing streaming response: {ex.Message}");
                                    Debug.LogError($"JSON data: {jsonData}");
                                }
                            }
                        }
                    }
                }
                
                // Check if request was successful
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Streaming request failed: {webRequest.error}");
                    Debug.LogError($"Response code: {webRequest.responseCode}");
                    Debug.LogError($"Response text: {webRequest.downloadHandler?.text ?? "No response"}");
                    
                    // Report the error
                    var errorMessage = new ChatMessage
                    {
                        role = "assistant",
                        content = "Sorry, an error occurred while streaming the response."
                    };
                    onStreamingResponse(errorMessage, true);
                }
            }
        }
    }

    // Define serializable classes for requests and responses
    // Use lowercase property names to match API's camelCase expectations
    
    [Serializable]
    public class ChatCompletionRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature;
        public bool stream;
    }

    [Serializable]
    public class ChatCompletionResponse
    {
        public List<ChatChoice> choices;
    }

    [Serializable]
    public class ChatChoice
    {
        public ChatMessage message;
    }

    // For streaming responses
    [Serializable]
    public class StreamingChatCompletionResponse
    {
        public List<StreamingChatChoice> choices;
    }

    [Serializable]
    public class StreamingChatChoice
    {
        public ChatMessage delta;
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    public class Configuration
    {
        public string ApiKey { get; }

        public Configuration(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}