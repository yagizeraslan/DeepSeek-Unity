using System;
using System.Threading.Tasks;

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekApi
    {
        UniTask<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
    }
}
