using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SheriffAmericanLife.Editor
{
    /// <summary>
    /// Codemagic entry point. Exports the Unity project as an iOS Xcode project.
    /// Signing is intentionally handled separately by the CI workflow.
    /// </summary>
    public static class CodemagicBuild
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        public static void BuildIOS()
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.iOS, BuildTarget.iOS))
            {
                throw new BuildFailedException(
                    "Could not switch the Unity editor to the iOS build target. Verify the iOS Build Support module is installed.");
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) == null)
            {
                ProjectSetup.Setup();
                AssetDatabase.Refresh();
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new BuildFailedException("No enabled scenes are configured in Build Settings.");

            string outputPath = Path.GetFullPath("build/ios");
            Directory.CreateDirectory(outputPath);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    "SHERIFF iOS export failed: " + report.summary.result +
                    " (" + report.summary.totalErrors + " errors).");
            }

            Debug.Log("SHERIFF iOS Xcode project exported to: " + outputPath);
        }
    }
}
