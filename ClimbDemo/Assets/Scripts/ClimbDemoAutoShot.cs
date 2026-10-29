using System.IO;
using UnityEngine;

namespace ClimbDemo
{
    /// <summary>Optional capture helper for documentation screenshots.</summary>
    public class ClimbDemoAutoShot : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            string dir = ArgumentValue("-climbShots");
            if (string.IsNullOrEmpty(dir)) return;
            GameObject host = new GameObject("ClimbDemoAutoShot");
            DontDestroyOnLoad(host);
            host.AddComponent<ClimbDemoAutoShot>().directory = dir;
        }

        private static string ArgumentValue(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }

        public string directory;

        private System.Collections.IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);
            Directory.CreateDirectory(directory);
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "01_idle.png"));
            yield return new WaitForSeconds(0.3f);

            ClimbDemoController player = FindFirstObjectByType<ClimbDemoController>();
            ClimbVolume[] volumes = FindObjectsByType<ClimbVolume>(FindObjectsSortMode.None);
            ClimbVolume target = null;
            for (int i = 0; i < volumes.Length; i++)
                if (volumes[i].blocksWalking) target = volumes[i];

            if (player != null && target != null)
            {
                player.Teleport(new Vector3(1.35f, 0f, 0f));
                yield return new WaitForSeconds(0.2f);
                player.BeginClimb(target);
                yield return new WaitForSeconds(0.42f);
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, "02_climb.png"));
                yield return new WaitForSeconds(0.75f);
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, "03_top.png"));

                player.SetTestMode(true, 1f);
                yield return new WaitForSeconds(0.85f);
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, "04_fall.png"));
            }

            yield return new WaitForSeconds(0.3f);
            Application.Quit();
        }
    }
}
