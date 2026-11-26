using UnityEngine;

namespace SheriffAmericanLife
{
    public enum TrafficDriverDisposition { Cooperative, Nervous, Argumentative, Fleeing }

    /// <summary>Fictional traffic-stop personality and dialogue state for civilian drivers.</summary>
    public sealed class TrafficStopDriverBehavior : MonoBehaviour
    {
        [SerializeField] private TrafficDriverDisposition disposition = TrafficDriverDisposition.Cooperative;
        private bool officerSpoke;
        private string latestResponse = "Waiting for officer contact.";

        public TrafficDriverDisposition Disposition => disposition;
        public string LatestResponse => latestResponse;
        public bool OfficerSpoke => officerSpoke;
        public float InitialEvasionSeconds => disposition == TrafficDriverDisposition.Fleeing ? 1.8f : 0f;

        public void Configure(TrafficDriverDisposition value)
        {
            disposition = value;
            officerSpoke = false;
            latestResponse = "Waiting for officer contact.";
        }

        public string RespondToOfficer(bool calmApproach)
        {
            officerSpoke = true;
            switch (disposition)
            {
                case TrafficDriverDisposition.Nervous:
                    latestResponse = calmApproach
                        ? "Driver appears nervous but begins cooperating."
                        : "Driver becomes more anxious and asks what is happening.";
                    break;
                case TrafficDriverDisposition.Argumentative:
                    latestResponse = calmApproach
                        ? "Driver complains about the stop but answers the officer."
                        : "Driver argues about the reason for the stop and delays answering.";
                    break;
                case TrafficDriverDisposition.Fleeing:
                    latestResponse = "Driver attempted to leave before stopping, then pulled over. Maintain awareness.";
                    break;
                default:
                    latestResponse = calmApproach
                        ? "Driver calmly agrees to provide documents."
                        : "Driver is confused but remains cooperative.";
                    break;
            }
            return latestResponse;
        }

        public void ResetStop()
        {
            officerSpoke = false;
            latestResponse = "Waiting for officer contact.";
        }
    }
}