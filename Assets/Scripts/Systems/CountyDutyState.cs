using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>
    /// Session-level duty state. Later systems subscribe to OnDutyChanged to gate
    /// dispatch, uniform/equipment, patrol assignment and duty-only interactions.
    /// </summary>
    public sealed class CountyDutyState : MonoBehaviour
    {
        public bool IsOnDuty { get; private set; } = true;
        public event Action<bool> OnDutyChanged;

        public void SetDuty(bool value)
        {
            if (IsOnDuty == value) return;
            IsOnDuty = value;
            OnDutyChanged?.Invoke(IsOnDuty);
            Debug.Log(IsOnDuty ? "DUTY: On duty" : "DUTY: Off duty");
        }

        public void ToggleDuty() => SetDuty(!IsOnDuty);
    }
}