using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum WorldIncidentType
    {
        Disturbance,
        VehicleCollision,
        Theft,
        Assault,
        Fire,
        SuspiciousActivity,
        MedicalEmergency
    }

    [Serializable]
    public sealed class WorldIncident
    {
        public string incidentId;
        public WorldIncidentType type;
        public string description;
        public string subjectDescription;
        public Vector3 location;
        public float occurredAtGameHour;
        public string occurredAtClockTime;
    }

    /// <summary>Central event channel for incidents created by gameplay systems.</summary>
    public static class WorldIncidentBus
    {
        public static event Action<WorldIncident> IncidentOccurred;
        private static int nextIncidentNumber = 1;

        public static WorldIncident Publish(WorldIncidentType type, string description,
            string subjectDescription, Vector3 location, float gameHour)
        {
            var incident = new WorldIncident
            {
                incidentId = "INC-" + (nextIncidentNumber++).ToString("0000"),
                type = type,
                description = description,
                subjectDescription = subjectDescription,
                location = location,
                occurredAtGameHour = gameHour,
                occurredAtClockTime = Mathf.FloorToInt(gameHour).ToString("00") + ":" +
                    Mathf.FloorToInt((gameHour - Mathf.Floor(gameHour)) * 60f).ToString("00")
            };

            IncidentOccurred?.Invoke(incident);
            Debug.Log("[WORLD INCIDENT] " + incident.incidentId + " | " + type + " | " + description);
            return incident;
        }
    }

    /// <summary>Scene component through which gameplay systems publish world incidents.</summary>
    public sealed class WorldIncidentReporter : MonoBehaviour
    {
        [SerializeField] private WorldClockSystem worldClock;
        [SerializeField] private CountyIncidentActorSpawner actorSpawner;

        public void Configure(WorldClockSystem clock, CountyIncidentActorSpawner spawner = null)
        {
            worldClock = clock;
            actorSpawner = spawner;
        }

        public WorldIncident Report(WorldIncidentType type, string description,
            string subjectDescription, Vector3 location)
        {
            float hour = worldClock != null ? worldClock.CurrentHour : 12f;
            if (actorSpawner != null) actorSpawner.Spawn(type, location);
            return WorldIncidentBus.Publish(type, description, subjectDescription, location, hour);
        }
    }
}