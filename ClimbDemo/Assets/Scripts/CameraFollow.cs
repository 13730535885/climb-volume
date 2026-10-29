using UnityEngine;

namespace ClimbDemo
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 1.7f, -10f);
        public float smooth = 0.16f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 wanted = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, smooth);
        }
    }
}
