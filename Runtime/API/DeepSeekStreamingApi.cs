using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekStreamingApi
    {
        public void CreateChatCompletionStream(ChatCompletionRequest request, string apiKey, Action<string> onStreamUpdate)
        {
            request.stream = true;
            string json = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            UnityWebRequest requestStream = new UnityWebRequest("https://api.deepseek.com/chat/completions", "POST");
            requestStream.uploadHandler = new UploadHandlerRaw(bodyRaw);
            requestStream.downloadHandler = new StreamingDownloadHandler(onStreamUpdate);
            requestStream.SetRequestHeader("Content-Type", "application/json");
            requestStream.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            requestStream.SendWebRequest().completed += _ =>
            {
                if (requestStream.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("DeepSeek stream request failed: " + requestStream.error);
                }
            };
        }

        private class StreamingDownloadHandler : DownloadHandlerScript
        {
            private StringBuilder buffer = new();
            private readonly Action<string> onStreamUpdate;

            public StreamingDownloadHandler(Action<string> onStreamUpdate, int bufferSize = 1024)
                : base(new byte[bufferSize])
            {
                this.onStreamUpdate = onStreamUpdate;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || dataLength == 0) return false;

                string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
                buffer.Append(chunk);

                string[] lines = buffer.ToString().Split('\n');
                buffer.Clear();

                foreach (string line in lines)
                {
                    if (line.StartsWith("data: "))
                    {
                        string payload = line.Substring(6).Trim();
                        if (payload == "[DONE]") return true;

                        try
                        {
                            var parsed = JsonUtility.FromJson<StreamingDelta>(payload);
                            string content = parsed?.choices?[0]?.delta?.content;
                            if (!string.IsNullOrEmpty(content))
                            {
                                onStreamUpdate?.Invoke(content);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Failed to parse stream chunk: " + e.Message);
                        }
                    }
                }

                return true;
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
