using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepSeek.Tests
{
    public class DeepSeekApiTests
    {
        private DeepSeekApi deepSeekApi = new DeepSeekApi("YOUR_API_KEY");

        [Test]
        public async Task Create_Chat_Completion()
        {
            var request = new ChatCompletionRequest
            {
                Model = "deepseek-r1",
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "user", Content = "Hello, DeepSeek!" }
                },
                Temperature = 0.7f
            };

            var response = await deepSeekApi.CreateChatCompletion(request);
            Assert.IsNotNull(response);
            Assert.IsNotEmpty(response.Choices);
        }
    }
}
