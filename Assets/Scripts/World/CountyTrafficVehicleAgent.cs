using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Basic waypoint-driven civilian traffic vehicle with Rigidbody collision reporting.
    /// Designed as a lightweight mobile greybox system, not final driving AI.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CountyTrafficVehicleAgent : MonoBehaviour
    {
        [SerializeField] private Transform[] route;
        [SerializeField] private float targetSpeed = 9f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float braking = 8f;
        [SerializeField] private float steeringRate = 95f;
        [SerializeField] private float waypointRadius = 7f;
        [SerializeField] private float collisionReportSpeed = 3.5f;
        [SerializeField] private WorldIncidentReporter incidentReporter;
        [SerializeField] private TrafficSignalController trafficSignal;
        [SerializeField] private bool eastWestRoad = true;
        [SerializeField] private Vector3 stopLinePosition;
        [SerializeField] private float stopLineDistance = 12f;

        [Header("Optional Four-Way Stop")]
        [SerializeField] private CountyFourWayStopController fourWayStop;
        [SerializeField] private CountyIntersectionRules.Approach stopApproach;
        [SerializeField] private CountyIntersectionRules.Movement stopMovement;
        [SerializeField] private Vector3 stopClearancePosition;
        [SerializeField] private Vector3 pedestrianStopLinePosition;
        [SerializeField] private float stopDetectionDistance = 12f;
        [SerializeField] private float stopPositionTolerance = 2.8f;
        [SerializeField] private float frontBumperOffset = 2.1f;
        [SerializeField] private float stoppedSpeedThreshold = 0.25f;
        [SerializeField] private float minimumStopDwellSeconds = 1.25f;
        [Header("Traffic Following")]
        [SerializeField] private float minimumFollowingDistance = 2.5f;
        [SerializeField] private float followingTimeHeadway = 1.35f;
        [SerializeField] private float followingDetectionDistance = 18f;
        [SerializeField] private float followingProbeRadius = 0.75f;
        private static readonly RaycastHit[] FollowingHits = new RaycastHit[16];
        private bool stopRegistered;
        private bool hasStopReservation;
        private float stopDwellStartedAt;

        private Rigidbody body;
        private int waypointIndex;
        private float nextCollisionReportTime;
        private const float CollisionCooldown = 20f;

        public void Configure(Transform[] waypoints, WorldIncidentReporter reporter, float speed,
            TrafficSignalController signal = null, bool isEastWest = true, Vector3 stopLine = default)
        {
            route = waypoints;
            incidentReporter = reporter;
            targetSpeed = Mathf.Max(2f, speed);
            trafficSignal = signal;
            eastWestRoad = isEastWest;
            stopLinePosition = stopLine;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1150f;
            body.drag = .12f;
            body.angularDrag = 2.5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void ConfigureFourWayStop(CountyFourWayStopController controller,
            CountyIntersectionRules.Approach approach, CountyIntersectionRules.Movement movement,
            Vector3 stopLine, Vector3 clearancePosition, Vector3 crosswalkStopLine)
        {
            fourWayStop = controller;
            stopApproach = approach;
            stopMovement = movement;
            stopLinePosition = stopLine;
            stopClearancePosition = clearancePosition;
            pedestrianStopLinePosition = crosswalkStopLine;
            stopRegistered = false;
            hasStopReservation = false;
            stopDwellStartedAt = 0f;
        }

        private void FixedUpdate()
        {
            if (route == null || route.Length == 0 || body == null) return;
            Transform target = route[waypointIndex];
            if (target == null) { AdvanceWaypoint(); return; }

            Vector3 flatDirection = target.position - transform.position;
            flatDirection.y = 0f;
            bool waitingAtFourWayStopLine = fourWayStop != null && !hasStopReservation && waypointIndex == 0;
            if (flatDirection.sqrMagnitude <= waypointRadius * waypointRadius && !waitingAtFourWayStopLine)
            {
                AdvanceWaypoint();
                return;
            }

            Vector3 direction = flatDirection.normalized;
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            Quaternion desired = Quaternion.LookRotation(direction, Vector3.up);
            body.MoveRotation(Quaternion.RotateTowards(body.rotation, desired,
                steeringRate * Time.fixedDeltaTime));

            float currentSpeed = Vector3.Dot(body.velocity, transform.forward);
            float speedError = targetSpeed - currentSpeed;
            float followingSpeedLimit = CalculateFollowingSpeedLimit(currentSpeed);
            speedError = Mathf.Min(speedError, followingSpeedLimit - currentSpeed);
            Vector3 toStopLine = stopLinePosition - transform.position;
            toStopLine.y = 0f;
            float distanceToLine = Vector3.Dot(toStopLine, transform.forward);
            bool approachingLine = trafficSignal != null && distanceToLine > 0f && distanceToLine < stopLineDistance;
            if (approachingLine && !trafficSignal.CanProceed(eastWestRoad))
                speedError = -Mathf.Max(0f, currentSpeed);

            if (fourWayStop != null && !hasStopReservation && fourWayStop.IsPedestrianConflictActive)
            {
                float crosswalkDistance = Vector3.Dot(pedestrianStopLinePosition - transform.position, transform.forward);
                if (crosswalkDistance > 0f && crosswalkDistance <= 18f)
                {
                    float crosswalkStopOffset = frontBumperOffset + 0.5f;
                    float safeCrosswalkSpeed = Mathf.Sqrt(2f * braking * Mathf.Max(0f, crosswalkDistance - crosswalkStopOffset));
                    speedError = Mathf.Min(speedError, safeCrosswalkSpeed - currentSpeed);
                    if (crosswalkDistance <= crosswalkStopOffset && Mathf.Abs(currentSpeed) <= stoppedSpeedThreshold)
                        speedError = -Mathf.Max(0f, currentSpeed);
                }
            }

            if (fourWayStop != null && !hasStopReservation)
            {
                float stopDistance = Vector3.Dot(stopLinePosition - transform.position, transform.forward);
                if (stopRegistered && !hasStopReservation &&
                    (stopDistance < -0.5f || Mathf.Abs(currentSpeed) > stoppedSpeedThreshold * 2f))
                {
                    // A vehicle that rolls forward again must lose its queued arrival.
                    fourWayStop.RemoveVehicle(GetInstanceID());
                    stopRegistered = false;
                    stopDwellStartedAt = 0f;
                }

                if (stopDistance > 0f && stopDistance <= stopDetectionDistance)
                {
                    float safeApproachSpeed = Mathf.Sqrt(2f * braking * Mathf.Max(0f, stopDistance - stopPositionTolerance));
                    speedError = Mathf.Min(speedError, safeApproachSpeed - currentSpeed);
                    if (stopDistance <= stopPositionTolerance && Mathf.Abs(currentSpeed) <= stoppedSpeedThreshold)
                    {
                        if (!stopRegistered)
                        {
                            fourWayStop.RegisterStoppedVehicle(GetInstanceID(), stopApproach, stopMovement, Time.time);
                            stopRegistered = true;
                            stopDwellStartedAt = Time.time;
                        }

                        bool completedStopDwell = Time.time - stopDwellStartedAt >= minimumStopDwellSeconds;
                        if (completedStopDwell)
                            hasStopReservation = fourWayStop.TryAcquireIntersection(GetInstanceID());

                        if (!hasStopReservation) speedError = -Mathf.Max(0f, currentSpeed);
                    }
                }
            }

            if (hasStopReservation &&
                Vector3.Distance(transform.position, stopClearancePosition) <= waypointRadius)
            {
                fourWayStop.ReleaseIntersection(GetInstanceID());
                fourWayStop = null;
                hasStopReservation = false;
                stopRegistered = false;
            }
            if (Mathf.Abs(angle) > 65f) speedError = Mathf.Min(speedError, 0f);
            if (speedError >= 0f)
                body.AddForce(transform.forward * Mathf.Min(acceleration, speedError * 2f), ForceMode.Acceleration);
            else
                body.AddForce(-transform.forward * Mathf.Min(braking, -speedError * 2f), ForceMode.Acceleration);

            Vector3 lateral = Vector3.Project(body.velocity, transform.right);
            body.AddForce(-lateral * 1.8f, ForceMode.Acceleration);
        }

        private float CalculateFollowingSpeedLimit(float currentSpeed)
        {
            Vector3 origin = transform.position + Vector3.up * 0.6f + transform.forward * 2.2f;
            int hitCount = Physics.SphereCastNonAlloc(origin, followingProbeRadius, transform.forward,
                FollowingHits, followingDetectionDistance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            float nearestGap = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                CountyTrafficVehicleAgent other = FollowingHits[i].collider != null
                    ? FollowingHits[i].collider.GetComponentInParent<CountyTrafficVehicleAgent>() : null;
                if (other == null || other == this) continue;
                Vector3 toOther = other.transform.position - transform.position;
                toOther.y = 0f;
                if (Vector3.Dot(toOther.normalized, transform.forward) < 0.65f) continue;
                nearestGap = Mathf.Min(nearestGap, FollowingHits[i].distance);
            }

            if (float.IsPositiveInfinity(nearestGap)) return targetSpeed;

            float availableGap = Mathf.Max(0f, nearestGap - minimumFollowingDistance);
            float timeHeadwayLimit = availableGap / Mathf.Max(0.25f, followingTimeHeadway);
            float brakingLimit = Mathf.Sqrt(2f * braking * availableGap);
            return Mathf.Min(targetSpeed, timeHeadwayLimit, brakingLimit);
        }

        private void AdvanceWaypoint()
        {
            if (route == null || route.Length == 0) return;
            waypointIndex = (waypointIndex + 1) % route.Length;
        }

        private void OnDisable()
        {
            if (fourWayStop != null) fourWayStop.RemoveVehicle(GetInstanceID());
        }

        private void OnCollisionEnter(Collision collision)
        {
            CountyTrafficVehicleAgent other = collision.collider.GetComponentInParent<CountyTrafficVehicleAgent>();
            if (other == null || other == this || GetInstanceID() > other.GetInstanceID()) return;
            if (Time.time < nextCollisionReportTime || incidentReporter == null) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < collisionReportSpeed) return;

            nextCollisionReportTime = Time.time + CollisionCooldown;
            other.nextCollisionReportTime = nextCollisionReportTime;
            Vector3 point = collision.contactCount > 0
                ? collision.GetContact(0).point : (transform.position + other.transform.position) * .5f;

            incidentReporter.Report(WorldIncidentType.VehicleCollision,
                "Traffic collision: two county vehicles collided. Estimated impact speed " +
                impactSpeed.ToString("0.0") + " m/s. Injuries unknown.",
                "Two civilian drivers; identities and injury status unconfirmed.", point);
        }
    }
}
