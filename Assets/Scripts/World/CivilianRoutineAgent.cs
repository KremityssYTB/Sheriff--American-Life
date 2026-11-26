using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Simple schedule-driven civilian for greybox testing.
    /// Assign destinations for home, work, shops, and leisure locations.
    /// Production locomotion should later be replaced with baked NavMesh paths.
    /// </summary>
    public sealed class CivilianRoutineAgent : MonoBehaviour
    {
        [Serializable]
        public class RoutineStop
        {
            [Range(0, 23)] public int startHour;
            [Range(0, 59)] public int startMinute;
            public Transform destination;
            [Min(0f)] public float arrivalPauseSeconds = 8f;
            public string activity = "Going about daily life";
        }

        [SerializeField] private WorldClockSystem worldClock;
        [SerializeField] private RoutineStop[] dailyRoutine;
        [SerializeField] private float walkSpeed = 1.35f;
        [SerializeField] private float turnSpeed = 240f;
        [SerializeField] private float arrivalDistance = 1.1f;

        public string CurrentActivity { get; private set; } = "Idle";
        private int activeStop = -1;
        private float pauseRemaining;

        public void Configure(WorldClockSystem clock, RoutineStop[] schedule)
        {
            worldClock = clock;
            dailyRoutine = schedule;
            activeStop = -1;
        }

        private void Update()
        {
            if (worldClock == null || dailyRoutine == null || dailyRoutine.Length == 0) return;

            int desiredStop = FindCurrentStop(worldClock.CurrentHour);
            if (desiredStop != activeStop)
            {
                activeStop = desiredStop;
                pauseRemaining = 0f;
                CurrentActivity = activeStop >= 0 ? dailyRoutine[activeStop].activity : "Idle";
            }

            if (activeStop < 0) return;
            RoutineStop stop = dailyRoutine[activeStop];
            if (stop.destination == null) return;

            Vector3 target = stop.destination.position;
            target.y = transform.position.y;
            Vector3 offset = target - transform.position;

            if (offset.magnitude <= arrivalDistance)
            {
                pauseRemaining += Time.deltaTime;
                if (pauseRemaining >= stop.arrivalPauseSeconds)
                    CurrentActivity = stop.activity;
                return;
            }

            pauseRemaining = 0f;
            Vector3 direction = offset.normalized;
            transform.position = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
            if (direction.sqrMagnitude > .001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
            }
        }

        private int FindCurrentStop(float hour)
        {
            int currentMinutes = Mathf.FloorToInt(hour * 60f);
            int selected = -1;
            int selectedMinutes = -1;

            for (int i = 0; i < dailyRoutine.Length; i++)
            {
                RoutineStop stop = dailyRoutine[i];
                if (stop == null || stop.destination == null) continue;
                int stopMinutes = stop.startHour * 60 + stop.startMinute;
                if (stopMinutes <= currentMinutes && stopMinutes >= selectedMinutes)
                {
                    selected = i;
                    selectedMinutes = stopMinutes;
                }
            }

            // Before the first scheduled stop, use the final stop from yesterday.
            if (selected < 0)
            {
                for (int i = 0; i < dailyRoutine.Length; i++)
                {
                    RoutineStop stop = dailyRoutine[i];
                    if (stop == null || stop.destination == null) continue;
                    int stopMinutes = stop.startHour * 60 + stop.startMinute;
                    if (stopMinutes > selectedMinutes)
                    {
                        selected = i;
                        selectedMinutes = stopMinutes;
                    }
                }
            }
            return selected;
        }
    }
}