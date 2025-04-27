using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.PackageManager;

[InitializeOnLoad]
public static class DeepSeekSetupWizard
{
    static DeepSeekSetupWizard()
    {
        EditorApplication.update += TryShowSetupWizard;
        EditorApplication.update += PostCompilationStep;
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

    private static void PostCompilationStep()
    {
        if (EditorApplication.isCompiling)
            return;

        string markerPath = Path.Combine(Application.dataPath, "DeepSeek_WaitingForDefine.txt");

        if (File.Exists(markerPath))
        {
            File.Delete(markerPath);
            AssetDatabase.Refresh();

            Debug.Log("[DeepSeek] ✅ Marker file found. Adding define symbol for UniTask...");
            DeepSeekSetupWindow.AddDefineSymbol("DEEPSEEK_HAS_UNITASK");

            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            DeepSeekSetupWindow.TryCloseWindow();
        }

        EditorApplication.update -= PostCompilationStep;
    }

    public static void CheckIfUniTaskImported()
    {
        if (Directory.Exists(Path.Combine("Packages", "com.cysharp.unitask")))
        {
            EditorApplication.update -= CheckIfUniTaskImported;

            Debug.Log("[DeepSeek] 📦 UniTask package detected! Requesting script recompilation...");

            CreateWaitingForDefineMarker();

            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }

    private static void CreateWaitingForDefineMarker()
    {
        string markerPath = Path.Combine(Application.dataPath, "DeepSeek_WaitingForDefine.txt");

        try
        {
            File.WriteAllText(markerPath, "waiting");
            AssetDatabase.ImportAsset("Assets/DeepSeek_WaitingForDefine.txt");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DeepSeek] ❌ Failed to create marker file: {ex.Message}");
        }
    }
}

public class DeepSeekSetupWindow : EditorWindow
{
    private static bool isUniTaskInstalled;
    private static DeepSeekSetupWindow instance;

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
        instance = GetWindow<DeepSeekSetupWindow>("DeepSeek Setup Wizard");
        instance.Show();
    }

    private void OnEnable()
    {
        instance = this;
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
            EditorGUILayout.HelpBox("⚠️ UniTask is required for streaming support.", MessageType.Warning);

            if (GUILayout.Button("Install UniTask"))
            {
                InstallUniTask();
            }
        }
    }

    private static void InstallUniTask()
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

        if (!File.Exists(manifestPath))
        {
            Debug.LogError("[DeepSeek] ❌ manifest.json not found!");
            return;
        }

        string manifestJson = File.ReadAllText(manifestPath);

        if (manifestJson.Contains("com.cysharp.unitask"))
        {
            Debug.Log("[DeepSeek] UniTask is already listed in manifest.json.");
            return;
        }

        try
        {
            int dependenciesIndex = manifestJson.IndexOf("\"dependencies\": {") + "\"dependencies\": {".Length;
            if (dependenciesIndex < "\"dependencies\": {".Length)
            {
                Debug.LogError("[DeepSeek] ❌ Could not find dependencies section in manifest.json.");
                return;
            }

            string insertion = "\n    \"com.cysharp.unitask\": \"https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask\",";
            manifestJson = manifestJson.Insert(dependenciesIndex, insertion);
            File.WriteAllText(manifestPath, manifestJson);

            Debug.Log("[DeepSeek] ✍️ UniTask dependency inserted into manifest.json.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DeepSeek] ❌ Failed to modify manifest.json: {ex.Message}");
            return;
        }

        AssetDatabase.Refresh();
        Client.Resolve();

        EditorApplication.update += DeepSeekSetupWizard.CheckIfUniTaskImported;

        Debug.Log("[DeepSeek] 🔄 Refreshing assets and resolving packages...");
        Debug.Log("[DeepSeek] ⚡ Please wait, UniTask will be installed automatically!");
    }

    public static void AddDefineSymbol(string symbol)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (!defines.Contains(symbol))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";

            defines += symbol;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"[DeepSeek] ➕ Added Scripting Define Symbol: {symbol}");
        }
        else
        {
            Debug.Log($"[DeepSeek] ✅ Scripting Define Symbol '{symbol}' already exists.");
        }
    }

    public static void TryCloseWindow()
    {
        if (instance != null)
        {
            instance.Close();
            Debug.Log("[DeepSeek] 🎉 Setup Wizard closed. UniTask installation completed!");
        }
    }
}