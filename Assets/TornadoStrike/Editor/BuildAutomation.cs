using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace TornadoStrike.Editor
{
    public static class BuildAutomation
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/TornadoStrike/Scenes/Splash.unity",
            "Assets/TornadoStrike/Scenes/MainMenu.unity",
            "Assets/TornadoStrike/Scenes/City_MVP.unity"
        };
        private const string AndroidOutputDirectory = "Builds/Android";
        private const string AndroidBundleId = "com.jnyoung.tornadostrike";
        private const string IosBundleId = "com.jnyoung.tornadostrike";

        [MenuItem("Tornado Strike/Configure Project Settings")]
        public static void ConfigureProjectSettings()
        {
            PlayerSettings.companyName = "JNYoung";
            PlayerSettings.productName = "Tornado Strike";
            PlayerSettings.bundleVersion = Env("TORNADO_STRIKE_VERSION", "0.1.0");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, AndroidBundleId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = int.Parse(Env("TORNADO_STRIKE_ANDROID_VERSION_CODE", "1"));

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, IosBundleId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePaths[0], true),
                new EditorBuildSettingsScene(ScenePaths[1], true),
                new EditorBuildSettingsScene(ScenePaths[2], true)
            };
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tornado Strike/Build/Android Debug AAB")]
        public static void BuildAndroidDebugAab()
        {
            ConfigureAndroidBuild(debug: true, buildAppBundle: true);
            BuildAndroid("TornadoStrike-debug.aab", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tornado Strike/Build/Android Debug APK")]
        public static void BuildAndroidDebugApk()
        {
            ConfigureAndroidBuild(debug: true, buildAppBundle: false);
            BuildAndroid("TornadoStrike-debug.apk", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Tornado Strike/Build/Android Release AAB")]
        public static void BuildAndroidReleaseAab()
        {
            ConfigureAndroidBuild(debug: false, buildAppBundle: true);
            ConfigureAndroidSigningFromEnvironment();
            BuildAndroid("TornadoStrike-release.aab", BuildOptions.None);
        }

        private static void ConfigureAndroidBuild(bool debug, bool buildAppBundle)
        {
            ConfigureProjectSettings();
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.development = debug;
            EditorUserBuildSettings.allowDebugging = debug;
        }

        private static void ConfigureAndroidSigningFromEnvironment()
        {
            var keystoreName = Environment.GetEnvironmentVariable("TORNADO_STRIKE_KEYSTORE");
            var keystorePass = Environment.GetEnvironmentVariable("TORNADO_STRIKE_KEYSTORE_PASS");
            var keyaliasName = Environment.GetEnvironmentVariable("TORNADO_STRIKE_KEYALIAS");
            var keyaliasPass = Environment.GetEnvironmentVariable("TORNADO_STRIKE_KEYALIAS_PASS");

            if (string.IsNullOrEmpty(keystoreName) ||
                string.IsNullOrEmpty(keystorePass) ||
                string.IsNullOrEmpty(keyaliasName) ||
                string.IsNullOrEmpty(keyaliasPass))
            {
                throw new InvalidOperationException("Release AAB requires TORNADO_STRIKE_KEYSTORE, TORNADO_STRIKE_KEYSTORE_PASS, TORNADO_STRIKE_KEYALIAS, and TORNADO_STRIKE_KEYALIAS_PASS.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystoreName;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = keyaliasName;
            PlayerSettings.Android.keyaliasPass = keyaliasPass;
        }

        private static void BuildAndroid(string fileName, BuildOptions options)
        {
            Directory.CreateDirectory(AndroidOutputDirectory);
            var outputPath = Path.Combine(AndroidOutputDirectory, fileName);

            var report = BuildPipeline.BuildPlayer(
                ScenePaths,
                outputPath,
                BuildTarget.Android,
                options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android build failed: {report.summary.result}");
            }

            UnityEngine.Debug.Log($"Android build succeeded: {outputPath} ({report.summary.totalSize} bytes)");
        }

        private static string Env(string key, string fallback)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
