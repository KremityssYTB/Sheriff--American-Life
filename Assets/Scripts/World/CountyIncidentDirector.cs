using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Lightweight ambient incident producer for the greybox county.
    /// Scenarios are simulated dispatch reports; they do not yet spawn physical crime,
    /// fire, medical, or traffic actors.
    /// </summary>
    public sealed class CountyIncidentDirector : MonoBehaviour
    {
        [Serializable]
        public sealed class Scenario
        {
            public WorldIncidentType type;
            public string description;
            public string subjectDescription;
            public Vector3 location;
            [Min(0f)] public float weight = 1f;
        }

        [SerializeField] private WorldIncidentReporter reporter;
        [SerializeField] private Scenario[] scenarios;
        [SerializeField] private bool runAmbientSimulation = true;
        [SerializeField] private float firstEventDelay = 25f;
        [SerializeField] private float minimumInterval = 65f;
        [SerializeField] private float maximumInterval = 115f;
        [SerializeField] private int maximumEventsPerSession = 12;

        private float nextEventTime;
        private int eventsCreated;
        private bool configured;
        private CountyIncidentActorSpawner actorSpawner;

        public int EventsCreated => eventsCreated;
        public event Action<WorldIncident> ScenarioReported;

        public void Configure(WorldIncidentReporter incidentReporter, Scenario[] availableScenarios)
        {
            reporter = incidentReporter;
            scenarios = availableScenarios;
            actorSpawner = GetComponent<CountyIncidentActorSpawner>();
            configured = true;
            ScheduleNext(firstEventDelay);
        }

        private void Start()
        {
            if (!configured) ScheduleNext(firstEventDelay);
        }

        private void Update()
        {
            if (!runAmbientSimulation || reporter == null || scenarios == null ||
                scenarios.Length == 0 || eventsCreated >= maximumEventsPerSession) return;
            if (Time.time < nextEventTime) return;

            Scenario scenario = ChooseScenario();
            if (scenario == null)
            {
                ScheduleNext(minimumInterval);
                return;
            }

            if (actorSpawner != null) actorSpawner.Spawn(scenario.type, scenario.location);
            WorldIncident incident = reporter.Report(scenario.type, scenario.description,
                scenario.subjectDescription, scenario.location);
            eventsCreated++;
            ScenarioReported?.Invoke(incident);
            ScheduleNext(UnityEngine.Random.Range(minimumInterval,
                Mathf.Max(minimumInterval, maximumInterval)));
        }

        public WorldIncident TriggerScenario(int index)
        {
            if (reporter == null || scenarios == null || index < 0 || index >= scenarios.Length || eventsCreated >= maximumEventsPerSession)
                return null;
            Scenario scenario = scenarios[index];
            if (scenario == null) return null;
            if (actorSpawner != null) actorSpawner.Spawn(scenario.type, scenario.location);
            WorldIncident incident = reporter.Report(scenario.type, scenario.description,
                scenario.subjectDescription, scenario.location);
            eventsCreated++;
            ScenarioReported?.Invoke(incident);
            return incident;
        }

        private Scenario ChooseScenario()
        {
            float total = 0f;
            foreach (Scenario scenario in scenarios)
                if (scenario != null) total += Mathf.Max(0f, scenario.weight);
            if (total <= 0f) return null;

            float roll = UnityEngine.Random.Range(0f, total);
            foreach (Scenario scenario in scenarios)
            {
                if (scenario == null) continue;
                roll -= Mathf.Max(0f, scenario.weight);
                if (roll <= 0f) return scenario;
            }
            return scenarios[scenarios.Length - 1];
        }

        private void ScheduleNext(float delay)
        {
            nextEventTime = Time.time + Mathf.Max(1f, delay);
        }
    }
}
