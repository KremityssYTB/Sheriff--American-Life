using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Listens for nearby world incidents and stores a basic hearing/visual observation.
    /// This is a prototype: it does not yet model memory decay, hearing occlusion, or witness interviews.
    /// </summary>
    [RequireComponent(typeof(CivilianCharacter))]
    public sealed class CivilianWitnessAgent : MonoBehaviour
    {
        [SerializeField] private float hearingRadius = 45f;
        [SerializeField] private float visualRadius = 32f;
        [SerializeField, Range(20f, 180f)] private float fieldOfView = 110f;

        private CivilianCharacter civilian;
        private WitnessStatementBook statementBook;

        public void Configure(WitnessStatementBook book)
        {
            statementBook = book;
            if (civilian == null) civilian = GetComponent<CivilianCharacter>();
        }

        private void Awake()
        {
            civilian = GetComponent<CivilianCharacter>();
        }

        private void OnEnable()
        {
            WorldIncidentBus.IncidentOccurred += OnIncidentOccurred;
        }

        private void OnDisable()
        {
            WorldIncidentBus.IncidentOccurred -= OnIncidentOccurred;
        }

        private void OnIncidentOccurred(WorldIncident incident)
        {
            if (incident == null || statementBook == null || civilian == null) return;

            Vector3 toIncident = incident.location - transform.position;
            float distance = toIncident.magnitude;
            if (distance > hearingRadius) return;

            bool canSee = distance <= visualRadius &&
                Vector3.Angle(transform.forward, toIncident) <= fieldOfView * 0.5f;

            string observation = canSee ? "Seen and heard" : "Heard nearby";
            float confidence = canSee ? 0.9f : Mathf.Clamp01(1f - distance / hearingRadius) * 0.65f;

            statementBook.Add(new WitnessStatement
            {
                incidentId = incident.incidentId,
                witnessName = civilian.FullName,
                incidentDescription = incident.description,
                subjectDescription = canSee ? incident.subjectDescription : "Not visually identified",
                observationType = observation,
                confidence = confidence,
                witnessPosition = transform.position
            });
        }
    }
}