using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class DeepSeekSetupWizard
{
    static DeepSeekSetupWizard()
    {
        EditorApplication.update += ShowSetupWizard;
    }

    private static void ShowSetupWizard()
    {
        EditorApplication.update -= ShowSetupWizard;
        DeepSeekSetupWindow.ShowWindow();
    }
}

public class DeepSeekSetupWindow : EditorWindow
{
    private static bool isUniTaskInstalled;

    [MenuItem("DeepSeek/Install UniTask")]
    public static void InstallUniTaskManually()
    {
        InstallUniTask();
    }

    [MenuItem("DeepSeek/Setup Wizard")]
    public static void ShowWindow()
    {
        var window = GetWindow<DeepSeekSetupWindow>("DeepSeek Setup Wizard");
        window.Show();
    }

    private void OnEnable()
    {
        isUniTaskInstalled = Directory.Exists(Path.Combine("Packages", "com.cysharp.unitask"));
    }

    private void OnGUI()
    {
        GUILayout.Label("DeepSeek Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (isUniTaskInstalled)
        {
            EditorGUILayout.HelpBox("✅ UniTask is already installed!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("UniTask is required for streaming support.", MessageType.Warning);

            if (GUILayout.Button("Install UniTask"))
            {
                InstallUniTask();
            }
        }
    }

    private static void InstallUniTask()
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        string manifestJson = File.ReadAllText(manifestPath);

        if (!manifestJson.Contains("com.cysharp.unitask"))
        {
            int dependenciesIndex = manifestJson.IndexOf("\"dependencies\": {") + "\"dependencies\": {".Length;
            string insertion = "\n    \"com.cysharp.unitask\": \"https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask\",";
            manifestJson = manifestJson.Insert(dependenciesIndex, insertion);
            File.WriteAllText(manifestPath, manifestJson);
            AssetDatabase.Refresh();
            Debug.Log("[DeepSeek] UniTask installed.");
        }
        else
        {
            Debug.Log("[DeepSeek] UniTask is already listed in manifest.json.");
        }
    }
}
