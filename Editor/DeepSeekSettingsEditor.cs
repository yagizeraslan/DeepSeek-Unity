#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class DeepSeekSettingsEditor : EditorWindow
    {
        private string apiKey;

        [MenuItem("DeepSeek/Settings")]
        public static void ShowWindow()
        {
            GetWindow<DeepSeekSettingsEditor>("DeepSeek Settings");
        }

        private void OnGUI()
        {
            apiKey = EditorPrefs.GetString("DeepSeekAPIKey", "");

            GUILayout.Label("Editor-only Dev API Key", EditorStyles.boldLabel);
            apiKey = EditorGUILayout.TextField("API Key", apiKey);

            if (GUILayout.Button("Save"))
            {
                EditorPrefs.SetString("DeepSeekAPIKey", apiKey);
                Debug.Log("API key saved for editor testing.");
            }
        }
    }
}
#endif
