using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Context-action entry points for touch HUD buttons or an interaction wheel.
    /// The HUD can call these public methods after assigning the focused civilian.
    /// </summary>
    public sealed class OfficerInteractionController : MonoBehaviour
    {
        [SerializeField] private CivilianCharacter focusedCivilian;
        [SerializeField] private IncidentReportBook reportBook;
        private CountyDutyState dutyState;
        private ArrestBookingBook bookingBook;

        public CivilianCharacter FocusedCivilian => focusedCivilian;
        public event System.Action<CivilianCharacter> ArrestCompleted;

        public void Configure(IncidentReportBook reports) => reportBook = reports;
        public void ConfigureDutyState(CountyDutyState state) => dutyState = state;
        public void ConfigureBookingBook(ArrestBookingBook book) => bookingBook = book;

        public void SetFocusedCivilian(CivilianCharacter civilian) => focusedCivilian = civilian;

        public void Greet()
        {
            if (!HasTarget()) return;
            focusedCivilian.Greet();
            Record("Community contact", "Greet", "Contact made");
        }

        public void AskQuestions()
        {
            if (!HasTarget()) return;
            focusedCivilian.AskQuestions();
            Record("Field interview", "Questioning", "Information requested");
        }

        public void RequestIdentification()
        {
            if (!HasTarget() || !RequireOnDuty("Request identification")) return;
            focusedCivilian.ProvideIdentification();
            Record("Identity check", "Request identification", "Identification provided");
        }

        public void IssueWarning()
        {
            if (!HasTarget() || !RequireOnDuty("Issue a warning")) return;
            focusedCivilian.Warn();
            Record("Enforcement", "Verbal warning", "Warning issued");
        }

        public void IssueCitation()
        {
            if (!HasTarget() || !RequireOnDuty("Issue a citation")) return;
            focusedCivilian.Cite();
            Record("Traffic / ordinance", "Citation", "Citation issued");
        }

        public void Detain()
        {
            if (!HasTarget() || !RequireOnDuty("Detain a person")) return;
            bool success = focusedCivilian.Detain();
            Record("Investigation", "Detain", success ? "Subject detained" : "Action not permitted");
        }

        public void Arrest()
        {
            if (!HasTarget() || !RequireOnDuty("Make an arrest")) return;
            bool success = focusedCivilian.Arrest();
            if (success)
            {
                string reason = focusedCivilian.HasOutstandingWarrant
                    ? "Outstanding warrant"
                    : "Arrest following completed detention";
                string[] property = { "Wallet (secured)", "Mobile phone (secured)", "Key ring (secured)" };
                ArrestBookingRecord booking = bookingBook != null
                    ? bookingBook.CreateBooking(focusedCivilian, reason, property)
                    : null;
                Record("Arrest", "Arrest", booking != null
                    ? "Arrest completed; booking " + booking.bookingId + " created"
                    : "Arrest completed; booking ledger unavailable", booking != null ? booking.caseId : null);
                if (booking != null) ArrestCompleted?.Invoke(focusedCivilian);
            }
            else
            {
                Record("Arrest", "Arrest", "Subject must be detained first");
            }
        }

        public void Release()
        {
            if (!HasTarget() || !RequireOnDuty("Release a detained person")) return;
            focusedCivilian.Release();
            Record("Investigation", "Release", "Subject released if previously detained");
        }

        private bool HasTarget()
        {
            if (focusedCivilian != null) return true;
            Debug.Log("No civilian is currently selected.");
            return false;
        }

        private bool RequireOnDuty(string action)
        {
            if (dutyState != null && dutyState.IsOnDuty) return true;
            Debug.LogWarning(action + " is only available while on duty.");
            return false;
        }

        private void Record(string category, string action, string outcome, string caseId = null)
        {
            if (reportBook == null) return;
            reportBook.AddReport(category, focusedCivilian.FullName, action, outcome,
                "Deputy interaction recorded for " + focusedCivilian.FullName +
                ". Current disposition: " + focusedCivilian.Disposition +
                ". Legal status: " + focusedCivilian.LegalState + ".", null, caseId);
        }
    }
}