using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekStreamingApi
    {
        public void CreateChatCompletionStream(ChatCompletionRequest request, string apiKey, 
            Action<string> onStreamUpdate, Action<string> onError = null, float timeoutSeconds = 60f)
        {
            request.stream = true;
            string json = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            UnityWebRequest requestStream = new UnityWebRequest("https://api.deepseek.com/chat/completions", "POST");
            requestStream.uploadHandler = new UploadHandlerRaw(bodyRaw);
            requestStream.downloadHandler = new StreamingDownloadHandler(onStreamUpdate, onError);
            requestStream.SetRequestHeader("Content-Type", "application/json");
            requestStream.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            requestStream.timeout = (int)timeoutSeconds;

            requestStream.SendWebRequest().completed += _ =>
            {
                if (requestStream.result == UnityWebRequest.Result.ConnectionError || 
                    requestStream.result == UnityWebRequest.Result.ProtocolError ||
                    requestStream.result == UnityWebRequest.Result.DataProcessingError)
                {
                    string errorMsg = $"Request failed: {requestStream.error} (Response Code: {requestStream.responseCode})";
                    Debug.LogError("DeepSeek stream request failed: " + errorMsg);
                    onError?.Invoke(errorMsg);
                }
                
                // Always dispose the request
                requestStream.Dispose();
            };
        }

        private class StreamingDownloadHandler : DownloadHandlerScript
        {
            private StringBuilder buffer = new();
            private readonly Action<string> onStreamUpdate;
            private readonly Action<string> onError;

            public StreamingDownloadHandler(Action<string> onStreamUpdate, Action<string> onError = null, int bufferSize = 1024)
                : base(new byte[bufferSize])
            {
                this.onStreamUpdate = onStreamUpdate;
                this.onError = onError;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || dataLength == 0) return false;

                try
                {
                    string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
                    buffer.Append(chunk);

                    string bufferContent = buffer.ToString();
                    string[] lines = bufferContent.Split('\n');
                    
                    // Keep the last incomplete line in buffer if buffer doesn't end with newline
                    if (lines.Length > 0 && !bufferContent.EndsWith('\n'))
                    {
                        buffer.Clear();
                        buffer.Append(lines[lines.Length - 1]);
                        // Process all lines except the last incomplete one
                        for (int i = 0; i < lines.Length - 1; i++)
                        {
                            ProcessLine(lines[i]);
                        }
                    }
                    else
                    {
                        buffer.Clear();
                        foreach (string line in lines)
                        {
                            ProcessLine(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing streaming data: {e.Message}");
                    onError?.Invoke($"Data processing error: {e.Message}");
                    return false;
                }

                return true;
            }

            private void ProcessLine(string line)
            {
                if (string.IsNullOrWhiteSpace(line)) return;

                if (line.StartsWith("data: "))
                {
                    string payload = line.Substring(6).Trim();
                    if (payload == "[DONE]") return;

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
                        Debug.LogWarning($"Failed to parse stream chunk: {e.Message}\nPayload: {payload}");
                        // Don't call onError for individual parsing failures as they're common in streaming
                    }
                }
            }

            protected override void CompleteContent()
            {
                base.CompleteContent();
                // Process any remaining content in buffer
                if (buffer.Length > 0)
                {
                    ProcessLine(buffer.ToString());
                }
            }

            protected override void ReceiveContentLengthHeader(ulong contentLength)
            {
                base.ReceiveContentLengthHeader(contentLength);
            }

            public override void Dispose()
            {
                buffer?.Clear();
                base.Dispose();
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