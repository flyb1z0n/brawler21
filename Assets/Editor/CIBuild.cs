using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Custom build method used by GameCI (see build-and-deploy.yml buildMethod).
/// Disables WebGL compression so files work on static hosts (GitHub Pages, itch.io)
/// that don't set Content-Encoding: gzip headers.
/// </summary>
public static class CIBuild
{
    public static void WebGL()
    {
        // Disable compression — GitHub Pages can't serve .gz with the required header
        PlayerSettings.WebGL.compressionFormat    = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        var options = new BuildPlayerOptions
        {
            scenes             = new[] { "Assets/Scenes/GameScene.unity" },
            locationPathName   = "build/WebGL/brawler21",
            target             = BuildTarget.WebGL,
            options            = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[CIBuild] Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log($"[CIBuild] Build succeeded ({report.summary.totalSize / 1024 / 1024} MB)");
        }
    }
}
