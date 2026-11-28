using System;
using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    [Serializable]
    public sealed class CaseFileSnapshot
    {
        public string caseId;
        public List<IncidentReport> reports = new List<IncidentReport>();
        public List<WitnessStatement> witnessStatements = new List<WitnessStatement>();
        public List<DispatchCall> dispatchCalls = new List<DispatchCall>();
        public List<TrafficCitation> citations = new List<TrafficCitation>();
        public List<ArrestBookingRecord> bookings = new List<ArrestBookingRecord>();

        public int TotalRecords =>
            reports.Count + witnessStatements.Count + dispatchCalls.Count +
            citations.Count + bookings.Count;
    }

    /// <summary>Read-only cross-ledger case lookup. It does not change record authority or legal status.</summary>
    public sealed class CaseFileIndex : MonoBehaviour
    {
        private IncidentReportBook reports;
        private WitnessStatementBook witnesses;
        private DispatchCallManager dispatch;
        private TrafficCitationBook citations;
        private ArrestBookingBook bookings;

        private void Awake()
        {
            Configure(
                GetComponent<IncidentReportBook>(),
                GetComponent<WitnessStatementBook>(),
                GetComponent<DispatchCallManager>(),
                GetComponent<TrafficCitationBook>(),
                GetComponent<ArrestBookingBook>());
        }

        public void Configure(
            IncidentReportBook reportBook,
            WitnessStatementBook witnessBook,
            DispatchCallManager dispatchBook,
            TrafficCitationBook citationBook,
            ArrestBookingBook bookingBook)
        {
            reports = reportBook;
            witnesses = witnessBook;
            dispatch = dispatchBook;
            citations = citationBook;
            bookings = bookingBook;
        }

        public List<string> GetKnownCaseIds(string query = null)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (reports != null)
                foreach (IncidentReport item in reports.Reports)
                    AddId(ids, item != null ? item.caseId : null);
            if (witnesses != null)
                foreach (WitnessStatement item in witnesses.Statements)
                    AddId(ids, item != null ? item.caseId : null);
            if (dispatch != null)
                foreach (DispatchCall item in dispatch.Calls)
                    AddId(ids, item != null ? item.caseId : null);
            if (citations != null)
                foreach (TrafficCitation item in citations.Citations)
                    AddId(ids, item != null ? item.caseId : null);
            if (bookings != null)
                foreach (ArrestBookingRecord item in bookings.Records)
                    AddId(ids, item != null ? item.caseId : null);

            var results = new List<string>(ids);
            results.Sort(StringComparer.Ordinal);
            if (!string.IsNullOrWhiteSpace(query))
                results.RemoveAll(id => id.IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) < 0);
            return results;
        }

        private static void AddId(HashSet<string> ids, string caseId)
        {
            if (!string.IsNullOrWhiteSpace(caseId)) ids.Add(caseId.Trim());
        }

        public CaseFileSnapshot FindCase(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId)) return null;
            string key = caseId.Trim();
            var snapshot = new CaseFileSnapshot { caseId = key };

            if (reports != null)
                foreach (IncidentReport item in reports.Reports)
                    if (item != null && Matches(item.caseId, item.relatedIncidentId, key))
                        snapshot.reports.Add(item);

            if (witnesses != null)
                foreach (WitnessStatement item in witnesses.Statements)
                    if (item != null && Matches(item.caseId, item.incidentId, key))
                        snapshot.witnessStatements.Add(item);

            if (dispatch != null)
                foreach (DispatchCall item in dispatch.Calls)
                    if (item != null && Matches(item.caseId, item.sourceIncidentId, key))
                        snapshot.dispatchCalls.Add(item);

            if (citations != null)
                foreach (TrafficCitation item in citations.Citations)
                    if (item != null && Matches(item.caseId, null, key))
                        snapshot.citations.Add(item);

            if (bookings != null)
                foreach (ArrestBookingRecord item in bookings.Records)
                    if (item != null && Matches(item.caseId, null, key))
                        snapshot.bookings.Add(item);

            return snapshot.TotalRecords > 0 ? snapshot : null;
        }

        private static bool Matches(string primary, string legacy, string key)
        {
            return string.Equals(primary, key, StringComparison.Ordinal) ||
                string.Equals(legacy, key, StringComparison.Ordinal);
        }
    }
}