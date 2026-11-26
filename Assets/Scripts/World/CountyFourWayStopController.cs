using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Conservative entry arbiter for a future four-way-stop scene.
    /// Vehicles register after a complete stop and receive exclusive occupancy.
    /// </summary>
    public sealed class CountyFourWayStopController : MonoBehaviour
    {
        private sealed class Arrival
        {
            public int vehicleId;
            public CountyIntersectionRules.Approach approach;
            public CountyIntersectionRules.Movement movement;
            public float stoppedAt;
        }

        [SerializeField] private float simultaneousArrivalTolerance = 0.15f;
        [SerializeField] private float arbitrationDelay = 0.25f;
        private readonly Dictionary<int, Arrival> waiting = new Dictionary<int, Arrival>();
        private int occupyingVehicleId;
        private bool pedestrianConflict;
        private readonly HashSet<int> pedestrianOccupants = new HashSet<int>();

        public int OccupyingVehicleId => occupyingVehicleId;
        public int WaitingVehicleCount => waiting.Count;
        public bool IsOccupied => occupyingVehicleId != 0;
        public bool IsPedestrianConflictActive => pedestrianConflict;

        public void RegisterStoppedVehicle(int vehicleId, CountyIntersectionRules.Approach approach, CountyIntersectionRules.Movement movement, float stoppedAt)
        {
            if (vehicleId == 0 || vehicleId == occupyingVehicleId) return;
            Arrival existing;
            if (waiting.TryGetValue(vehicleId, out existing))
            {
                existing.approach = approach;
                existing.movement = movement;
                return;
            }
            waiting.Add(vehicleId, new Arrival { vehicleId = vehicleId, approach = approach, movement = movement, stoppedAt = stoppedAt });
        }

        public bool TryAcquireIntersection(int vehicleId)
        {
            if (occupyingVehicleId == vehicleId) return true;
            if (occupyingVehicleId != 0 || pedestrianConflict) return false;
            Arrival mine;
            if (!waiting.TryGetValue(vehicleId, out mine)) return false;
            // Allow other approaches to register arrivals from the same physics-time window
            // before awarding the intersection to the first callback that asks.
            if (Time.time - mine.stoppedAt < arbitrationDelay) return false;

            bool blockedByRightSideArrival = false;
            foreach (Arrival other in waiting.Values)
            {
                if (other.vehicleId == vehicleId) continue;
                if (other.stoppedAt < mine.stoppedAt - simultaneousArrivalTolerance) return false;
                if (CountyIntersectionRules.MustYieldForOncomingTraffic(mine.approach, mine.movement, other.approach, other.movement)) return false;
                if (CountyIntersectionRules.MustYieldAtFourWayStop(mine.approach, mine.stoppedAt,
                    other.approach, other.stoppedAt, simultaneousArrivalTolerance))
                    blockedByRightSideArrival = true;
            }

            if (blockedByRightSideArrival)
            {
                // If at least one waiting vehicle has no higher-priority conflict, let it go first.
                // Only use a stable simulation tie-break when every simultaneous arrival is blocked
                // by another vehicle on its right (the all-four-arrive-at-once deadlock case).
                bool anUnblockedCandidateExists = false;
                foreach (Arrival candidate in waiting.Values)
                {
                    bool candidateBlocked = false;
                    foreach (Arrival other in waiting.Values)
                    {
                        if (other.vehicleId == candidate.vehicleId) continue;
                        if (other.stoppedAt < candidate.stoppedAt - simultaneousArrivalTolerance ||
                            CountyIntersectionRules.MustYieldForOncomingTraffic(candidate.approach,
                                candidate.movement, other.approach, other.movement) ||
                            CountyIntersectionRules.MustYieldAtFourWayStop(candidate.approach,
                                candidate.stoppedAt, other.approach, other.stoppedAt,
                                simultaneousArrivalTolerance))
                        {
                            candidateBlocked = true;
                            break;
                        }
                    }
                    if (!candidateBlocked) { anUnblockedCandidateExists = true; break; }
                }

                if (anUnblockedCandidateExists) return false;

                foreach (Arrival other in waiting.Values)
                {
                    if (other.vehicleId == vehicleId) continue;
                    bool simultaneous = Mathf.Abs(other.stoppedAt - mine.stoppedAt) <= simultaneousArrivalTolerance;
                    if (!simultaneous) continue;
                    if ((int)other.approach < (int)mine.approach ||
                        (other.approach == mine.approach && other.vehicleId < mine.vehicleId))
                        return false;
                }
            }

            occupyingVehicleId = vehicleId;
            waiting.Remove(vehicleId);
            return true;
        }

        public void ReleaseIntersection(int vehicleId)
        {
            if (occupyingVehicleId == vehicleId) occupyingVehicleId = 0;
        }

        public void RemoveVehicle(int vehicleId)
        {
            waiting.Remove(vehicleId);
            if (occupyingVehicleId == vehicleId) occupyingVehicleId = 0;
        }

        public void SetPedestrianConflict(bool occupied) { pedestrianConflict = occupied || pedestrianOccupants.Count > 0; }

        public void SetPedestrianOccupancy(int pedestrianId, bool inConflict)
        {
            if (pedestrianId == 0) return;
            if (inConflict) pedestrianOccupants.Add(pedestrianId);
            else pedestrianOccupants.Remove(pedestrianId);
            pedestrianConflict = pedestrianOccupants.Count > 0;
        }

        private void OnDisable()
        {
            waiting.Clear();
            pedestrianOccupants.Clear();
            occupyingVehicleId = 0;
            pedestrianConflict = false;
        }
    }
}