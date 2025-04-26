#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity.Editor
{
    [InitializeOnLoad]
    public static class DeepSeekSetupWizard
    {
        private const string UniTaskDependencyName = "com.cysharp.unitask";
        private const string UniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/PackageResources/UniTask";
        private const string SetupCompleteKey = "DeepSeekSetupWizardCompleted";

        static DeepSeekSetupWizard()
        {
            if (EditorPrefs.GetBool(SetupCompleteKey, false))
                return;

            if (!IsUniTaskInstalled())
            {
                if (EditorUtility.DisplayDialog(
                    "DeepSeek Setup Wizard",
                    "DeepSeek API for Unity requires the UniTask package for async operations.\n\nWould you like to install UniTask now?",
                    "Install UniTask",
                    "Cancel"))
                {
                    AddUniTaskToManifest();
                    AddDefineSymbol();
                }
            }

            EditorPrefs.SetBool(SetupCompleteKey, true);
        }

        private static bool IsUniTaskInstalled()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found.");
                return false;
            }

            string manifestContent = File.ReadAllText(manifestPath);
            return manifestContent.Contains(UniTaskDependencyName);
        }

        private static void AddUniTaskToManifest()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found.");
                return;
            }

            string manifestContent = File.ReadAllText(manifestPath);

            int dependenciesIndex = manifestContent.IndexOf("\"dependencies\": {");

            if (dependenciesIndex == -1)
            {
                Debug.LogError("Dependencies section not found in manifest.json.");
                return;
            }

            int insertIndex = manifestContent.IndexOf("{", dependenciesIndex) + 1;

            string uniTaskEntry = $"\n    \"{UniTaskDependencyName}\": \"{UniTaskGitUrl}\",";

            manifestContent = manifestContent.Insert(insertIndex, uniTaskEntry);

            File.WriteAllText(manifestPath, manifestContent);

            Debug.Log("UniTask has been added to manifest.json. Unity will now refresh.");

            AssetDatabase.Refresh();
        }

        private static void AddDefineSymbol()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (!symbols.Contains("DEEPSEEK_HAS_UNITASK"))
            {
                symbols += ";DEEPSEEK_HAS_UNITASK";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
                Debug.Log("Added scripting define: DEEPSEEK_HAS_UNITASK");
            }
        }
    }
}
#endif