using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Small deterministic pedestrian used to exercise a crosswalk sensor.
    /// Walks between opposite sidewalks and pauses before reversing direction.
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class CountyPedestrianCrossingAgent : MonoBehaviour
    {
        [SerializeField] private Vector3 endpointA;
        [SerializeField] private Vector3 endpointB;
        [SerializeField] private float walkingSpeed = 1.25f;
        [SerializeField] private float sidewalkPause = 2.5f;
        private Rigidbody body;
        private CountyFourWayStopController intersectionController;
        private Vector3 destination;
        private float pauseUntil;

        public void Configure(Vector3 pointA, Vector3 pointB, CountyFourWayStopController controller, float speed)
        {
            endpointA = pointA;
            endpointB = pointB;
            intersectionController = controller;
            walkingSpeed = Mathf.Max(0.2f, speed);
            transform.position = endpointA;
            destination = endpointB;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            Collider actorCollider = GetComponent<Collider>();
            actorCollider.isTrigger = true;
        }

        private void FixedUpdate()
        {
            if (Time.time < pauseUntil) return;
            bool standingOnSidewalk = Vector3.Distance(body.position, endpointA) <= 0.12f || Vector3.Distance(body.position, endpointB) <= 0.12f;
            if (standingOnSidewalk && intersectionController != null && intersectionController.IsOccupied) return;
            Vector3 next = Vector3.MoveTowards(body.position, destination, walkingSpeed * Time.fixedDeltaTime);
            body.MovePosition(next);
            Vector3 direction = destination - body.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                body.MoveRotation(Quaternion.LookRotation(direction.normalized, Vector3.up));
            if (Vector3.Distance(next, destination) <= 0.08f)
            {
                destination = destination == endpointA ? endpointB : endpointA;
                pauseUntil = Time.time + sidewalkPause;
            }
        }
    }
}
