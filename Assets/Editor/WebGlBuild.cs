using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MonET.Editor
{
    public static class WebGlBuild
    {
        private const string DefaultOutputPath = "Builds/WebGL";
        private const string OutputPathArgName = "-webglBuildPath";
        private const string FooterBrandText = "SSAFY 14th";
        private const string FooterBrandMarkup = "<div id=\"unity-brand-footer\">SSAFY 14th</div>";
        private const string FooterBrandStyle =
            "#unity-brand-footer { float:left; height: 38px; line-height: 38px; margin-left: 10px; font-family: Arial, sans-serif; font-size: 18px; font-weight: 700; color: #ffffff }";

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

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

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

            ApplyBranding(outputPath);

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

        private static void ApplyBranding(string outputPath)
        {
            string indexPath = Path.Combine(outputPath, "index.html");
            string stylePath = Path.Combine(outputPath, "TemplateData", "style.css");

            if (File.Exists(indexPath))
            {
                string indexHtml = File.ReadAllText(indexPath, Encoding.UTF8);
                indexHtml = indexHtml.Replace(
                    "<div id=\"unity-logo-title-footer\"></div>",
                    FooterBrandMarkup);
                File.WriteAllText(indexPath, indexHtml, new UTF8Encoding(false));
            }

            if (File.Exists(stylePath))
            {
                string styleCss = File.ReadAllText(stylePath, Encoding.UTF8);
                styleCss = styleCss.Replace(
                    "#unity-logo-title-footer { float:left; width: 102px; height: 38px; background: url('unity-logo-title-footer.png') no-repeat center }",
                    FooterBrandStyle);
                File.WriteAllText(stylePath, styleCss, new UTF8Encoding(false));
            }
        }
    }
}
