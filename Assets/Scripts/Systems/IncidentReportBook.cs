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
    }

    /// <summary>Session-only incident log. Persistent save storage will be added later.</summary>
    public sealed class IncidentReportBook : MonoBehaviour
    {
        private readonly List<IncidentReport> reports = new List<IncidentReport>();
        public IReadOnlyList<IncidentReport> Reports => reports;
        public event Action<IncidentReport> ReportAdded;

        public IncidentReport AddReport(string category, string subject, string action, string outcome, string narrative)
        {
            var report = new IncidentReport {
                incidentNumber = "OC-" + (reports.Count + 1).ToString("0000"),
                category = category,
                subjectName = subject,
                actionTaken = action,
                outcome = outcome,
                narrative = narrative,
                recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            reports.Add(report);
            ReportAdded?.Invoke(report);
            Debug.Log("[SHERIFF REPORT] " + report.incidentNumber + " | " + category + " | " + subject + " | " + outcome);
            return report;
        }
    }
}