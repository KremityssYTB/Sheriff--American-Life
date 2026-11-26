using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum BookingStatus { AwaitingTransport, Booked, Released }

    [Serializable]
    public sealed class ArrestBookingRecord
    {
        public string bookingId;
        public string personName;
        public string identificationNumber;
        public string arrestReason;
        public string arrestedAt;
        public string bookedAt;
        public string releasedAt;
        public BookingStatus status;
        public List<string> propertyInventory = new List<string>();
    }

    /// <summary>Local fictional arrest/booking ledger. No real-world justice records are used.</summary>
    public sealed class ArrestBookingBook : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int nextSequence = 1;
            public List<ArrestBookingRecord> records = new List<ArrestBookingRecord>();
        }

        private const int SaveVersion = 1;
        private const int MaximumRecords = 500;
        private readonly List<ArrestBookingRecord> records = new List<ArrestBookingRecord>();
        private int nextSequence = 1;
        private string SavePath => Path.Combine(Application.persistentDataPath, "arrest-bookings.json");

        public IReadOnlyList<ArrestBookingRecord> Records => records;
        public event Action BookingsChanged;

        private void Awake() => Load();

        public ArrestBookingRecord CreateBooking(CivilianCharacter person, string reason, IList<string> property)
        {
            if (person == null || string.IsNullOrWhiteSpace(reason)) return null;
            var record = new ArrestBookingRecord {
                bookingId = "MCB-" + nextSequence.ToString("000000"),
                personName = person.FullName,
                identificationNumber = person.IdentificationNumber,
                arrestReason = reason,
                arrestedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                status = BookingStatus.AwaitingTransport,
                propertyInventory = property != null ? new List<string>(property) : new List<string>()
            };
            nextSequence++;
            records.Insert(0, record);
            if (records.Count > MaximumRecords) records.RemoveAt(records.Count - 1);
            Save();
            BookingsChanged?.Invoke();
            return record;
        }

        public bool MarkBooked(string bookingId)
        {
            ArrestBookingRecord record = Find(bookingId);
            if (record == null || record.status != BookingStatus.AwaitingTransport) return false;
            record.status = BookingStatus.Booked;
            record.bookedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Save();
            BookingsChanged?.Invoke();
            return true;
        }

        public bool MarkReleased(string bookingId)
        {
            ArrestBookingRecord record = Find(bookingId);
            if (record == null || record.status != BookingStatus.Booked) return false;
            record.status = BookingStatus.Released;
            record.releasedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Save();
            BookingsChanged?.Invoke();
            return true;
        }

        private ArrestBookingRecord Find(string bookingId)
        {
            if (string.IsNullOrEmpty(bookingId)) return null;
            return records.Find(item => item != null && item.bookingId == bookingId);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (data == null || data.version != SaveVersion || data.records == null) return;
                records.Clear();
                records.AddRange(data.records.FindAll(item => item != null));
                nextSequence = Math.Max(1, data.nextSequence);
                foreach (ArrestBookingRecord item in records)
                {
                    if (item.propertyInventory == null) item.propertyInventory = new List<string>();
                    if (!string.IsNullOrEmpty(item.bookingId) &&
                        item.bookingId.StartsWith("MCB-", StringComparison.Ordinal))
                    {
                        int number;
                        if (int.TryParse(item.bookingId.Substring(4), out number))
                            nextSequence = Math.Max(nextSequence, number + 1);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not load local arrest bookings: " + exception.Message);
                records.Clear();
                nextSequence = 1;
            }
        }

        private void Save()
        {
            try
            {
                var data = new SaveData { nextSequence = nextSequence, records = records };
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not save local arrest bookings: " + exception.Message);
            }
        }
    }
}