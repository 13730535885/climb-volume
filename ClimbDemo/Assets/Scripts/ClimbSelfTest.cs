using System.Collections;
using UnityEngine;

namespace ClimbDemo
{
    /// <summary>
    /// Headless check for the standalone demo:
    /// Unity.exe -projectPath ...\攀爬\ClimbDemo -thatgjClimbTest -logFile ...
    /// Logs "CLIMB TEST PASS" after climb up -> stand on top -> walk off -> fall.
    /// </summary>
    public class ClimbSelfTest : MonoBehaviour
    {
        private const float GroundY = 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!HasArgument("-thatgjClimbTest")) return;
            GameObject host = new GameObject("ClimbSelfTest");
            DontDestroyOnLoad(host);
            host.AddComponent<ClimbSelfTest>();
        }

        private static bool HasArgument(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == name) return true;
            return false;
        }

        private IEnumerator Start()
        {
            yield return null;

            GameObject ground = new GameObject("TestGround");
            ground.layer = 0;
            ground.transform.position = new Vector3(1000f, GroundY - 0.5f, 0f);
            BoxCollider2D groundBox = ground.AddComponent<BoxCollider2D>();
            groundBox.size = new Vector2(30f, 1f);

            GameObject playerObject = new GameObject("TestPlayer");
            playerObject.layer = 2;
            playerObject.transform.position = new Vector3(1001.5f, GroundY, 0f);

            SpriteRenderer sr = playerObject.AddComponent<SpriteRenderer>();
            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D playerBox = playerObject.AddComponent<BoxCollider2D>();
            playerBox.size = new Vector2(0.62f, 2.62f);
            playerBox.offset = new Vector2(0f, 1.31f);

            ClimbDemoController controller = playerObject.AddComponent<ClimbDemoController>();
            controller.SetTestMode(true, 0f);

            GameObject volumeObject = new GameObject("TestVolume");
            volumeObject.transform.position = new Vector3(1004f, GroundY + 0.7f, 0f);
            BoxCollider2D volumeBox = volumeObject.AddComponent<BoxCollider2D>();
            volumeBox.size = new Vector2(1.6f, 1.4f);
            volumeBox.isTrigger = true;

            ClimbVolume volume = volumeObject.AddComponent<ClimbVolume>();
            volume.blocksWalking = true;
            volume.climbDuration = 0.55f;
            volume.reach = 1.4f;
            volume.landingInset = 0.25f;

            yield return new WaitForSeconds(0.5f);

            controller.Teleport(new Vector3(1002.6f, GroundY, 0f));
            yield return new WaitForSeconds(0.35f);

            bool started = controller.BeginClimb(volume);
            yield return new WaitForSeconds(0.75f);

            float top = controller.transform.position.y;
            bool onTop = top > GroundY + 1.0f;

            controller.SetTestMode(true, 1f);
            yield return new WaitForSeconds(1.1f);
            controller.SetTestMode(true, 0f);
            yield return new WaitForSeconds(0.6f);

            float landed = controller.transform.position.y;
            bool fell = landed < GroundY + 0.5f;

            if (started && onTop && fell)
                Debug.Log("CLIMB TEST PASS top=" + top.ToString("F2") + " landed=" + landed.ToString("F2"));
            else
                Debug.LogError("CLIMB TEST FAIL started=" + started + " onTop=" + onTop +
                               " fell=" + fell + " top=" + top.ToString("F2") +
                               " landed=" + landed.ToString("F2"));

            yield return new WaitForSeconds(0.2f);
            Application.Quit();
        }
    }
}
