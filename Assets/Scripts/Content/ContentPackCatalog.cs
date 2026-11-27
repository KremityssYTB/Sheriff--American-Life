using System;
using UnityEngine;

namespace SheriffAmericanLife
{
    public enum ContentPackCategory
    {
        VehicleCollection,
        CountyExpansion,
        Department,
        CharacterAndClothing,
        InteriorsAndBusinesses,
        MissionsAndActivities,
        AudioAndVisuals,
        GameplaySystems
    }

    [Serializable]
    public sealed class ContentPackEntry
    {
        [Tooltip("Permanent unique ID used by save data and the download manager.")]
        public string packId;
        public string displayName;
        [TextArea(2, 4)] public string description;
        public ContentPackCategory category;
        public string version = "1.0.0";
        [Min(0)] public long downloadSizeBytes;
        [Tooltip("HTTPS URL to a platform-compatible AssetBundle, supplied when CDN hosting is configured.")]
        public string bundleUrl;
        [Tooltip("Expected SHA-256 hex digest. Downloads must be verified before installation.")]
        public string sha256;
        public string[] dependencies = new string[0];
        public bool includedInBaseInstall;
        public bool removable = true;
    }

    [CreateAssetMenu(fileName = "ContentPackCatalog", menuName = "Sheriff American Life/Content Pack Catalog")]
    public sealed class ContentPackCatalog : ScriptableObject
    {
        public int manifestVersion = 1;
        public ContentPackEntry[] packs = new ContentPackEntry[0];

        public bool TryGetPack(string id, out ContentPackEntry result)
        {
            result = null;
            if (string.IsNullOrEmpty(id) || packs == null) return false;
            for (int i = 0; i < packs.Length; i++)
            {
                if (packs[i] != null && string.Equals(packs[i].packId, id, StringComparison.Ordinal))
                {
                    result = packs[i];
                    return true;
                }
            }
            return false;
        }

        public bool Validate(out string issue)
        {
            issue = null;
            if (packs == null) { issue = "Pack list is missing."; return false; }
            for (int i = 0; i < packs.Length; i++)
            {
                ContentPackEntry entry = packs[i];
                if (entry == null) { issue = "Pack entry " + i + " is null."; return false; }
                if (string.IsNullOrWhiteSpace(entry.packId))
                { issue = "Pack entry " + i + " has no permanent ID."; return false; }
                for (int j = i + 1; j < packs.Length; j++)
                    if (packs[j] != null && string.Equals(entry.packId, packs[j].packId, StringComparison.Ordinal))
                    { issue = "Duplicate pack ID: " + entry.packId; return false; }
                if (!entry.includedInBaseInstall && !entry.removable)
                { issue = "Optional pack must be removable: " + entry.packId; return false; }
                if (!string.IsNullOrEmpty(entry.bundleUrl) &&
                    !entry.bundleUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                { issue = "Remote bundle URLs must use HTTPS: " + entry.packId; return false; }
                if (!string.IsNullOrEmpty(entry.sha256) && entry.sha256.Length != 64)
                { issue = "SHA-256 digest must contain 64 hexadecimal characters: " + entry.packId; return false; }
            }
            return true;
        }
    }
}
