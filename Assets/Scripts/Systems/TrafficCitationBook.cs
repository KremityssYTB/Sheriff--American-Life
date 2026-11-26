using System;
using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum CitationPaymentStatus { Outstanding, Paid, Dismissed }

    [Serializable]
    public sealed class TrafficCitation
    {
        public string citationNumber;
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

    /// <summary>Session-only citation ledger for fictional traffic-stop gameplay.</summary>
    public sealed class TrafficCitationBook : MonoBehaviour
    {
        private readonly List<TrafficCitation> citations = new List<TrafficCitation>();
        public IReadOnlyList<TrafficCitation> Citations => citations;
        public event Action<TrafficCitation> CitationAdded;
        public event Action<TrafficCitation> CitationUpdated;

        public TrafficCitation Issue(TrafficStopDriverProfile profile, string location, string officerUnit)
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
                citationNumber = "MC-" + (citations.Count + 1).ToString("000000"),
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
            citations.Add(citation);
            CitationAdded?.Invoke(citation);
            Debug.Log("[TRAFFIC CITATION] " + citation.citationNumber + " | " + citation.driverName +
                " | $" + citation.fineAmount + " | " + citation.violations);
            return citation;
        }

        public bool SetPaymentStatus(string citationNumber, CitationPaymentStatus status)
        {
            TrafficCitation citation = citations.Find(item => item.citationNumber == citationNumber);
            if (citation == null) return false;
            citation.paymentStatus = status;
            CitationUpdated?.Invoke(citation);
            return true;
        }
    }
}