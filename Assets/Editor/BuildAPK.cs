using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Automated APK build script
/// </summary>
public class BuildAPK
{
    private const string APK_NAME = "game-o-quan.apk";
    private const string BUILD_PATH = "Build";

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        Debug.Log("=== Starting Android Build ===");

        // Setup build settings
        PlayerSettings.Android.bundleVersionCode++;
        PlayerSettings.bundleVersion = GetVersionString();
        
        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = $"{BUILD_PATH}/{APK_NAME}",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // Build
        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build succeeded: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"📦 APK: {buildOptions.locationPathName}");
            EditorUtility.RevealInFinder(buildOptions.locationPathName);
        }
        else
        {
            Debug.LogError($"❌ Build failed: {report.summary.result}");
        }
    }

    [MenuItem("Build/Build Android APK (Development)")]
    public static void BuildAndroidDev()
    {
        Debug.Log("=== Starting Development Build ===");

        PlayerSettings.Android.bundleVersionCode++;
        PlayerSettings.bundleVersion = GetVersionString();

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = $"{BUILD_PATH}/{APK_NAME}",
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Development build succeeded");
            EditorUtility.RevealInFinder(buildOptions.locationPathName);
        }
    }

    [MenuItem("Build/Configure Android Settings")]
    public static void ConfigureAndroid()
    {
        Debug.Log("=== Configuring Android Settings ===");

        // Company & Product
        PlayerSettings.companyName = "DefaultCompany";
        PlayerSettings.productName = "Ô Quan";
        
        // Package
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.defaultcompany.oquan");
        
        // Version
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        
        // Android settings
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        
        // Graphics
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] {
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2
        });
        
        // Scripting
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        // Optimization
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.Android.useAPKExpansionFiles = false;
        
        Debug.Log("✅ Android settings configured");
        Debug.Log($"Package: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
        Debug.Log($"Version: {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
    }

    private static string[] GetScenes()
    {
        var scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }

    private static string GetVersionString()
    {
        var version = PlayerSettings.bundleVersion.Split('.');
        if (version.Length == 3)
        {
            int patch = int.Parse(version[2]) + 1;
            return $"{version[0]}.{version[1]}.{patch}";
        }
        return PlayerSettings.bundleVersion;
    }
}
