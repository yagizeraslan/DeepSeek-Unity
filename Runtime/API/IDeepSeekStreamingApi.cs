using System;

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekStreamingApi
    {
        void CreateChatCompletionStream(
            ChatCompletionRequest request,
            string apiKey,
            Action<string> onStreamUpdate,
            Action<string> onError = null,
            Action onComplete = null,
            float timeoutSeconds = 60f);

        void CancelStream();
    }
}
