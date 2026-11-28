using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SheriffAmericanLife
{
    [Serializable]
    public sealed class DriverHistoryEvent
    {
        public string recordedAt;
        public string action;
        public string details;
        public string citationNumber;
    }

    [Serializable]
    public sealed class DriverHistoryRecord
    {
        public string driverName;
        public string licenseNumber;
        public string plateNumber;
        public int totalStops;
        public int warningCount;
        public int citationCount;
        public string lastEncounterAt;
        public string lastOutcome;
        public List<DriverHistoryEvent> events = new List<DriverHistoryEvent>();
    }

    [Serializable]
    internal sealed class DriverHistorySaveData
    {
        public int schemaVersion = 1;
        public List<DriverHistoryRecord> records = new List<DriverHistoryRecord>();
    }

    /// <summary>Local fictional repeat-driver history for the in-game MDT.</summary>
    public sealed class TrafficDriverHistoryBook : MonoBehaviour
    {
        private const int CurrentSchemaVersion = 1;
        private const int MaxRecords = 500;
        private const int MaxEventsPerDriver = 20;
        private const string SaveFileName = "traffic-driver-history.json";

        private readonly List<DriverHistoryRecord> records = new List<DriverHistoryRecord>();
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public IReadOnlyList<DriverHistoryRecord> Records => records;
        public event Action HistoryChanged;

        private void Awake() { Load(); }

        public DriverHistoryRecord FindByLicenseOrPlate(string licenseNumber, string plateNumber)
        {
            string license = Normalize(licenseNumber);
            string plate = Normalize(plateNumber);

            if (!string.IsNullOrEmpty(license))
            {
                DriverHistoryRecord licenseMatch = records.Find(record =>
                    record != null && Normalize(record.licenseNumber) == license);
                if (licenseMatch != null)
                    return string.IsNullOrEmpty(plate) || string.IsNullOrEmpty(Normalize(licenseMatch.plateNumber)) ||
                        Normalize(licenseMatch.plateNumber) == plate ? licenseMatch : null;
            }

            if (!string.IsNullOrEmpty(plate))
            {
                DriverHistoryRecord plateMatch = records.Find(record =>
                    record != null && Normalize(record.plateNumber) == plate);
                if (plateMatch != null)
                    return string.IsNullOrEmpty(license) || string.IsNullOrEmpty(Normalize(plateMatch.licenseNumber)) ||
                        Normalize(plateMatch.licenseNumber) == license ? plateMatch : null;
            }

            return null;
        }

        public DriverHistoryRecord RecordWarning(TrafficStopDriverProfile profile, string details)
        {
            if (profile == null) return null;
            DriverHistoryRecord record = GetOrCreate(profile);
            AddEvent(record, "WARNING", details, null);
            record.warningCount++;
            record.totalStops++;
            record.lastOutcome = "Verbal warning";
            Commit();
            return record;
        }

        public DriverHistoryRecord RecordCitation(TrafficStopDriverProfile profile, TrafficCitation citation)
        {
            if (profile == null || citation == null) return null;
            DriverHistoryRecord record = GetOrCreate(profile);
            AddEvent(record, "CITATION", citation.violations + " | $" + citation.fineAmount,
                citation.citationNumber);
            record.citationCount++;
            record.totalStops++;
            record.lastOutcome = "Citation " + citation.citationNumber;
            Commit();
            return record;
        }

        private DriverHistoryRecord GetOrCreate(TrafficStopDriverProfile profile)
        {
            DriverHistoryRecord record = FindByLicenseOrPlate(profile.LicenseNumber, profile.PlateNumber);
            if (record == null)
            {
                record = new DriverHistoryRecord();
                records.Add(record);
            }

            record.driverName = profile.DriverName;
            record.licenseNumber = profile.LicenseNumber;
            record.plateNumber = profile.PlateNumber;
            record.lastEncounterAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            while (records.Count > MaxRecords) records.RemoveAt(0);
            return record;
        }

        private static void AddEvent(DriverHistoryRecord record, string action, string details, string citationNumber)
        {
            record.events ??= new List<DriverHistoryEvent>();
            record.events.Add(new DriverHistoryEvent {
                recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                action = action,
                details = details,
                citationNumber = citationNumber
            });
            while (record.events.Count > MaxEventsPerDriver) record.events.RemoveAt(0);
        }

        private void Commit()
        {
            Save();
            HistoryChanged?.Invoke();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
        }

        private void Load()
        {
            if (!File.Exists(SavePath)) return;
            try
            {
                DriverHistorySaveData data = JsonUtility.FromJson<DriverHistorySaveData>(File.ReadAllText(SavePath));
                if (data == null || data.schemaVersion != CurrentSchemaVersion || data.records == null)
                {
                    Debug.LogWarning("[DRIVER HISTORY] Unsupported or empty save. Starting a new history book.");
                    return;
                }

                records.Clear();
                foreach (DriverHistoryRecord record in data.records)
                    if (record != null && (!string.IsNullOrEmpty(record.licenseNumber) ||
                        !string.IsNullOrEmpty(record.plateNumber)))
                    {
                        if (record.events == null) record.events = new List<DriverHistoryEvent>();
                        records.Add(record);
                    }
                while (records.Count > MaxRecords) records.RemoveAt(0);
                Debug.Log("[DRIVER HISTORY] Loaded " + records.Count + " driver record(s).");
            }
            catch (Exception exception)
            {
                records.Clear();
                Debug.LogWarning("[DRIVER HISTORY] Could not load saved records; starting empty. " + exception.Message);
            }
        }

        private void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                var data = new DriverHistorySaveData {
                    schemaVersion = CurrentSchemaVersion,
                    records = new List<DriverHistoryRecord>(records)
                };
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception)
            {
                Debug.LogError("[DRIVER HISTORY] Save failed; current-session history remains available. " + exception.Message);
            }
        }
    }
}