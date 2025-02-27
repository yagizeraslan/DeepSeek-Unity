using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DeepSeek
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
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            
            var json = JsonConvert.SerializeObject(request, settings);
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
                    
                    return JsonConvert.DeserializeObject<ChatCompletionResponse>(
                        responseText, 
                        new JsonSerializerSettings { 
                            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() 
                        }
                    );
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
            request.Stream = true;
            
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
                                        Role = "assistant",
                                        Content = fullContent
                                    };
                                    onStreamingResponse(finalMessage, true);
                                    continue;
                                }
                                
                                try
                                {
                                    var streamResponse = JsonConvert.DeserializeObject<StreamingChatCompletionResponse>(jsonData);
                                    if (streamResponse != null && 
                                        streamResponse.Choices != null && 
                                        streamResponse.Choices.Count > 0)
                                    {
                                        var delta = streamResponse.Choices[0].Delta;
                                        if (!string.IsNullOrEmpty(delta.Content))
                                        {
                                            fullContent += delta.Content;
                                            
                                            // Report the partial message
                                            var partialMessage = new ChatMessage
                                            {
                                                Role = "assistant",
                                                Content = fullContent
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
                        Role = "assistant",
                        Content = "Sorry, an error occurred while streaming the response."
                    };
                    onStreamingResponse(errorMessage, true);
                }
            }
        }
    }

    // Define the request and response classes with JsonProperty attributes to ensure
    // proper serialization regardless of C# naming conventions
    
    public class ChatCompletionRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }
        
        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; }
        
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
        
        [JsonProperty("stream")]
        public bool Stream { get; set; }
    }

    public class ChatCompletionResponse
    {
        [JsonProperty("choices")]
        public List<ChatChoice> Choices { get; set; }
    }

    public class ChatChoice
    {
        [JsonProperty("message")]
        public ChatMessage Message { get; set; }
    }

    // For streaming responses
    public class StreamingChatCompletionResponse
    {
        [JsonProperty("choices")]
        public List<StreamingChatChoice> Choices { get; set; }
    }

    public class StreamingChatChoice
    {
        [JsonProperty("delta")]
        public ChatMessage Delta { get; set; }
    }

    public class ChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        
        [JsonProperty("content")]
        public string Content { get; set; }
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