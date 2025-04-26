using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

namespace YagizEraslan.DeepSeek.Unity.Editor
{
    public class DeepSeekSetupWizard : EditorWindow
    {
        private const string UniTaskDefine = "DEEPSEEK_HAS_UNITASK";
        private const string UniTaskPackageName = "com.cysharp.unitask";
        private const string UniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/PackageResources/UniTask";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!IsUniTaskInstalled())
            {
                EditorApplication.update += ShowWindowOnLoad;
            }
            else
            {
                AddDefineIfNeeded();
            }
        }

        private static void ShowWindowOnLoad()
        {
            EditorApplication.update -= ShowWindowOnLoad;
            ShowWindow();
        }

        [MenuItem("DeepSeek/Setup Wizard", false, 1)]
        public static void ShowWindow()
        {
            DeepSeekSetupWizard window = GetWindow<DeepSeekSetupWizard>(true, "DeepSeek Setup Wizard");
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 400, 150);
        }

        private void OnGUI()
        {
            GUILayout.Label("DeepSeek API for Unity", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("UniTask is required for streaming functionality.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("Would you like to install UniTask automatically?", EditorStyles.wordWrappedLabel);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Install UniTask"))
            {
                InstallUniTask();
                Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            GUILayout.EndHorizontal();
        }

        private static bool IsUniTaskInstalled()
        {
            return Directory.Exists(Path.Combine("Packages", UniTaskPackageName))
                   || Directory.GetDirectories("Packages").Any(path => path.Contains("UniTask"));
        }

        private static void InstallUniTask()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestText = File.ReadAllText(manifestPath);

            if (!manifestText.Contains(UniTaskPackageName))
            {
                int index = manifestText.IndexOf("dependencies": {");
                if (index != -1)
                {
                    int insertPos = manifestText.IndexOf('{', index) + 1;
                    string toInsert = $"\n    \"{UniTaskPackageName}\": \"{UniTaskGitUrl}\",";
                    manifestText = manifestText.Insert(insertPos, toInsert);

                    File.WriteAllText(manifestPath, manifestText);
                    AssetDatabase.Refresh();

                    Debug.Log("[DeepSeek] UniTask installed successfully.");
                }
                else
                {
                    Debug.LogError("[DeepSeek] Could not find dependencies block in manifest.json.");
                }
            }

            AddDefineIfNeeded();
        }

        private static void AddDefineIfNeeded()
        {
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!currentDefines.Contains(UniTaskDefine))
            {
                currentDefines = string.IsNullOrEmpty(currentDefines) ? UniTaskDefine : currentDefines + ";" + UniTaskDefine;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefines);
            }
        }
    }
}

#endif