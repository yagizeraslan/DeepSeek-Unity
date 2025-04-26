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

        public async UniTask<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
        {
            using var www = new UnityEngine.Networking.UnityWebRequest("https://api.deepseek.com/chat/completions", "POST");
            string body = JsonUtility.ToJson(request);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
            www.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {settings.apiKey}");

            await www.SendWebRequest().ToUniTask();

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