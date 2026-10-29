using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ClimbDemo.Editor
{
    public static class ClimbDemoBuild
    {
        [MenuItem("Climb Demo/Build Windows")]
        public static void BuildWindows()
        {
            string output = Path.GetFullPath("Build/ClimbDemo.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/ClimbDemo.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("CLIMB DEMO BUILD " + report.summary.result + " -> " + output);
        }
    }
}
