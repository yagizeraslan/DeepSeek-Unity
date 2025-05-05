using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace YagizEraslan.DeepSeek.Unity
{
    public static class UnityWebRequestAwaiter
    {
        public static TaskAwaiter<UnityWebRequest> GetAwaiter(this UnityWebRequestAsyncOperation request)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest>();
            request.completed += _ => tcs.SetResult(request.webRequest);
            return tcs.Task.GetAwaiter();
        }
    }
}
