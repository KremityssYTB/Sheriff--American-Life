using System;
using System.Collections.Generic;
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

    /// <summary>Session-only witness statement collection for the MDT/investigation system.</summary>
    public sealed class WitnessStatementBook : MonoBehaviour
    {
        private readonly List<WitnessStatement> statements = new List<WitnessStatement>();
        public IReadOnlyList<WitnessStatement> Statements => statements;
        public event Action<WitnessStatement> StatementAdded;

        public void Add(WitnessStatement statement)
        {
            if (statement == null) return;
            statement.statementId = "WS-" + (statements.Count + 1).ToString("0000");
            statement.recordedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            statements.Add(statement);
            StatementAdded?.Invoke(statement);
            Debug.Log("[WITNESS] " + statement.statementId + " | " +
                statement.witnessName + " | " + statement.observationType);
        }

        public List<WitnessStatement> FindForIncident(string incidentId)
        {
            return statements.FindAll(s => s.incidentId == incidentId);
        }
    }
}