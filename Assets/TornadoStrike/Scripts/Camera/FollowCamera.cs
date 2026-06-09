using UnityEngine;

namespace TornadoStrike.CameraRig
{
    [DisallowMultipleComponent]
    public sealed class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 16f, -14f);
        [Range(0.02f, 1f)] public float smoothTime = 0.16f;
        public bool lookAtTarget = true;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, smoothTime);

            if (lookAtTarget)
            {
                transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            }
        }
    }
}
