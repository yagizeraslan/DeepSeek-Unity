using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DeepSeek
{
    public class DeepSeekApi
    {
        private const string BASE_PATH = "https://platform.deepseek.com/v1";
        private Configuration configuration;

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
            var json = JsonConvert.SerializeObject(request, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
        {
            var path = $"{BASE_PATH}/chat/completions";
            var payload = CreatePayload(request);

            using (var webRequest = UnityWebRequest.Put(path, payload))
            {
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Authorization", $"Bearer {Configuration.ApiKey}");
                webRequest.SetRequestHeader("Content-Type", "application/json");

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    return JsonConvert.DeserializeObject<ChatCompletionResponse>(webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"Request failed: {webRequest.error}");
                    return null;
                }
            }
        }
    }

    public class ChatCompletionRequest
    {
        public string Model { get; set; }
        public List<ChatMessage> Messages { get; set; }
        public float Temperature { get; set; }
    }

    public class ChatCompletionResponse
    {
        public List<ChatChoice> Choices { get; set; }
    }

    public class ChatChoice
    {
        public ChatMessage Message { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
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
