using System.Threading.Tasks;

namespace YagizEraslan.DeepSeek.Unity
{
    public interface IDeepSeekApi
    {
        string ApiKey { get; }

        Task<Result<ChatCompletionResponse>> CreateChatCompletionAsync(ChatCompletionRequest request);

        Task<ChatCompletionResponse> CreateChatCompletion(ChatCompletionRequest request);
    }
}
