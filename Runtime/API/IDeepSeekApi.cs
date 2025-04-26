using System;
using System.Threading.Tasks;

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekApi
    {
        Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
        Task CreateChatCompletionStreaming(ChatCompletionRequest request, Action<ChatMessage, bool> onStream);
    }
}
