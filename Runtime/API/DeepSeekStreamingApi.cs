using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekStreamingApi : IDeepSeekStreamingApi
    {
        private UnityWebRequest currentRequest;
        private bool isCancelled;

        public void CreateChatCompletionStream(
            ChatCompletionRequest request,
            string apiKey,
            Action<string> onStreamUpdate,
            Action<string> onError = null,
            Action onComplete = null,
            float timeoutSeconds = 60f)
        {
            if (request == null)
            {
                onError?.Invoke("Request cannot be null");
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API key is not configured");
                return;
            }

            isCancelled = false;
            request.stream = true;
            string json = JsonUtility.ToJson(request);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            var sseParser = new SseParser(onStreamUpdate, onComplete, onError);

            currentRequest = new UnityWebRequest(ApiConstants.API_URL, "POST");
            currentRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            currentRequest.downloadHandler = new StreamingDownloadHandler(sseParser, onError);
            currentRequest.SetRequestHeader(ApiConstants.HEADER_CONTENT_TYPE, ApiConstants.CONTENT_TYPE_JSON);
            currentRequest.SetRequestHeader(ApiConstants.HEADER_AUTHORIZATION, ApiConstants.BEARER_PREFIX + apiKey);
            currentRequest.timeout = (int)timeoutSeconds;

            currentRequest.SendWebRequest().completed += _ =>
            {
                if (isCancelled)
                {
                    Debug.Log($"{ApiConstants.LOG_PREFIX_STREAMING} Stream cancelled by user");
                }
                else if (currentRequest.result == UnityWebRequest.Result.ConnectionError ||
                    currentRequest.result == UnityWebRequest.Result.ProtocolError ||
                    currentRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    string errorMsg = $"Request failed: {currentRequest.error} (Response Code: {currentRequest.responseCode})";
                    Debug.LogError($"{ApiConstants.LOG_PREFIX_STREAMING} {errorMsg}");
                    onError?.Invoke(errorMsg);
                }

                currentRequest?.Dispose();
                currentRequest = null;
            };
        }

        public void CancelStream()
        {
            if (currentRequest != null)
            {
                isCancelled = true;
                currentRequest.Abort();
                Debug.Log($"{ApiConstants.LOG_PREFIX_STREAMING} Stream cancellation requested");
            }
        }

        private class StreamingDownloadHandler : DownloadHandlerScript
        {
            private readonly SseParser sseParser;
            private readonly Action<string> onError;

            public StreamingDownloadHandler(SseParser parser, Action<string> onError = null, int bufferSize = 1024)
                : base(new byte[bufferSize])
            {
                this.sseParser = parser ?? throw new ArgumentNullException(nameof(parser));
                this.onError = onError;
            }

            protected override bool ReceiveData(byte[] data, int dataLength)
            {
                if (data == null || dataLength == 0) return false;

                try
                {
                    string chunk = Encoding.UTF8.GetString(data, 0, dataLength);
                    sseParser.ProcessChunk(chunk);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{ApiConstants.LOG_PREFIX_STREAMING} Error processing streaming data: {e.Message}");
                    onError?.Invoke($"Data processing error: {e.Message}");
                    return false;
                }

                return true;
            }

            protected override void CompleteContent()
            {
                base.CompleteContent();
                sseParser.ProcessRemainingBuffer();
            }

            protected override void ReceiveContentLengthHeader(ulong contentLength)
            {
                base.ReceiveContentLengthHeader(contentLength);
            }

            public override void Dispose()
            {
                sseParser?.Reset();
                base.Dispose();
            }
        }
    }
}
