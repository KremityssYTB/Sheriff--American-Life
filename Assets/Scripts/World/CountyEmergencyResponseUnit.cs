using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Connects an emergency test vehicle to the MDT dispatch lifecycle.
    /// The unit responds only to calls explicitly assigned to its unit name.
    /// </summary>
    public sealed class CountyEmergencyResponseUnit : MonoBehaviour
    {
        [SerializeField] private string unitName = "OCALA-1";
        [SerializeField] private DispatchCallManager dispatch;
        [SerializeField] private CountyEmergencyLightbar lightbar;
        [SerializeField] private CountyEmergencySiren siren;

        public string UnitName => unitName;

        public void Configure(string assignedUnitName, CountyEmergencyLightbar vehicleLightbar,
            CountyEmergencySiren vehicleSiren)
        {
            unitName = string.IsNullOrWhiteSpace(assignedUnitName) ? "OCALA-1" : assignedUnitName;
            lightbar = vehicleLightbar;
            siren = vehicleSiren;
        }

        private void Start()
        {
            if (dispatch == null)
                dispatch = FindFirstObjectByType<DispatchCallManager>();

            if (dispatch != null)
            {
                dispatch.CallUpdated -= HandleCallUpdated;
                dispatch.CallUpdated += HandleCallUpdated;
            }

            SetResponseState(false, false);
        }

        private void OnDestroy()
        {
            if (dispatch != null) dispatch.CallUpdated -= HandleCallUpdated;
            SetResponseState(false, false);
        }

        private void HandleCallUpdated(DispatchCall call)
        {
            if (call == null || call.assignedUnit != unitName) return;

            switch (call.status)
            {
                case DispatchCallStatus.EnRoute:
                    SetResponseState(true, true);
                    break;
                case DispatchCallStatus.OnScene:
                    SetResponseState(true, false);
                    break;
                case DispatchCallStatus.Resolved:
                case DispatchCallStatus.Cancelled:
                    SetResponseState(false, false);
                    break;
            }
        }

        private void SetResponseState(bool lightsEnabled, bool sirenEnabled)
        {
            if (lightbar != null) lightbar.SetLightbarEnabled(lightsEnabled);
            if (siren != null) siren.SetSirenEnabled(sirenEnabled);
        }
    }
}