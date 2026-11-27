using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;

namespace SheriffAmericanLife.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string SceneFolder = "Assets/Scenes";

        static ProjectSetup()
        {
            EditorApplication.delayCall += EnsureGeneratedProject;
        }

        private static void EnsureGeneratedProject()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneFolder + "/Main.unity") == null)
                Setup();
        }

        [MenuItem("SHERIFF/Setup Mobile URP Project")]
        public static void Setup()
        {
            EnsureFolder("Assets", "Settings");
            EnsureFolder("Assets", "Scenes");
            CreateUrpAssets();
            CreateMainScene();
            ConfigureIOS();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SHERIFF setup complete. Open Assets/Scenes/Main.unity.");
        }

        private static void CreateUrpAssets()
        {
            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                SettingsFolder + "/SHERIFF_Mobile_URP.asset");
            if (existing != null)
            {
                ConfigureUrpAsset(existing);
                GraphicsSettings.defaultRenderPipeline = existing;
                QualitySettings.renderPipeline = existing;
                return;
            }

            var pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            pipeline.name = "SHERIFF_Mobile_URP";
            string pipelinePath = SettingsFolder + "/SHERIFF_Mobile_URP.asset";

            var factory = FindRendererFactory();
            if (factory == null)
            {
                Debug.LogError("Could not find URP renderer factory. Create Assets > Create > Rendering > URP Asset (with Universal Renderer) manually.");
                UnityEngine.Object.DestroyImmediate(pipeline);
                return;
            }

            var parameters = factory.GetParameters();
            object rendererKind = Enum.Parse(parameters[1].ParameterType, "UniversalRenderer");
            var renderer = factory.Invoke(null, new object[] {
                pipelinePath, rendererKind, true, "Renderer"
            }) as ScriptableObject;

            if (renderer == null)
            {
                Debug.LogError("Could not create the URP Universal Renderer asset.");
                UnityEngine.Object.DestroyImmediate(pipeline);
                return;
            }

            var serialized = new SerializedObject(pipeline);
            var rendererList = serialized.FindProperty("m_RendererDataList");
            rendererList.arraySize = 1;
            rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            var defaultIndex = serialized.FindProperty("m_DefaultRendererIndex");
            if (defaultIndex != null) defaultIndex.intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(pipeline, pipelinePath);
            ConfigureUrpAsset(pipeline);
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static void ConfigureUrpAsset(UniversalRenderPipelineAsset pipeline)
        {
            // Balanced high-quality mobile starting point. Device profiling may
            // justify separate URP assets for lower tiers later.
            pipeline.supportsHDR = true;
            pipeline.msaaSampleCount = 2;
            pipeline.renderScale = 1f;
            pipeline.shadowDistance = 45f;
            pipeline.shadowCascadeCount = 2;
            pipeline.mainLightShadowmapResolution = 2048;
            pipeline.useSRPBatcher = true;
            pipeline.supportsDynamicBatching = false;
            pipeline.supportsCameraDepthTexture = false;
            pipeline.supportsCameraOpaqueTexture = false;
            pipeline.maxAdditionalLightsCount = 2;
            pipeline.additionalLightsShadowmapResolution = 256;

            // Main-light shadows stay enabled; additional-light shadows are
            // disabled to avoid multiplying shadow passes on mobile.
            var serialized = new SerializedObject(pipeline);
            SerializedProperty mainShadows = serialized.FindProperty("m_MainLightShadowsSupported");
            if (mainShadows != null) mainShadows.boolValue = true;
            SerializedProperty additionalShadows = serialized.FindProperty("m_AdditionalLightShadowsSupported");
            if (additionalShadows != null) additionalShadows.boolValue = false;
            SerializedProperty softShadows = serialized.FindProperty("m_SoftShadowsSupported");
            if (softShadows != null) softShadows.boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
        }

        private static MethodInfo FindRendererFactory()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types; }

                foreach (var type in types)
                {
                    if (type == null) continue;
                    var method = type.GetMethod("CreateRendererAsset",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null && method.GetParameters().Length == 4) return method;
                }
            }
            return null;
        }

        private static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("WorldBootstrap");
            world.AddComponent<CountyWorldBootstrap>();

            EditorSceneManager.SaveScene(scene, SceneFolder + "/Main.unity");
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(SceneFolder + "/Main.unity", true)
            };
        }

        private static void ConfigureIOS()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,
                "com.kremityss.sheriffamericanlife");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}