using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum CivilianDisposition { Calm, Nervous, Agitated, Cooperative, Resistant }
    public enum CivilianLegalState { Free, Detained, Arrested }

    /// <summary>Runtime civilian identity, disposition, and interaction state.</summary>
    public sealed class CivilianCharacter : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string fullName = "Jordan Miller";
        [SerializeField] private string occupation = "Local resident";
        [SerializeField] private string identificationNumber = "FL-000001";

        [Header("Behaviour")]
        [SerializeField] private CivilianDisposition disposition = CivilianDisposition.Calm;
        [SerializeField] private CivilianLegalState legalState = CivilianLegalState.Free;
        [Range(0f, 100f)] [SerializeField] private float stress;
        [SerializeField] private bool hasOutstandingWarrant;

        public string FullName => fullName;
        public string Occupation => occupation;
        public string IdentificationNumber => identificationNumber;
        public CivilianDisposition Disposition => disposition;
        public CivilianLegalState LegalState => legalState;
        public float Stress => stress;
        public bool HasOutstandingWarrant => hasOutstandingWarrant;

        public event Action<string> Reaction;

        public void Greet()
        {
            stress = Mathf.Max(0f, stress - 5f);
            disposition = stress > 65f ? CivilianDisposition.Nervous : CivilianDisposition.Cooperative;
            Reaction?.Invoke(fullName + " acknowledges the deputy.");
        }

        public void AskQuestions()
        {
            stress = Mathf.Clamp(stress + 3f, 0f, 100f);
            Reaction?.Invoke(fullName + " says they are a " + occupation + ".");
        }

        public bool ProvideIdentification()
        {
            stress = Mathf.Clamp(stress + 2f, 0f, 100f);
            Reaction?.Invoke(fullName + " provides identification: " + identificationNumber + ".");
            return true;
        }

        public void Warn()
        {
            stress = Mathf.Clamp(stress + 5f, 0f, 100f);
            Reaction?.Invoke(fullName + " receives a verbal warning.");
        }

        public void Cite()
        {
            stress = Mathf.Clamp(stress + 12f, 0f, 100f);
            Reaction?.Invoke(fullName + " is issued a citation.");
        }

        public bool Detain()
        {
            if (legalState == CivilianLegalState.Arrested) return false;
            legalState = CivilianLegalState.Detained;
            stress = Mathf.Clamp(stress + 25f, 0f, 100f);
            disposition = CivilianDisposition.Nervous;
            Reaction?.Invoke(fullName + " is detained pending investigation.");
            return true;
        }

        public bool Arrest()
        {
            if (legalState != CivilianLegalState.Detained) return false;
            legalState = CivilianLegalState.Arrested;
            Reaction?.Invoke(fullName + " is placed under arrest.");
            return true;
        }

        public void Release()
        {
            if (legalState == CivilianLegalState.Detained)
            {
                legalState = CivilianLegalState.Free;
                stress = Mathf.Max(0f, stress - 15f);
                Reaction?.Invoke(fullName + " is released.");
            }
        }

        public void SetWarrantForTesting(bool value) => hasOutstandingWarrant = value;
    }
}