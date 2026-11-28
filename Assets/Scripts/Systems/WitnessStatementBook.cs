using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SheriffAmericanLife
{
    [Serializable]
    public sealed class WitnessStatement
    {
        public string statementId;
        public string incidentId;
        public string witnessName;
        public string incidentDescription;
        public string subjectDescription;
        public string observationType;
        [Range(0f, 1f)] public float confidence;
        public Vector3 witnessPosition;
        public string recordedAt;
    }

    /// <summary>Persistent local witness statement collection for fictional MDT/investigation records.</summary>
    public sealed class WitnessStatementBook : MonoBehaviour
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = 1;
            public int nextSequence = 1;
            public List<WitnessStatement> statements = new List<WitnessStatement>();
        }

        private const int SaveVersion = 1;
        private const int MaximumStatements = 2000;
        private readonly List<WitnessStatement> statements = new List<WitnessStatement>();
        private int nextSequence = 1;
        private string SavePath => Path.Combine(Application.persistentDataPath, "witness-statements.json");
        private string TemporarySavePath => SavePath + ".tmp";

        public IReadOnlyList<WitnessStatement> Statements => statements;
        public event Action<WitnessStatement> StatementAdded;

        private void Awake()
        {
            Load();
        }

        public void Add(WitnessStatement statement)
        {
            if (statement == null) return;
            statement.statementId = "WS-" + nextSequence.ToString("000000");
            statement.recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            statement.confidence = Mathf.Clamp01(statement.confidence);
            nextSequence++;
            statements.Insert(0, statement);
            if (statements.Count > MaximumStatements) statements.RemoveAt(statements.Count - 1);
            Save();
            StatementAdded?.Invoke(statement);
            Debug.Log("[WITNESS] " + statement.statementId + " | " +
                statement.witnessName + " | " + statement.observationType);
        }

        public List<WitnessStatement> FindForIncident(string incidentId)
        {
            return statements.FindAll(s => s != null && s.incidentId == incidentId);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (data == null || data.version != SaveVersion || data.statements == null)
                {
                    Debug.LogWarning("Witness statement save has an unsupported or invalid format; starting with an empty in-memory ledger.");
                    return;
                }

                statements.Clear();
                statements.AddRange(data.statements.FindAll(item => item != null));
                if (statements.Count > MaximumStatements)
                    statements.RemoveRange(MaximumStatements, statements.Count - MaximumStatements);

                nextSequence = Math.Max(1, data.nextSequence);
                foreach (WitnessStatement item in statements)
                {
                    if (!string.IsNullOrEmpty(item.statementId) &&
                        item.statementId.StartsWith("WS-", StringComparison.Ordinal))
                    {
                        int number;
                        if (int.TryParse(item.statementId.Substring(3), out number))
                            nextSequence = Math.Max(nextSequence, number + 1);
                    }
                }
            }
            catch (Exception exception)
            {
                statements.Clear();
                nextSequence = 1;
                Debug.LogWarning("Could not load local witness statements: " + exception.Message);
            }
        }

        private void Save()
        {
            try
            {
                var data = new SaveData { nextSequence = nextSequence, statements = statements };
                File.WriteAllText(TemporarySavePath, JsonUtility.ToJson(data, true));
                File.Copy(TemporarySavePath, SavePath, true);
                File.Delete(TemporarySavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not save local witness statements: " + exception.Message);
            }
        }
    }
}