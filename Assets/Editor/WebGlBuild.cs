using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MonET.Editor
{
    public static class WebGlBuild
    {
        private const string DefaultOutputPath = "Builds/WebGL";
        private const string OutputPathArgName = "-webglBuildPath";
        private const string TemplateFolderPath = "Assets/WebGLTemplates/MonETBrand";
        private const string TemplateSetting = "PROJECT:MonETBrand";

        [MenuItem("Build/WebGL Release")]
        public static void BuildReleaseMenu()
        {
            BuildRelease();
        }

        public static void BuildRelease()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new InvalidOperationException(
                    "Unity WebGL Build Support is not installed for editor version " +
                    Application.unityVersion +
                    ". Install the WebGL module in Unity Hub first.");
            }

            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (enabledScenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes were found in Build Settings.");
            }

            string outputPath = ResolveOutputPath();
            Directory.CreateDirectory(outputPath);

            if (!Directory.Exists(TemplateFolderPath))
            {
                throw new InvalidOperationException(
                    $"WebGL template folder '{TemplateFolderPath}' was not found.");
            }

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.template = TemplateSetting;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build failed with result {report.summary.result}. See Unity Editor logs for details.");
            }

            UnityEngine.Debug.Log(
                $"WebGL build completed successfully at '{Path.GetFullPath(outputPath)}'.");
        }

        private static string ResolveOutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (!string.Equals(args[i], OutputPathArgName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string candidate = args[i + 1]?.Trim();
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim('"');
                }
            }

            return DefaultOutputPath;
        }
    }
}
