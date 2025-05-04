using System.Threading.Tasks;

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekApi
    {
        Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
    }
}