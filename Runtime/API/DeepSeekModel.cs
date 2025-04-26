namespace YagizEraslan.DeepSeek.Unity
{
    public enum DeepSeekModel
    {
        DeepSeek_V3,
        DeepSeek_R1
    }

    public static class DeepSeekModelExtensions
    {
        public static string ToModelString(this DeepSeekModel model)
        {
            return model switch
            {
                DeepSeekModel.DeepSeek_V3 => "deepseek-chat",
                DeepSeekModel.DeepSeek_R1 => "deepseek-reasoner",
                _ => "deepseek-chat"
            };
        }
    }
}