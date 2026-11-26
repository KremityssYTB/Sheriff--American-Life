using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SheriffAmericanLife.Editor
{
    /// <summary>
    /// Ensures the generated starter scene and iOS player settings exist
    /// when a cloud build starts from a clean checkout.
    /// </summary>
    public sealed class EnsureProjectSetupBeforeBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            const string scenePath = "Assets/Scenes/Main.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                ProjectSetup.Setup();
                AssetDatabase.Refresh();
            }

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (scene == null)
                throw new BuildFailedException("SHERIFF setup failed to generate Assets/Scenes/Main.unity.");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.iOS,
                "com.kremityss.sheriffamericanlife");
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.iOS,
                ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneOnly;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        }
    }
}