namespace YagizEraslan.DeepSeek.Unity
{
    public enum DeepSeekModel
    {
        DeepSeek-V3,
        DeepSeek-R1
    }

    public static class DeepSeekModelExtensions
    {
        public static string ToModelString(this DeepSeekModel model)
        {
            return model switch
            {
                DeepSeekModel.DeepSeek-V3 => "deepseek-chat",
                DeepSeekModel.DeepSeek-R1 => "deepseek-r1",
                _ => "deepseek-chat"
            };
        }
    }
}
