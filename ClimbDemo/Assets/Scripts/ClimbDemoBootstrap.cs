using System.Collections.Generic;
using UnityEngine;

namespace ClimbDemo
{
    /// <summary>
    /// Builds the whole demo scene at runtime: camera, ground, one blocking crate,
    /// one optional low crate, the character and the HUD. No scene assets required.
    /// </summary>
    public class ClimbDemoBootstrap : MonoBehaviour
    {
        private static bool booted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (booted) return;
            if (HasArgument("-thatgjClimbTest")) return;
            booted = true;

            GameObject host = new GameObject("ClimbDemo");
            host.AddComponent<ClimbDemoBootstrap>();
        }

        private static bool HasArgument(string name)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == name) return true;
            return false;
        }

        private void Start()
        {
            Camera camera = CreateCamera();
            CreateGround();
            CreateBackdrop();
            CreateCrate(3.3f, 1.8f, 1.4f, true, new Color(0.46f, 0.36f, 0.26f));
            CreateCrate(6.7f, 1.2f, 0.9f, false, new Color(0.30f, 0.34f, 0.36f));
            ClimbDemoController player = CreatePlayer();

            CameraFollow follow = camera.gameObject.AddComponent<CameraFollow>();
            follow.target = player.transform;
            follow.offset = new Vector3(0f, 1.7f, -10f);
            follow.smooth = 0.16f;

            gameObject.AddComponent<ClimbDemoHUD>().player = player;
        }

        private Camera CreateCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.06f);
            go.transform.position = new Vector3(2.2f, 1.8f, -10f);
            return camera;
        }

        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            ground.layer = 0;
            ground.transform.position = new Vector3(3f, -0.5f, 0f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(ground.transform, false);
            visual.transform.localScale = new Vector3(20f / 0.32f, 1f / 0.32f, 1f);
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = MakeBoxSprite(new Color(0.10f, 0.13f, 0.17f), new Color(0.22f, 0.27f, 0.32f));
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingOrder = 0;

            BoxCollider2D box = ground.AddComponent<BoxCollider2D>();
            box.size = new Vector2(20f, 1f);
        }

        private void CreateBackdrop()
        {
            Color[] colors =
            {
                new Color(0.07f, 0.09f, 0.12f),
                new Color(0.09f, 0.11f, 0.15f),
                new Color(0.06f, 0.08f, 0.11f)
            };
            float[] xs = { 0.2f, 3.4f, 6.4f };
            float[] widths = { 2.4f, 2.8f, 2.2f };
            float[] heights = { 3.0f, 4.2f, 3.6f };

            for (int i = 0; i < xs.Length; i++)
            {
                GameObject building = new GameObject("Backdrop_" + i);
                building.transform.position = new Vector3(xs[i], heights[i] * 0.5f - 0.4f, 0f);
                SpriteRenderer sr = building.AddComponent<SpriteRenderer>();
                sr.sprite = MakeBoxSprite(colors[i], colors[i] * 1.3f);
                sr.drawMode = SpriteDrawMode.Simple;
                building.transform.localScale = new Vector3(widths[i] / 0.32f, heights[i] / 0.32f, 1f);
                sr.sortingOrder = -2;
            }
        }

        private void CreateCrate(float x, float width, float height, bool blocking, Color color)
        {
            GameObject root = new GameObject(blocking ? "ClimbCrate_Blocking" : "ClimbCrate_Optional");
            root.transform.position = new Vector3(x, height * 0.5f, 0f);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = MakeBoxSprite(color, color * 1.25f);
            sr.drawMode = SpriteDrawMode.Simple;
            visual.transform.localScale = new Vector3(width / 0.32f, height / 0.32f, 1f);
            sr.sortingOrder = 2;

            BoxCollider2D box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(width, height);
            box.isTrigger = true;

            ClimbVolume volume = root.AddComponent<ClimbVolume>();
            volume.useColliderTop = true;
            volume.blocksWalking = blocking;
            volume.climbDuration = 1.05f;
            volume.reach = 1.35f;
            volume.landingInset = Mathf.Min(0.28f, width * 0.22f);
        }

        private ClimbDemoController CreatePlayer()
        {
            GameObject go = new GameObject("Player");
            go.layer = 2; // Ignore Raycast, so the ground check never hits itself
            go.transform.position = new Vector3(0.2f, 0f, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            Sprite[] frames = LoadFrames();
            if (frames.Length > 0) sr.sprite = frames[0];

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.62f, 2.62f);
            box.offset = new Vector2(0f, 1.31f);

            ClimbDemoController controller = go.AddComponent<ClimbDemoController>();
            controller.climbFrames = frames;
            return controller;
        }

        private static Sprite[] LoadFrames()
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>("Climb");
            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogWarning("ClimbDemo: no Resources/Climb frames found.");
                return new Sprite[0];
            }

            List<Sprite> frames = new List<Sprite>(loaded);
            frames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return frames.ToArray();
        }

        private static Sprite MakeBoxSprite(Color fill, Color edge)
        {
            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool border = x < 2 || y < 2 || x >= size - 2 || y >= size - 2;
                    Color c = border ? edge : fill;
                    if (!border)
                    {
                        float grain = 0.94f + 0.06f * Mathf.PerlinNoise(x * 0.35f, y * 0.35f);
                        c *= grain;
                    }
                    c.a = 1f;
                    texture.SetPixel(x, y, c);
                }
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
