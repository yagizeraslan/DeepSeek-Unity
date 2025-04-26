#if DEEPSEEK_HAS_UNITASK
 using Cysharp.Threading.Tasks;
#endif

using System.Threading.Tasks;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekApi : IDeepSeekApi
    {
        private readonly DeepSeekSettings settings;

        public string ApiKey => settings.apiKey; // ✅ Public Getter

        public DeepSeekApi(DeepSeekSettings config)
        {
            this.settings = config;
        }

#if DEEPSEEK_HAS_UNITASK
        public async UniTask<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
#else
        public async Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
#endif
        {
            using var www = new UnityEngine.Networking.UnityWebRequest("https://api.deepseek.com/chat/completions", "POST");
            string body = JsonUtility.ToJson(request);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {settings.apiKey}");

#if DEEPSEEK_HAS_UNITASK
            await www.SendWebRequest().ToUniTask();
#else
            await www.SendWebRequest();
#endif

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Request Failed: {www.error}");
                return null;
            }

            var json = www.downloadHandler.text;
            return JsonUtility.FromJson<ChatCompletionResponse>(json);
        }
    }
}