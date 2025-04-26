using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekStreamingApi
    {
        private readonly string apiKey;
        private readonly string apiUrl = "https://api.deepseek.com/chat/completions";

        public DeepSeekStreamingApi(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async UniTask StreamChatCompletionAsync(ChatCompletionRequest request, Action<string> onPartialMessage, Action onComplete)
        {
            request.stream = true; // Force real streaming enabled

            string jsonBody = JsonUtility.ToJson(request);

            using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                var operation = await www.SendWebRequest().ToUniTask();

#if UNITY_WEBGL
                // WebGL fallback: full response at once
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Streaming request failed: {www.error}");
                    return;
                }

                var fullResponse = www.downloadHandler.text;
                // Here you might parse the whole response (simulate streaming)
                onPartialMessage?.Invoke(fullResponse);
                onComplete?.Invoke();
#else
                // Desktop/Mobile: try to stream properly
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Streaming request failed: {www.error}");
                    return;
                }

                var rawData = www.downloadHandler.data;
                string responseText = Encoding.UTF8.GetString(rawData);

                foreach (var line in responseText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("data: "))
                    {
                        string json = line.Substring(6);

                        if (json.Trim() == "[DONE]")
                        {
                            onComplete?.Invoke();
                            break;
                        }

                        try
                        {
                            var partial = JsonUtility.FromJson<StreamChunk>(json);
                            if (partial.choices != null && partial.choices.Length > 0 && partial.choices[0].delta != null)
                            {
                                string deltaContent = partial.choices[0].delta.content;
                                if (!string.IsNullOrEmpty(deltaContent))
                                {
                                    onPartialMessage?.Invoke(deltaContent);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Failed parsing streamed chunk: {ex.Message}");
                        }
                    }
                }
#endif
            }
        }

        [Serializable]
        private class StreamChunk
        {
            public Choice[] choices;
        }

        [Serializable]
        private class Choice
        {
            public Delta delta;
        }

        [Serializable]
        private class Delta
        {
            public string content;
        }
    }
}
