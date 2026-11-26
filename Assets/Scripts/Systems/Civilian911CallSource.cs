using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Public 911 call entry points for civilian AI, mission scripts, or future phone UI.
    /// A call is a report from a caller, not proof that the reported event occurred.
    /// </summary>
    public sealed class Civilian911CallSource : MonoBehaviour
    {
        [SerializeField] private WorldIncidentReporter reporter;
        [SerializeField] private CivilianCharacter caller;
        [SerializeField] private float callCooldownSeconds = 30f;
        private float nextAllowedCallTime;

        public void Configure(WorldIncidentReporter incidentReporter, CivilianCharacter civilian)
        {
            reporter = incidentReporter;
            caller = civilian;
        }

        public bool ReportDisturbance(string details = "Caller reports a disturbance.")
            => Submit(WorldIncidentType.Disturbance, details);

        public bool ReportTheft(string details = "Caller reports a theft.")
            => Submit(WorldIncidentType.Theft, details);

        public bool ReportAssault(string details = "Caller reports an assault.")
            => Submit(WorldIncidentType.Assault, details);

        public bool ReportFire(string details = "Caller reports a fire.")
            => Submit(WorldIncidentType.Fire, details);

        public bool RequestMedicalHelp(string details = "Caller reports a medical emergency.")
            => Submit(WorldIncidentType.MedicalEmergency, details);

        public bool ReportSuspiciousActivity(string details = "Caller reports suspicious activity.")
            => Submit(WorldIncidentType.SuspiciousActivity, details);

        public bool ReportVehicleCollision(string details = "Caller reports a vehicle collision.")
            => Submit(WorldIncidentType.VehicleCollision, details);

        private bool Submit(WorldIncidentType type, string details)
        {
            if (reporter == null || Time.time < nextAllowedCallTime) return false;
            string callerName = caller != null ? caller.FullName : "Unknown caller";
            string description = callerName + " called 911: " + details;
            string subject = caller != null ? "Reporting party: " + callerName : "Reporting party unknown";
            reporter.Report(type, description, subject, transform.position);
            nextAllowedCallTime = Time.time + Mathf.Max(1f, callCooldownSeconds);
            return true;
        }
    }
}
