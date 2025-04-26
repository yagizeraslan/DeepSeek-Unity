#if DEEPSEEK_HAS_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekApi
    {
#if DEEPSEEK_HAS_UNITASK
        UniTask<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
#else
        Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
#endif
    }
}