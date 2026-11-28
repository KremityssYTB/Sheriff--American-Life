using System;
using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum DispatchCallStatus
    {
        Available,
        Accepted,
        EnRoute,
        OnScene,
        Resolved,
        Cancelled
    }

    public enum DispatchPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    [Serializable]
    public sealed class DispatchCall
    {
        public string callId;
        public string sourceIncidentId;
        public WorldIncidentType incidentType;
        public DispatchPriority priority;
        public DispatchCallStatus status;
        public string title;
        public string description;
        public string subjectDescription;
        public Vector3 location;
        public string receivedAtGameTime;
        public string assignedUnit;
        public string disposition;
    }

    /// <summary>
    /// Converts world incident events into dispatch calls and enforces a simple call lifecycle.
    /// Calls are session-only and intended for a future radio/MDT interface.
    /// </summary>
    public sealed class DispatchCallManager : MonoBehaviour
    {
        [SerializeField] private bool createCallsFromWorldIncidents = true;
        private readonly List<DispatchCall> calls = new List<DispatchCall>();
        private readonly Dictionary<string, DispatchCall> callsByIncident = new Dictionary<string, DispatchCall>();
        private IncidentReportBook reportBook;

        public void Configure(IncidentReportBook reports) => reportBook = reports;

        public IReadOnlyList<DispatchCall> Calls => calls;
        public event Action<DispatchCall> CallCreated;
        public event Action<DispatchCall> CallUpdated;

        private void OnEnable()
        {
            WorldIncidentBus.IncidentOccurred += HandleWorldIncident;
        }

        private void OnDisable()
        {
            WorldIncidentBus.IncidentOccurred -= HandleWorldIncident;
        }

        private void HandleWorldIncident(WorldIncident incident)
        {
            if (!createCallsFromWorldIncidents || incident == null) return;
            if (callsByIncident.ContainsKey(incident.incidentId)) return;

            var call = new DispatchCall
            {
                callId = "CAD-" + (calls.Count + 1).ToString("0000"),
                sourceIncidentId = incident.incidentId,
                incidentType = incident.type,
                priority = GetPriority(incident.type),
                status = DispatchCallStatus.Available,
                title = GetTitle(incident.type),
                description = incident.description,
                subjectDescription = incident.subjectDescription,
                location = incident.location,
                receivedAtGameTime = incident.occurredAtClockTime,
                assignedUnit = "Unassigned",
                disposition = string.Empty
            };

            calls.Add(call);
            callsByIncident.Add(incident.incidentId, call);
            CallCreated?.Invoke(call);
            Debug.Log("[DISPATCH] " + call.callId + " | " + call.priority + " | " + call.title);
        }

        public bool Accept(string callId, string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName)) return false;
            return Transition(callId, DispatchCallStatus.Available, DispatchCallStatus.Accepted, unitName.Trim());
        }

        public bool MarkEnRoute(string callId)
        {
            return Transition(callId, DispatchCallStatus.Accepted, DispatchCallStatus.EnRoute);
        }

        public bool MarkOnScene(string callId)
        {
            return Transition(callId, DispatchCallStatus.EnRoute, DispatchCallStatus.OnScene);
        }

        public bool Resolve(string callId, string disposition)
        {
            if (string.IsNullOrWhiteSpace(disposition)) return false;
            DispatchCall call = Find(callId);
            if (call == null || call.status != DispatchCallStatus.OnScene) return false;
            call.status = DispatchCallStatus.Resolved;
            call.disposition = disposition;
            if (reportBook != null)
                reportBook.AddReport("Dispatch resolution", call.title, "Responded to call", disposition,
                    "Call " + call.callId + " resolved by " + call.assignedUnit + ". " + call.description,
                    call.sourceIncidentId);
            CallUpdated?.Invoke(call);
            return true;
        }

        public bool Cancel(string callId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return false;
            DispatchCall call = Find(callId);
            if (call == null || call.status == DispatchCallStatus.Resolved ||
                call.status == DispatchCallStatus.Cancelled) return false;
            call.status = DispatchCallStatus.Cancelled;
            call.disposition = reason;
            if (reportBook != null)
                reportBook.AddReport("Dispatch cancellation", call.title, "Cancel call", reason,
                    "Call " + call.callId + " was cancelled. " + call.description, call.sourceIncidentId);
            CallUpdated?.Invoke(call);
            return true;
        }

        public DispatchCall Find(string callId)
        {
            return calls.Find(c => c.callId == callId);
        }

        private bool Transition(string callId, DispatchCallStatus required,
            DispatchCallStatus next, string unitName = null)
        {
            DispatchCall call = Find(callId);
            if (call == null || call.status != required) return false;
            call.status = next;
            if (!string.IsNullOrWhiteSpace(unitName)) call.assignedUnit = unitName;
            CallUpdated?.Invoke(call);
            return true;
        }

        private static DispatchPriority GetPriority(WorldIncidentType type)
        {
            switch (type)
            {
                case WorldIncidentType.Assault:
                case WorldIncidentType.Fire:
                case WorldIncidentType.MedicalEmergency:
                    return DispatchPriority.Critical;
                case WorldIncidentType.VehicleCollision:
                case WorldIncidentType.Theft:
                    return DispatchPriority.High;
                case WorldIncidentType.Disturbance:
                    return DispatchPriority.Medium;
                default:
                    return DispatchPriority.Low;
            }
        }

        private static string GetTitle(WorldIncidentType type)
        {
            switch (type)
            {
                case WorldIncidentType.VehicleCollision: return "Vehicle collision";
                case WorldIncidentType.Theft: return "Reported theft";
                case WorldIncidentType.Assault: return "Assault reported";
                case WorldIncidentType.Fire: return "Fire response";
                case WorldIncidentType.MedicalEmergency: return "Medical emergency";
                case WorldIncidentType.Disturbance: return "Disturbance";
                default: return "Suspicious activity";
            }
        }
    }
}