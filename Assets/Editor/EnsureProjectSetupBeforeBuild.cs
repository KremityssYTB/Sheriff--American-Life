using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SheriffAmericanLife.Editor
{
    /// <summary>
    /// Ensures the generated starter scene, mobile URP asset, and iOS player
    /// settings are valid when a cloud build starts from a clean checkout.
    /// </summary>
    public sealed class EnsureProjectSetupBeforeBuild : IPreprocessBuildWithReport
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string UrpAssetPath = "Assets/Settings/SHERIFF_Mobile_URP.asset";
        private const string BundleIdentifier = "com.kremityss.sheriffamericanlife";

        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                ProjectSetup.Setup();
                AssetDatabase.Refresh();
            }

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
                throw new BuildFailedException("SHERIFF setup failed to generate " + ScenePath + ".");

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (urp == null)
                throw new BuildFailedException(
                    "SHERIFF mobile URP asset is missing. Run SHERIFF > Setup Mobile URP Project and resolve any renderer-generation errors.");

            // Do not silently export a Built-in Render Pipeline build if setup
            // failed to assign the project's intended URP asset.
            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
            if (GraphicsSettings.defaultRenderPipeline != urp)
                throw new BuildFailedException("SHERIFF URP asset could not be assigned as the default render pipeline.");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BundleIdentifier);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            Debug.Log("SHERIFF iOS preflight passed: Main scene, URP asset, iPhone+iPad target, IL2CPP, and Metal are configured.");
        }
    }
}