using System;
using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    [Serializable]
    public class IncidentReport
    {
        public string incidentNumber;
        public string category;
        public string subjectName;
        public string actionTaken;
        public string outcome;
        public string narrative;
        public string recordedAt;
        public string relatedIncidentId;
        public string caseId;
    }

    /// <summary>Persistent local incident report ledger for fictional gameplay records.</summary>
    public sealed class IncidentReportBook : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int nextSequence = 1;
            public List<IncidentReport> reports = new List<IncidentReport>();
        }

        private const int SaveVersion = 1;
        private const int MaximumRecords = 1000;
        private readonly List<IncidentReport> reports = new List<IncidentReport>();
        private int nextSequence = 1;

        public IReadOnlyList<IncidentReport> Reports => reports;
        public event Action<IncidentReport> ReportAdded;

        private void Awake()
        {
            Load();
        }

        public IncidentReport AddReport(string category, string subject, string action, string outcome, string narrative, string relatedIncidentId = null, string caseId = null)
        {
            var report = new IncidentReport {
                incidentNumber = "OC-" + nextSequence.ToString("000000"),
                category = category,
                subjectName = subject,
                actionTaken = action,
                outcome = outcome,
                narrative = narrative,
                recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                relatedIncidentId = relatedIncidentId,
                caseId = !string.IsNullOrWhiteSpace(caseId) ? caseId.Trim() : relatedIncidentId
            };
            nextSequence++;
            reports.Insert(0, report);
            if (reports.Count > MaximumRecords) reports.RemoveAt(reports.Count - 1);
            Save();
            ReportAdded?.Invoke(report);
            Debug.Log("[SHERIFF REPORT] " + report.incidentNumber + " | " + category + " | " + subject + " | " + outcome);
            return report;
        }

        private void Load()
        {
            if (!LocalJsonSave.TryLoad<SaveData>("incident-reports.json", out SaveData data)) return;
            if (data.version != SaveVersion || data.reports == null)
            {
                Debug.LogWarning("Incident report save has an unsupported format; starting with an empty in-memory ledger.");
                return;
            }
            reports.Clear();
            reports.AddRange(data.reports.FindAll(item => item != null));
            foreach (IncidentReport item in reports)
                if (string.IsNullOrWhiteSpace(item.caseId)) item.caseId = item.relatedIncidentId;
            if (reports.Count > MaximumRecords) reports.RemoveRange(MaximumRecords, reports.Count - MaximumRecords);
            nextSequence = Math.Max(1, data.nextSequence);
            foreach (IncidentReport item in reports)
            {
                if (!string.IsNullOrEmpty(item.incidentNumber) && item.incidentNumber.StartsWith("OC-", StringComparison.Ordinal))
                {
                    int number;
                    if (int.TryParse(item.incidentNumber.Substring(3), out number))
                        nextSequence = Math.Max(nextSequence, number + 1);
                }
            }
        }

        private void Save()
        {
            LocalJsonSave.TrySave("incident-reports.json",
                new SaveData { nextSequence = nextSequence, reports = reports });
        }
    }
}