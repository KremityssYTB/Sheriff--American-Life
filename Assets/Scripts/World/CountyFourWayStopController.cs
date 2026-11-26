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
            public bool emergencyVehicle;
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

        public void RegisterStoppedVehicle(int vehicleId, CountyIntersectionRules.Approach approach, CountyIntersectionRules.Movement movement, float stoppedAt, bool emergencyVehicle = false)
        {
            if (vehicleId == 0 || vehicleId == occupyingVehicleId) return;
            Arrival existing;
            if (waiting.TryGetValue(vehicleId, out existing))
            {
                existing.approach = approach;
                existing.movement = movement;
                existing.emergencyVehicle = emergencyVehicle;
                return;
            }
            waiting.Add(vehicleId, new Arrival { vehicleId = vehicleId, approach = approach, movement = movement, stoppedAt = stoppedAt, emergencyVehicle = emergencyVehicle });
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
                if (mine.emergencyVehicle && !other.emergencyVehicle) continue;
                if (!mine.emergencyVehicle && other.emergencyVehicle) return false;
                if (other.stoppedAt < mine.stoppedAt - simultaneousArrivalTolerance) return false;
                bool simultaneousArrival = Mathf.Abs(mine.stoppedAt - other.stoppedAt) <= simultaneousArrivalTolerance;
                if (simultaneousArrival && CountyIntersectionRules.MustYieldForOncomingTraffic(
                    mine.approach, mine.movement, other.approach, other.movement)) return false;
                if (CountyIntersectionRules.MustYieldAtFourWayStop(mine.approach, mine.stoppedAt,
                    other.approach, other.stoppedAt, simultaneousArrivalTolerance))
                    blockedByRightSideArrival = true;
            }

            if (blockedByRightSideArrival)
            {
                // Detect a genuine priority deadlock. The fallback never selects a
                // vehicle with an earlier-arrival or oncoming left-turn conflict.
                bool everyArrivalHasPriorityConflict = true;
                int fallbackWinnerId = 0;
                int fallbackWinnerApproach = int.MaxValue;

                foreach (Arrival candidate in waiting.Values)
                {
                    bool candidateHasPriorityConflict = false;
                    bool candidateHasEarlierArrival = false;
                    bool candidateHasOncomingConflict = false;
                    bool candidateHasEmergencyConflict = false;

                    foreach (Arrival other in waiting.Values)
                    {
                        if (other.vehicleId == candidate.vehicleId) continue;
                        if (candidate.emergencyVehicle && !other.emergencyVehicle) continue;
                        if (!candidate.emergencyVehicle && other.emergencyVehicle)
                        {
                            candidateHasPriorityConflict = true;
                            candidateHasEmergencyConflict = true;
                            continue;
                        }
                        if (other.stoppedAt < candidate.stoppedAt - simultaneousArrivalTolerance)
                            candidateHasEarlierArrival = true;
                        if (CountyIntersectionRules.MustYieldAtFourWayStop(candidate.approach,
                            candidate.stoppedAt, other.approach, other.stoppedAt,
                            simultaneousArrivalTolerance))
                            candidateHasPriorityConflict = true;
                        bool simultaneousArrival = Mathf.Abs(candidate.stoppedAt - other.stoppedAt) <= simultaneousArrivalTolerance;
                        if (simultaneousArrival && CountyIntersectionRules.MustYieldForOncomingTraffic(candidate.approach,
                            candidate.movement, other.approach, other.movement))
                        {
                            candidateHasOncomingConflict = true;
                            candidateHasPriorityConflict = true;
                        }
                    }

                    if (!candidateHasPriorityConflict && !candidateHasEarlierArrival)
                        everyArrivalHasPriorityConflict = false;

                    if (!candidateHasEarlierArrival && !candidateHasOncomingConflict && !candidateHasEmergencyConflict &&
                        (int)candidate.approach < fallbackWinnerApproach)
                    {
                        fallbackWinnerApproach = (int)candidate.approach;
                        fallbackWinnerId = candidate.vehicleId;
                    }
                    else if (!candidateHasEarlierArrival && !candidateHasOncomingConflict &&
                        !candidateHasEmergencyConflict &&
                        (int)candidate.approach == fallbackWinnerApproach &&
                        (fallbackWinnerId == 0 || candidate.vehicleId < fallbackWinnerId))
                    {
                        fallbackWinnerId = candidate.vehicleId;
                    }
                }

                if (!everyArrivalHasPriorityConflict || fallbackWinnerId == 0 ||
                    fallbackWinnerId != vehicleId)
                    return false;
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