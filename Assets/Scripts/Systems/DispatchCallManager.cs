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
        public string caseId;
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

    /// <summary>Converts world incidents into persistent local dispatch calls and enforces their lifecycle.</summary>
    public sealed class DispatchCallManager : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int nextSequence = 1;
            public List<DispatchCall> calls = new List<DispatchCall>();
        }

        private const int SaveVersion = 1;
        private const int MaximumCalls = 1000;
        [SerializeField] private bool createCallsFromWorldIncidents = true;
        private readonly List<DispatchCall> calls = new List<DispatchCall>();
        private readonly Dictionary<string, DispatchCall> callsByIncident = new Dictionary<string, DispatchCall>();
        private int nextSequence = 1;
        private IncidentReportBook reportBook;

        public void Configure(IncidentReportBook reports) => reportBook = reports;

        public IReadOnlyList<DispatchCall> Calls => calls;
        public event Action<DispatchCall> CallCreated;
        public event Action<DispatchCall> CallUpdated;

        private void Awake()
        {
            Load();
        }

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
            if (!string.IsNullOrEmpty(incident.incidentId) && callsByIncident.ContainsKey(incident.incidentId)) return;

            var call = new DispatchCall
            {
                callId = "CAD-" + nextSequence.ToString("000000"),
                sourceIncidentId = incident.incidentId,
                caseId = incident.incidentId,
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

            nextSequence++;
            calls.Add(call);
            if (!string.IsNullOrEmpty(incident.incidentId))
                callsByIncident[incident.incidentId] = call;
            if (calls.Count > MaximumCalls) calls.RemoveAt(0);
            Save();
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
            call.disposition = disposition.Trim();
            if (reportBook != null)
                reportBook.AddReport("Dispatch resolution", call.title, "Responded to call", call.disposition,
                    "Call " + call.callId + " resolved by " + call.assignedUnit + ". " + call.description,
                    call.sourceIncidentId, call.caseId);
            Save();
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
            call.disposition = reason.Trim();
            if (reportBook != null)
                reportBook.AddReport("Dispatch cancellation", call.title, "Cancel call", call.disposition,
                    "Call " + call.callId + " was cancelled. " + call.description, call.sourceIncidentId);
            Save();
            CallUpdated?.Invoke(call);
            return true;
        }

        public DispatchCall Find(string callId)
        {
            return calls.Find(c => c != null && c.callId == callId);
        }

        private bool Transition(string callId, DispatchCallStatus required,
            DispatchCallStatus next, string unitName = null)
        {
            DispatchCall call = Find(callId);
            if (call == null || call.status != required) return false;
            call.status = next;
            if (!string.IsNullOrWhiteSpace(unitName)) call.assignedUnit = unitName;
            Save();
            CallUpdated?.Invoke(call);
            return true;
        }

        private void Load()
        {
            if (!LocalJsonSave.TryLoad<SaveData>("dispatch-calls.json", out SaveData data)) return;
            if (data.version != SaveVersion || data.calls == null)
            {
                Debug.LogWarning("Dispatch save has an unsupported format; starting with an empty in-memory ledger.");
                return;
            }
            calls.Clear();
            callsByIncident.Clear();
            calls.AddRange(data.calls.FindAll(item => item != null && !string.IsNullOrEmpty(item.callId)));
            foreach (DispatchCall item in calls)
                if (string.IsNullOrWhiteSpace(item.caseId)) item.caseId = item.sourceIncidentId;
            if (calls.Count > MaximumCalls) calls.RemoveRange(0, calls.Count - MaximumCalls);
            nextSequence = Math.Max(1, data.nextSequence);
            foreach (DispatchCall item in calls)
            {
                if (item.callId.StartsWith("CAD-", StringComparison.Ordinal))
                {
                    int number;
                    if (int.TryParse(item.callId.Substring(4), out number))
                        nextSequence = Math.Max(nextSequence, number + 1);
                }
                if (!string.IsNullOrEmpty(item.sourceIncidentId) && !callsByIncident.ContainsKey(item.sourceIncidentId))
                    callsByIncident.Add(item.sourceIncidentId, item);
            }
        }

        private void Save()
        {
            LocalJsonSave.TrySave("dispatch-calls.json",
                new SaveData { nextSequence = nextSequence, calls = calls });
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