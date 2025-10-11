using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        BuildAndroidAPK();
    }

    public static void BuildAndroidAPK()
    {
        // Đảm bảo platform được set đúng
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        
        // Tạo thư mục Build nếu chưa có
        string buildPath = Application.dataPath + "/../Build";
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        // Đường dẫn output APK
        string apkPath = buildPath + "/game-o-quan.apk";
        
        // Lấy tất cả scenes trong build settings
        string[] scenes = GetScenesInBuild();
        
        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = apkPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Bắt đầu build APK...");
        Debug.Log("Output path: " + apkPath);
        
        // Thực hiện build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build thành công! APK đã được tạo tại: " + apkPath);
            Debug.Log("Build size: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("Build thất bại! " + report.summary.result);
        }
    }
    
    private static string[] GetScenesInBuild()
    {
        var scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
    
    // Command line build method
    public static void CommandLineBuild()
    {
        Debug.Log("=== Command Line Build Started ===");
        BuildAndroidAPK();
        Debug.Log("=== Command Line Build Finished ===");
    }
}