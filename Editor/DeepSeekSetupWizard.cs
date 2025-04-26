using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class DeepSeekSetupWizard
{
    static DeepSeekSetupWizard()
    {
        EditorApplication.update += TryShowSetupWizard;
    }

    private static void TryShowSetupWizard()
    {
        EditorApplication.update -= TryShowSetupWizard;

        if (!IsUniTaskInstalled())
        {
            DeepSeekSetupWindow.ShowWindow();
        }
    }

    private static bool IsUniTaskInstalled()
    {
        return Directory.Exists(Path.Combine("Packages", "com.cysharp.unitask"));
    }
}

public class DeepSeekSetupWindow : EditorWindow
{
    private static bool isUniTaskInstalled;

    [MenuItem("DeepSeek/Install UniTask Manually")]
    public static void InstallUniTaskManually()
    {
        InstallUniTask();
    }

    [MenuItem("DeepSeek/Setup Wizard")]
    public static void OpenSetupWizard()
    {
        ShowWindow();
    }

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
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

            Debug.Log("[DeepSeek] UniTask installed and recompile requested.");

            AddDefineSymbol("DEEPSEEK_HAS_UNITASK");
        }
        else
        {
            Debug.Log("[DeepSeek] UniTask already listed.");
        }
    }

    private static void AddDefineSymbol(string symbol)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (!defines.Contains(symbol))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";

            defines += symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"[DeepSeek] Added Scripting Define Symbol: {symbol}");
        }
        else
        {
            Debug.Log($"[DeepSeek] Scripting Define Symbol '{symbol}' already exists.");
        }
    }
}