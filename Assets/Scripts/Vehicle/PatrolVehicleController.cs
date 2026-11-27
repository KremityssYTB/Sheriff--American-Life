using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Mobile-friendly arcade patrol handling. Uses a Rigidbody so traffic stops,
    /// road props, and other vehicles can physically interact with the patrol unit.
    /// This is a first-pass controller; wheel-collider suspension and tuning remain
    /// a later vehicle-art integration task.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PatrolVehicleController : MonoBehaviour
    {
        [Header("Handling")]
        [SerializeField, Min(1f)] private float acceleration = 18f;
        [SerializeField, Min(1f)] private float maxSpeed = 28f;
        [SerializeField, Min(1f)] private float reverseSpeed = 9f;
        [SerializeField, Min(1f)] private float steering = 85f;
        [SerializeField, Min(0f)] private float coastDrag = 5f;
        [SerializeField, Range(0f, 1f)] private float lateralGrip = .88f;
        [SerializeField, Min(100f)] private float vehicleMass = 1500f;

        private Rigidbody body;
        private float throttle;
        private float steer;
        private float speed;
        private bool controlsEnabled = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = vehicleMass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;
            body.useGravity = true;
        }

        public void SetControlsEnabled(bool value)
        {
            controlsEnabled = value;
            if (!value)
            {
                throttle = 0f;
                steer = 0f;
            }
        }

        public void StopImmediately()
        {
            throttle = 0f;
            steer = 0f;
            speed = 0f;
            if (body != null)
            {
                body.velocity = new Vector3(0f, body.velocity.y, 0f);
                body.angularVelocity = Vector3.zero;
            }
        }

        public void SetThrottle(float value) { throttle = Mathf.Clamp(value, -1f, 1f); }
        public void SetSteering(float value) { steer = Mathf.Clamp(value, -1f, 1f); }

        private void FixedUpdate()
        {
            if (body == null) return;

            if (!controlsEnabled)
            {
                throttle = 0f;
                steer = 0f;
            }

            float limit = throttle >= 0f ? maxSpeed : reverseSpeed;
            float targetSpeed = throttle * limit;
            float rate = Mathf.Abs(throttle) < .05f ? coastDrag : acceleration;
            speed = Mathf.MoveTowards(speed, targetSpeed, rate * Time.fixedDeltaTime);

            Vector3 velocity = body.velocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 forwardVelocity = transform.forward * speed;
            Vector3 corrected = Vector3.Lerp(horizontal, forwardVelocity,
                Mathf.Clamp01(lateralGrip));
            body.velocity = new Vector3(corrected.x, velocity.y, corrected.z);

            float speedFactor = Mathf.Clamp01(Mathf.Abs(speed) / 4f);
            float direction = speed < -.1f ? -1f : 1f;
            float yaw = steer * steering * speedFactor * direction * Time.fixedDeltaTime;
            if (Mathf.Abs(yaw) > 0.0001f)
                body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
