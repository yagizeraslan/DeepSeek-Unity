using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekApi : IDeepSeekApi
    {
        private readonly string apiKey;
        private readonly string endpoint = "https://api.deepseek.com/chat/completions";

        public DeepSeekApi(DeepSeekSettings settings)
        {
            apiKey = settings.apiKey;
        }

        public async Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
        {
            string json = JsonUtility.ToJson(request);
            using UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(endpoint, json);
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            await webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {webRequest.error}");
                return null;
            }

            return JsonUtility.FromJson<ChatCompletionResponse>(webRequest.downloadHandler.text);
        }

        public async Task CreateChatCompletionStreaming(ChatCompletionRequest request, Action<ChatMessage, bool> onStream)
        {
            throw new NotImplementedException("Streaming not implemented yet.");
        }
    }
}
