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
    private static DeepSeekSetupWindow instance;
    private static bool isUniTaskInstalled;
    private static bool defineSymbolAdded;

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
        RefreshState();
    }

    private static void RefreshState()
    {
        isUniTaskInstalled = Directory.Exists(Path.Combine("Packages", "com.cysharp.unitask"));
        defineSymbolAdded = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains("DEEPSEEK_HAS_UNITASK");
    }

    private void OnGUI()
    {
        GUILayout.Label("DeepSeek Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Install UniTask Button
        EditorGUI.BeginDisabledGroup(isUniTaskInstalled);
        if (GUILayout.Button(isUniTaskInstalled ? "✅ UniTask Installed" : "Install UniTask"))
        {
            InstallUniTask();
            RefreshState();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);

        // Add Define Symbol Button
        EditorGUI.BeginDisabledGroup(!isUniTaskInstalled || defineSymbolAdded);
        if (GUILayout.Button(defineSymbolAdded ? "✅ Define Symbol Added" : "Add DeepSeek Define Symbol"))
        {
            AddDefineSymbol("DEEPSEEK_HAS_UNITASK");
            RefreshState();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);

        // Done Button
        EditorGUI.BeginDisabledGroup(!isUniTaskInstalled || !defineSymbolAdded);
        if (GUILayout.Button("DONE!"))
        {
            Debug.Log("[DeepSeek] 🎉 Setup Complete! Closing Wizard.");
            CloseSetupWindow();
        }
        EditorGUI.EndDisabledGroup();
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

        Debug.Log("[DeepSeek] 🔄 Refreshing assets and resolving packages...");
        Debug.Log("[DeepSeek] ⚡ Please wait for Unity to finish reloading.");
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

    private static void CloseSetupWindow()
    {
        if (instance != null)
        {
            instance.Close();
            instance = null;
        }
    }
}