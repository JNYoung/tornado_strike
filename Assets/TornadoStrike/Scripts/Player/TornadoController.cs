using UnityEngine;

namespace TornadoStrike.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class TornadoController : MonoBehaviour
    {
        [Header("Movement")]
        [Min(0.1f)] public float moveSpeed = 8f;
        [Min(0f)] public float acceleration = 16f;
        public bool useWorldBounds = true;
        public Vector2 worldBounds = new Vector2(30f, 30f);

        [Header("Pointer Input")]
        public bool pointerInputEnabled = true;
        [Range(16f, 240f)] public float pointerDeadZone = 32f;
        [Range(40f, 500f)] public float pointerRange = 180f;

        private Rigidbody body;
        private Vector3 currentVelocity;
        private Vector3 desiredDirection;
        private Vector2 pointerStart;
        private bool pointerActive;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void Update()
        {
            desiredDirection = ReadInputDirection();
        }

        private void FixedUpdate()
        {
            var targetVelocity = desiredDirection * moveSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            var next = body.position + currentVelocity * Time.fixedDeltaTime;
            if (useWorldBounds)
            {
                next.x = Mathf.Clamp(next.x, -worldBounds.x, worldBounds.x);
                next.z = Mathf.Clamp(next.z, -worldBounds.y, worldBounds.y);
            }

            next.y = 0f;

            body.MovePosition(next);
        }

        private Vector3 ReadInputDirection()
        {
            var axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (pointerInputEnabled)
            {
                var pointerAxis = ReadPointerAxis();
                if (pointerAxis.sqrMagnitude > axis.sqrMagnitude)
                {
                    axis = pointerAxis;
                }
            }

            axis = Vector2.ClampMagnitude(axis, 1f);
            return new Vector3(axis.x, 0f, axis.y);
        }

        private Vector2 ReadPointerAxis()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    pointerStart = touch.position;
                    pointerActive = true;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    pointerActive = false;
                }

                return pointerActive ? PointerDeltaToAxis(touch.position - pointerStart) : Vector2.zero;
            }

            if (Input.GetMouseButtonDown(0))
            {
                pointerStart = Input.mousePosition;
                pointerActive = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                pointerActive = false;
            }

            return pointerActive ? PointerDeltaToAxis((Vector2)Input.mousePosition - pointerStart) : Vector2.zero;
        }

        private Vector2 PointerDeltaToAxis(Vector2 delta)
        {
            if (delta.magnitude < pointerDeadZone)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(delta / pointerRange, 1f);
        }
    }
}
