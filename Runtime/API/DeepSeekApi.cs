using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekApi : IDeepSeekApi
    {
        private readonly DeepSeekSettings settings;

        public string ApiKey => settings.apiKey;

        public DeepSeekApi(DeepSeekSettings config)
        {
            this.settings = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<Result<ChatCompletionResponse>> CreateChatCompletionAsync(ChatCompletionRequest request)
        {
            if (request == null)
                return Result<ChatCompletionResponse>.Failure("Request cannot be null");

            if (string.IsNullOrEmpty(settings.apiKey))
                return Result<ChatCompletionResponse>.Failure("API key is not configured");

            try
            {
                using var www = new UnityWebRequest(ApiConstants.API_URL, "POST");
                string body = JsonUtility.ToJson(request);

                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader(ApiConstants.HEADER_CONTENT_TYPE, ApiConstants.CONTENT_TYPE_JSON);
                www.SetRequestHeader(ApiConstants.HEADER_AUTHORIZATION, ApiConstants.BEARER_PREFIX + settings.apiKey);

                await www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"{ApiConstants.LOG_PREFIX_API} Request Failed: {www.error}");
                    return Result<ChatCompletionResponse>.Failure(
                        www.error,
                        null,
                        (int)www.responseCode);
                }

                var json = www.downloadHandler.text;
                var response = JsonUtility.FromJson<ChatCompletionResponse>(json);

                if (response == null)
                    return Result<ChatCompletionResponse>.Failure("Failed to parse API response");

                return Result<ChatCompletionResponse>.Success(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ApiConstants.LOG_PREFIX_API} Exception: {ex.Message}");
                return Result<ChatCompletionResponse>.Failure(ex.Message, ex);
            }
        }

        public async Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request)
        {
            var result = await CreateChatCompletionAsync(request);
            return result.GetValueOrDefault(null);
        }
    }
}
