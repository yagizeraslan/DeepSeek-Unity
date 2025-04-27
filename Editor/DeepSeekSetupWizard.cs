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

    private float displayedProgress = 0f;
    private float progressSpeed = 2f;

    [MenuItem("DeepSeek/Setup Wizard")]
    public static void OpenSetupWizard()
    {
        ShowWindow();
    }

    public static void ShowWindow()
    {
        instance = GetWindow<DeepSeekSetupWindow>("DeepSeek Setup Wizard");
        instance.minSize = new Vector2(400, 550);
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

        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

        defineSymbolAdded = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Contains("DEEPSEEK_HAS_UNITASK");
    }

    private void OnGUI()
    {
        GUILayout.Space(20);

        // Big Title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 20;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("DeepSeek Setup Wizard", titleStyle);

        GUILayout.Space(5);

        // Short Description
        GUIStyle descStyle = new GUIStyle(EditorStyles.label);
        descStyle.alignment = TextAnchor.MiddleCenter;
        descStyle.wordWrap = true;
        GUILayout.Label("An unofficial Unity package for seamless integration with the DeepSeek API, enabling advanced reasoning, chat, and task automation in Unity projects. Includes sample scenes, easy API setup, and comprehensive scripts for developers to build smarter interactive experiences.", descStyle);

        GUILayout.Space(20);

        // Progress Bar
        float targetProgress = CalculateProgress();
        displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * progressSpeed);

        Rect progressRect = GUILayoutUtility.GetRect(300, 20);
        EditorGUI.ProgressBar(progressRect, displayedProgress, $"{(int)(displayedProgress * 100)}% Complete");

        GUILayout.Space(20);

        // Steps
        DrawStep("Install UniTask", isUniTaskInstalled);
        GUILayout.Space(5);
        DrawStep("Add DeepSeek Define Symbol", defineSymbolAdded);

        GUILayout.Space(20);

        // Buttons
        EditorGUI.BeginDisabledGroup(isUniTaskInstalled);
        if (GUILayout.Button(isUniTaskInstalled ? "✅ UniTask Installed" : "Install UniTask", GUILayout.Height(40)))
        {
            InstallUniTask();
            RefreshState();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(!isUniTaskInstalled || defineSymbolAdded);
        if (GUILayout.Button(defineSymbolAdded ? "✅ Define Symbol Added" : "Add DeepSeek Define Symbol", GUILayout.Height(40)))
        {
            AddDefineSymbol("DEEPSEEK_HAS_UNITASK");
            RefreshState();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(!isUniTaskInstalled || !defineSymbolAdded);
        if (GUILayout.Button("🎉 DONE!", GUILayout.Height(40)))
        {
            CloseSetupWindow();
        }
        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();
        GUILayout.Space(20);

        GUILayout.Label("Developed by Yağız Eraslan", EditorStyles.centeredGreyMiniLabel);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("⭐ Support Developer"))
        {
            Application.OpenURL("https://github.com/yagizeraslan/DeepSeek-Unity");
        }
        if (GUILayout.Button("📨 Contact"))
        {
            Application.OpenURL("mailto:yagizeraslan@gmail.com");
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        Repaint();
    }

    private void DrawStep(string title, bool completed)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label(completed ? "✅" : "⬜", GUILayout.Width(25));
        GUILayout.Label(title, EditorStyles.label);
        GUILayout.EndHorizontal();
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
    }

    public static void AddDefineSymbol(string symbol)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

        string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);

        if (!defines.Contains(symbol))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";

            defines += symbol;
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
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

    private float CalculateProgress()
    {
        int totalSteps = 2;
        int completedSteps = 0;

        if (isUniTaskInstalled) completedSteps++;
        if (defineSymbolAdded) completedSteps++;

        return (float)completedSteps / totalSteps;
    }
}