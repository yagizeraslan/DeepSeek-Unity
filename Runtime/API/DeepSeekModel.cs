namespace YagizEraslan.DeepSeek.Unity
{
    public enum DeepSeekModel
    {
        DeepSeekChat,
        DeepSeekR1
    }

    public static class DeepSeekModelExtensions
    {
        public static string ToModelString(this DeepSeekModel model)
        {
            return model switch
            {
                DeepSeekModel.DeepSeekChat => "deepseek-chat",
                DeepSeekModel.DeepSeekR1 => "deepseek-r1",
                _ => "deepseek-chat"
            };
        }
    }
}
