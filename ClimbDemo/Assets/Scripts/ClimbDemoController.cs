using UnityEngine;

namespace ClimbDemo
{
    /// <summary>
    /// Small standalone character controller used only by the climb demo.
    /// A/D move, W / Up climbs, walking off the top edge falls back to the ground.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClimbDemoController : MonoBehaviour
    {
        public float moveSpeed = 2.2f;
        public float acceleration = 30f;
        public float deceleration = 34f;
        public float gravity = 40f;
        public float maxFallSpeed = -18f;
        public Sprite[] climbFrames;

        public bool IsClimbing { get; private set; }
        public bool IsGrounded { get; private set; }

        private Rigidbody2D body;
        private BoxCollider2D box;
        private SpriteRenderer sprite;
        private float horizontal;
        private float verticalVelocity;
        private float climbElapsed;
        private float climbDuration;
        private bool testMode;
        private float testHorizontal;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            sprite = GetComponent<SpriteRenderer>();

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        private void Start()
        {
            if (climbFrames != null && climbFrames.Length > 0 && sprite != null)
                sprite.sprite = climbFrames[0];
        }

        private void Update()
        {
            if (IsClimbing)
            {
                AnimateClimb();
                return;
            }

            horizontal = testMode ? testHorizontal : Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(horizontal) > 0.01f)
                sprite.flipX = horizontal < 0f;

            bool climbPressed = testMode ? false :
                (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow));

            if (climbPressed && IsGrounded)
                TryStartClimb();

            if (!IsClimbing && climbFrames != null && climbFrames.Length > 1)
                sprite.sprite = Mathf.Abs(horizontal) > 0.05f ? climbFrames[1] : climbFrames[0];
        }

        private void FixedUpdate()
        {
            if (IsClimbing)
            {
                body.velocity = Vector2.zero;
                return;
            }

            CheckGround();

            float target = horizontal * moveSpeed;
            float rate = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
            float vx = Mathf.MoveTowards(body.velocity.x, target, rate * Time.fixedDeltaTime);

            if (IsGrounded && verticalVelocity < 0f) verticalVelocity = 0f;
            verticalVelocity = Mathf.Max(maxFallSpeed, verticalVelocity - gravity * Time.fixedDeltaTime);

            body.velocity = new Vector2(vx, verticalVelocity);
        }

        private void CheckGround()
        {
            Vector2 origin = box.bounds.center;
            Vector2 size = new Vector2(box.bounds.size.x * 0.86f, box.bounds.size.y * 0.6f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down,
                box.bounds.extents.y - size.y * 0.5f + 0.10f, 1 << 0);
            IsGrounded = hit.collider != null && body.velocity.y <= 0.6f;
            if (IsGrounded && body.velocity.y < 0f) verticalVelocity = 0f;
        }

        private void TryStartClimb()
        {
            ClimbVolume volume = ClimbVolume.FindClimbable(transform.position);
            if (volume != null) BeginClimb(volume);
        }

        public bool BeginClimb(ClimbVolume volume)
        {
            if (volume == null || IsClimbing) return false;
            IsClimbing = true;
            StartCoroutine(ClimbRoutine(volume));
            return true;
        }

        private System.Collections.IEnumerator ClimbRoutine(ClimbVolume volume)
        {
            box.enabled = false;
            body.velocity = Vector2.zero;
            verticalVelocity = 0f;

            float startX = transform.position.x;
            float startY = transform.position.y;
            float targetY = volume.TopY;
            float sideX = volume.GetSideX(startX);
            float landingX = volume.GetLandingX(startX);
            climbDuration = Mathf.Max(0.25f, volume.climbDuration);
            climbElapsed = 0f;

            sprite.flipX = volume.CenterX < startX;

            while (climbElapsed < climbDuration)
            {
                climbElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(climbElapsed / climbDuration);

                float x;
                if (t < 0.55f)
                    x = Mathf.Lerp(startX, sideX, Mathf.SmoothStep(0f, 1f, t / 0.55f));
                else
                    x = Mathf.Lerp(sideX, landingX, Mathf.SmoothStep(0f, 1f, (t - 0.55f) / 0.45f));

                float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, t)) +
                          Mathf.Sin(t * Mathf.PI) * 0.10f;

                Vector3 point = new Vector3(x, y, transform.position.z);
                transform.position = point;
                body.position = point;
                yield return null;
            }

            Vector3 landing = new Vector3(landingX, targetY, transform.position.z);
            transform.position = landing;
            body.position = landing;
            body.velocity = Vector2.zero;
            verticalVelocity = 0f;

            box.enabled = true;
            IsClimbing = false;
            if (climbFrames != null && climbFrames.Length > 0) sprite.sprite = climbFrames[0];
        }

        private void AnimateClimb()
        {
            if (climbFrames == null || climbFrames.Length == 0) return;
            int index = Mathf.Min(climbFrames.Length - 1,
                Mathf.FloorToInt(climbElapsed / Mathf.Max(0.001f, climbDuration / climbFrames.Length)));
            sprite.sprite = climbFrames[index];
        }

        public void Teleport(Vector3 position)
        {
            body.velocity = Vector2.zero;
            transform.position = position;
            body.position = position;
            IsGrounded = false;
            verticalVelocity = 0f;
        }

        public void SetTestMode(bool enabled, float horizontal)
        {
            testMode = enabled;
            testHorizontal = horizontal;
        }
    }
}
