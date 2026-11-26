using System;
using System.Collections.Generic;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Data/controller layer for the future touch-screen MDT. Keeps UI presentation
    /// separate from dispatch, report, and witness data systems.
    /// </summary>
    public sealed class MobileMdtController : MonoBehaviour
    {
        private DispatchCallManager dispatch;
        private IncidentReportBook reports;
        private WitnessStatementBook witnesses;
        private TrafficCitationBook citations;
        private string selectedCallId;

        public DispatchCall SelectedCall => dispatch != null ? dispatch.Find(selectedCallId) : null;
        public IReadOnlyList<DispatchCall> Calls => dispatch != null ? dispatch.Calls : Array.Empty<DispatchCall>();
        public IReadOnlyList<TrafficCitation> Citations => citations != null ? citations.Citations : Array.Empty<TrafficCitation>();
        public event Action MdtChanged;

        public void Configure(DispatchCallManager dispatchManager, IncidentReportBook reportBook,
            WitnessStatementBook witnessBook, TrafficCitationBook citationBook)
        {
            Unsubscribe();
            dispatch = dispatchManager;
            reports = reportBook;
            witnesses = witnessBook;
            citations = citationBook;
            Subscribe();
            MdtChanged?.Invoke();
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (dispatch != null)
            {
                dispatch.CallCreated -= HandleCallChanged;
                dispatch.CallUpdated -= HandleCallChanged;
                dispatch.CallCreated += HandleCallChanged;
                dispatch.CallUpdated += HandleCallChanged;
            }
            if (reports != null)
            {
                reports.ReportAdded -= HandleReportAdded;
                reports.ReportAdded += HandleReportAdded;
            }
            if (witnesses != null)
            {
                witnesses.StatementAdded -= HandleWitnessAdded;
                witnesses.StatementAdded += HandleWitnessAdded;
            }
            if (citations != null)
            {
                citations.CitationAdded -= HandleCitationChanged;
                citations.CitationUpdated -= HandleCitationChanged;
                citations.CitationAdded += HandleCitationChanged;
                citations.CitationUpdated += HandleCitationChanged;
            }
        }

        private void Unsubscribe()
        {
            if (dispatch != null)
            {
                dispatch.CallCreated -= HandleCallChanged;
                dispatch.CallUpdated -= HandleCallChanged;
            }
            if (reports != null) reports.ReportAdded -= HandleReportAdded;
            if (witnesses != null) witnesses.StatementAdded -= HandleWitnessAdded;
            if (citations != null)
            {
                citations.CitationAdded -= HandleCitationChanged;
                citations.CitationUpdated -= HandleCitationChanged;
            }
        }

        public bool SelectCall(string callId)
        {
            if (dispatch == null || dispatch.Find(callId) == null) return false;
            selectedCallId = callId;
            MdtChanged?.Invoke();
            return true;
        }

        public void ClearSelection()
        {
            selectedCallId = null;
            MdtChanged?.Invoke();
        }

        public List<WitnessStatement> GetSelectedWitnessStatements()
        {
            DispatchCall call = SelectedCall;
            return call != null && witnesses != null
                ? witnesses.FindForIncident(call.sourceIncidentId)
                : new List<WitnessStatement>();
        }

        public List<IncidentReport> GetSelectedReports()
        {
            DispatchCall call = SelectedCall;
            if (call == null || reports == null) return new List<IncidentReport>();
            var linked = new List<IncidentReport>();
            foreach (IncidentReport report in reports.Reports)
                if (report.relatedIncidentId == call.sourceIncidentId) linked.Add(report);
            return linked;
        }

        public bool AcceptSelectedCall(string unitName)
        {
            DispatchCall call = SelectedCall;
            return call != null && dispatch.Accept(call.callId, unitName);
        }

        public bool MarkSelectedEnRoute()
        {
            DispatchCall call = SelectedCall;
            return call != null && dispatch.MarkEnRoute(call.callId);
        }

        public bool MarkSelectedOnScene()
        {
            DispatchCall call = SelectedCall;
            return call != null && dispatch.MarkOnScene(call.callId);
        }

        public bool ResolveSelectedCall(string disposition)
        {
            DispatchCall call = SelectedCall;
            return call != null && dispatch.Resolve(call.callId, disposition);
        }

        public bool CancelSelectedCall(string reason)
        {
            DispatchCall call = SelectedCall;
            return call != null && dispatch.Cancel(call.callId, reason);
        }

        private void HandleCallChanged(DispatchCall call) => MdtChanged?.Invoke();
        private void HandleReportAdded(IncidentReport report) => MdtChanged?.Invoke();
        private void HandleWitnessAdded(WitnessStatement statement) => MdtChanged?.Invoke();
        private void HandleCitationChanged(TrafficCitation citation) => MdtChanged?.Invoke();
    }
}
