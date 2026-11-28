using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum CitationPaymentStatus { Outstanding, Paid, Dismissed }

    [Serializable]
    public sealed class TrafficCitation
    {
        public string citationNumber;
        public string caseId;
        public string driverName;
        public string licenseNumber;
        public string plateNumber;
        public string issuedAt;
        public string location;
        public string violations;
        public int fineAmount;
        public CitationPaymentStatus paymentStatus;
        public string officerUnit;
    }

    [Serializable]
    internal sealed class TrafficCitationSaveData
    {
        public int schemaVersion = 1;
        public int nextSequence = 1;
        public List<TrafficCitation> citations = new List<TrafficCitation>();
    }

    /// <summary>Persistent local ledger for fictional traffic-stop citations.</summary>
    public sealed class TrafficCitationBook : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 1;
        private const string SaveFileName = "traffic-citations.json";

        private readonly List<TrafficCitation> citations = new List<TrafficCitation>();
        private int nextSequence = 1;
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public IReadOnlyList<TrafficCitation> Citations => citations;
        public event Action<TrafficCitation> CitationAdded;
        public event Action<TrafficCitation> CitationUpdated;

        private void Awake()
        {
            Load();
        }

        public TrafficCitation Issue(TrafficStopDriverProfile profile, string location, string officerUnit, string caseId = null)
        {
            if (profile == null || !profile.AllDocumentsChecked || !profile.HasViolation) return null;

            var violations = new List<string>();
            int fine = 0;
            if (profile.LicenseStatus == DriverDocumentStatus.Suspended)
            { violations.Add("Driving while license suspended"); fine += 250; }
            else if (profile.LicenseStatus == DriverDocumentStatus.Expired)
            { violations.Add("Expired driver license"); fine += 100; }
            else if (profile.LicenseStatus == DriverDocumentStatus.NotFound)
            { violations.Add("Driver license not found"); fine += 150; }

            if (profile.RegistrationStatus == VehicleDocumentStatus.Expired)
            { violations.Add("Expired vehicle registration"); fine += 100; }
            else if (profile.RegistrationStatus == VehicleDocumentStatus.NotFound)
            { violations.Add("Vehicle registration not found"); fine += 150; }

            if (profile.InsuranceStatus == VehicleDocumentStatus.NotFound)
            { violations.Add("Proof of insurance not found"); fine += 150; }
            else if (profile.InsuranceStatus == VehicleDocumentStatus.Expired)
            { violations.Add("Expired insurance"); fine += 150; }

            if (violations.Count == 0) return null;
            var citation = new TrafficCitation {
                citationNumber = "MC-" + nextSequence.ToString("000000"),
                caseId = !string.IsNullOrWhiteSpace(caseId) ? caseId.Trim() : "CASE-MC-" + nextSequence.ToString("000000"),
                driverName = profile.DriverName,
                licenseNumber = profile.LicenseNumber,
                plateNumber = profile.PlateNumber,
                issuedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                location = location,
                violations = string.Join("; ", violations),
                fineAmount = fine,
                paymentStatus = CitationPaymentStatus.Outstanding,
                officerUnit = officerUnit
            };

            nextSequence++;
            citations.Add(citation);
            Save();
            CitationAdded?.Invoke(citation);
            Debug.Log("[TRAFFIC CITATION] " + citation.citationNumber + " | " + citation.driverName +
                " | $" + citation.fineAmount + " | " + citation.violations);
            return citation;
        }

        public bool SetPaymentStatus(string citationNumber, CitationPaymentStatus status)
        {
            TrafficCitation citation = citations.Find(item =>
                item != null && item.citationNumber == citationNumber);
            if (citation == null) return false;
            citation.paymentStatus = status;
            Save();
            CitationUpdated?.Invoke(citation);
            return true;
        }

        private void Load()
        {
            if (!File.Exists(SavePath)) return;

            try
            {
                string json = File.ReadAllText(SavePath);
                TrafficCitationSaveData data = JsonUtility.FromJson<TrafficCitationSaveData>(json);
                if (data == null || data.schemaVersion != CurrentSchemaVersion || data.citations == null)
                {
                    Debug.LogWarning("[TRAFFIC CITATIONS] Save data is empty or uses an unsupported version. Starting a new ledger.");
                    return;
                }

                citations.Clear();
                foreach (TrafficCitation citation in data.citations)
                    if (citation != null && !string.IsNullOrEmpty(citation.citationNumber))
                        citations.Add(citation);

                nextSequence = Mathf.Max(1, data.nextSequence);
                foreach (TrafficCitation citation in citations)
                {
                    string number = citation.citationNumber;
                    if (number.StartsWith("MC-", StringComparison.Ordinal) &&
                        int.TryParse(number.Substring(3), out int sequence))
                        nextSequence = Mathf.Max(nextSequence, sequence + 1);
                }

                Debug.Log("[TRAFFIC CITATIONS] Loaded " + citations.Count + " saved citation(s).");
            }
            catch (Exception exception)
            {
                citations.Clear();
                nextSequence = 1;
                Debug.LogWarning("[TRAFFIC CITATIONS] Could not load saved records; starting an empty in-memory ledger. " +
                    exception.Message);
            }
        }

        private void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                var data = new TrafficCitationSaveData {
                    schemaVersion = CurrentSchemaVersion,
                    nextSequence = nextSequence,
                    citations = new List<TrafficCitation>(citations)
                };
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogError("[TRAFFIC CITATIONS] Save failed. Current-session records remain available. " +
                    exception.Message);
            }
        }
    }
}