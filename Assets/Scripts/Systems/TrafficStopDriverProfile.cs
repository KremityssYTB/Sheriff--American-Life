using UnityEngine;

namespace SheriffAmericanLife
{
    public enum DriverDocumentStatus { Valid, Suspended, Expired, NotFound }
    public enum VehicleDocumentStatus { Valid, Expired, NotFound }

    /// <summary>Lightweight fictional driver and vehicle records for traffic-stop gameplay.</summary>
    public sealed class TrafficStopDriverProfile : MonoBehaviour
    {
        [SerializeField] private string driverName = "Jordan Miller";
        [SerializeField] private string dateOfBirth = "1994-06-18";
        [SerializeField] private string licenseNumber = "FL-000000";
        [SerializeField] private DriverDocumentStatus licenseStatus = DriverDocumentStatus.Valid;
        [SerializeField] private string plateNumber = "ABC123";
        [SerializeField] private string registeredOwner = "Jordan Miller";
        [SerializeField] private VehicleDocumentStatus registrationStatus = VehicleDocumentStatus.Valid;
        [SerializeField] private VehicleDocumentStatus insuranceStatus = VehicleDocumentStatus.Valid;

        private bool licenseChecked;
        private bool registrationChecked;
        private bool insuranceChecked;

        public string DriverName => driverName;
        public string DateOfBirth => dateOfBirth;
        public string LicenseNumber => licenseNumber;
        public DriverDocumentStatus LicenseStatus => licenseStatus;
        public string PlateNumber => plateNumber;
        public string RegisteredOwner => registeredOwner;
        public VehicleDocumentStatus RegistrationStatus => registrationStatus;
        public VehicleDocumentStatus InsuranceStatus => insuranceStatus;
        public bool LicenseChecked => licenseChecked;
        public bool RegistrationChecked => registrationChecked;
        public bool InsuranceChecked => insuranceChecked;
        public bool AllDocumentsChecked => licenseChecked && registrationChecked && insuranceChecked;
        public bool HasViolation =>
            licenseStatus != DriverDocumentStatus.Valid ||
            registrationStatus != VehicleDocumentStatus.Valid ||
            insuranceStatus != VehicleDocumentStatus.Valid;

        public void Configure(string name, string dob, string license, DriverDocumentStatus licenseState,
            string plate, string owner, VehicleDocumentStatus registration, VehicleDocumentStatus insurance)
        {
            driverName = name;
            dateOfBirth = dob;
            licenseNumber = license;
            licenseStatus = licenseState;
            plateNumber = plate;
            registeredOwner = owner;
            registrationStatus = registration;
            insuranceStatus = insurance;
        }

        public string CheckLicense()
        {
            licenseChecked = true;
            return "Driver license: " + licenseStatus + " (" + licenseNumber + ").";
        }

        public string CheckRegistration()
        {
            registrationChecked = true;
            return "Registration: " + registrationStatus + " for plate " + plateNumber +
                "; registered owner: " + registeredOwner + ".";
        }

        public string CheckInsurance()
        {
            insuranceChecked = true;
            return "Insurance: " + insuranceStatus + ".";
        }

        public void ResetChecks()
        {
            licenseChecked = false;
            registrationChecked = false;
            insuranceChecked = false;
        }
    }
}