using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    [CreateAssetMenu(fileName = "DeepSeekSettings", menuName = "DeepSeek/Settings", order = 1)]
    public class DeepSeekSettings : ScriptableObject
    {
        [Tooltip("Your DeepSeek API Key (used at runtime)")]
        public string apiKey;
    }
}