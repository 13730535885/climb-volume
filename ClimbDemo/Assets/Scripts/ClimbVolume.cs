using System.Collections.Generic;
using UnityEngine;

namespace ClimbDemo
{
    /// <summary>
    /// Standalone climbable volume. Attach to an empty GameObject with one BoxCollider2D.
    /// The top edge is the standable surface; the player climbs from either side with W / Up.
    /// No dependency on the Small Voice project.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public class ClimbVolume : MonoBehaviour
    {
        [Header("Volume")]
        public bool useColliderTop = true;
        public float topY;

        [Header("Rules")]
        public bool blocksWalking = true;
        public float climbDuration = 1.05f;
        public float reach = 1.35f;
        public float landingInset = 0.28f;

        public static readonly List<ClimbVolume> All = new List<ClimbVolume>();

        private BoxCollider2D volume;
        private BoxCollider2D body;
        private BoxCollider2D platform;
        private ClimbDemoController player;

        public Bounds Bounds
        {
            get
            {
                if (volume == null) volume = GetComponent<BoxCollider2D>();
                return volume != null ? volume.bounds : new Bounds(transform.position, Vector3.zero);
            }
        }

        public float TopY => useColliderTop ? Bounds.max.y : topY;
        public float CenterX => Bounds.center.x;

        private void Reset()
        {
            volume = GetComponent<BoxCollider2D>();
            if (volume != null)
            {
                volume.size = new Vector2(2f, 2f);
                volume.isTrigger = true;
            }
            topY = transform.position.y + 2f;
        }

        private void Awake()
        {
            volume = GetComponent<BoxCollider2D>();
            if (volume != null) volume.isTrigger = true;
            BuildPhysics();
        }

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Update()
        {
            if (blocksWalking || platform == null) return;
            if (player == null) player = FindFirstObjectByType<ClimbDemoController>();
            bool above = player != null && player.transform.position.y >= TopY - 0.08f;
            platform.enabled = above;
        }

        private void OnValidate()
        {
            if (volume == null) volume = GetComponent<BoxCollider2D>();
            if (volume != null) volume.isTrigger = true;
            climbDuration = Mathf.Max(0.2f, climbDuration);
            reach = Mathf.Max(0.2f, reach);
            landingInset = Mathf.Max(0.02f, landingInset);
        }

        private void BuildPhysics()
        {
            if (volume == null) return;
            if (body != null) Destroy(body);
            if (platform != null) Destroy(platform);

            if (blocksWalking)
            {
                GameObject go = new GameObject("Body");
                go.transform.SetParent(transform, false);
                body = go.AddComponent<BoxCollider2D>();
                body.size = volume.size;
                body.offset = volume.offset;
                body.isTrigger = false;
            }
            else
            {
                GameObject go = new GameObject("TopPlatform");
                go.transform.SetParent(transform, false);
                platform = go.AddComponent<BoxCollider2D>();
                platform.size = new Vector2(volume.size.x, Mathf.Min(0.28f, volume.size.y * 0.5f));
                platform.offset = new Vector2(volume.offset.x,
                    volume.offset.y + volume.size.y * 0.5f - platform.size.y * 0.5f);
                platform.usedByEffector = true;

                PlatformEffector2D effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.useSideFriction = false;
                effector.useSideBounce = false;
                effector.surfaceArc = 180f;
                platform.enabled = false;
            }
        }

        public bool CanClimbFrom(Vector3 feetPosition)
        {
            if (volume == null) return false;
            if (feetPosition.y > TopY - 0.12f) return false;

            Bounds b = Bounds;
            if (feetPosition.y < b.min.y - 0.75f) return false;

            float dx = 0f;
            if (feetPosition.x < b.min.x) dx = b.min.x - feetPosition.x;
            else if (feetPosition.x > b.max.x) dx = feetPosition.x - b.max.x;
            return dx <= reach;
        }

        public float GetSideX(float fromX)
        {
            Bounds b = Bounds;
            return fromX < b.center.x ? b.min.x - 0.06f : b.max.x + 0.06f;
        }

        public float GetLandingX(float fromX)
        {
            Bounds b = Bounds;
            float inset = Mathf.Min(landingInset, b.size.x * 0.45f);
            return fromX < b.center.x ? b.min.x + inset : b.max.x - inset;
        }

        public static ClimbVolume FindClimbable(Vector3 feetPosition)
        {
            ClimbVolume best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < All.Count; i++)
            {
                ClimbVolume candidate = All[i];
                if (candidate == null || !candidate.isActiveAndEnabled) continue;
                if (!candidate.CanClimbFrom(feetPosition)) continue;

                Bounds b = candidate.Bounds;
                float dx = 0f;
                if (feetPosition.x < b.min.x) dx = b.min.x - feetPosition.x;
                else if (feetPosition.x > b.max.x) dx = feetPosition.x - b.max.x;

                if (dx < bestDistance)
                {
                    bestDistance = dx;
                    best = candidate;
                }
            }
            return best;
        }

        private void OnDrawGizmos()
        {
            if (volume == null) volume = GetComponent<BoxCollider2D>();
            if (volume == null) return;

            Bounds b = volume.bounds;
            Gizmos.color = new Color(0.25f, 1f, 0.45f, 0.85f);
            Gizmos.DrawWireCube(b.center, b.size);

            Gizmos.color = new Color(1f, 0.85f, 0.25f, 1f);
            Gizmos.DrawLine(new Vector3(b.min.x, TopY, 0f), new Vector3(b.max.x, TopY, 0f));

#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1f, 0.85f, 0.25f, 0.9f);
            UnityEditor.Handles.Label(new Vector3(b.center.x, TopY + 0.15f, 0f),
                "top " + TopY.ToString("F2"));
#endif
        }
    }
}
