using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekStreamingApi
    {
        public void CreateChatCompletionStream(ChatCompletionRequest request, string apiKey, Action<string> onStreamUpdate)
        {
            StartCoroutine(SendStreamingRequestCoroutine(request, apiKey, onStreamUpdate));
        }

        private IEnumerator SendStreamingRequestCoroutine(ChatCompletionRequest request, string apiKey, Action<string> onStreamUpdate)
        {
            request.stream = true;
            string json = JsonUtility.ToJson(request);
            byte[] postData = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest requestStream = new UnityWebRequest("https://api.deepseek.com/chat/completions", "POST")
            {
                uploadHandler = new UploadHandlerRaw(postData),
                downloadHandler = new DownloadHandlerBuffer()
            };

            requestStream.SetRequestHeader("Content-Type", "application/json");
            requestStream.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            var operation = requestStream.SendWebRequest();
            string lastText = "";

            while (!operation.isDone)
            {
                yield return null;

                string currentText = requestStream.downloadHandler.text;
                if (currentText.Length > lastText.Length)
                {
                    string diff = currentText.Substring(lastText.Length);
                    lastText = currentText;

                    string[] lines = diff.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("data: "))
                        {
                            string jsonChunk = line.Substring(6).Trim();
                            if (jsonChunk == "[DONE]") yield break;

                            try
                            {
                                var parsed = JsonUtility.FromJson<StreamingDelta>(jsonChunk);
                                string content = parsed?.choices?[0]?.delta?.content;
                                if (!string.IsNullOrEmpty(content))
                                {
                                    onStreamUpdate?.Invoke(content);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning("DeepSeek: Failed to parse stream chunk: " + ex.Message);
                            }
                        }
                    }
                }
            }

            if (requestStream.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("DeepSeek streaming failed: " + requestStream.error);
            }
        }

        [Serializable]
        private class StreamingDelta
        {
            public Choice[] choices;

            [Serializable]
            public class Choice
            {
                public Delta delta;
            }

            [Serializable]
            public class Delta
            {
                public string content;
            }
        }
    }
}
