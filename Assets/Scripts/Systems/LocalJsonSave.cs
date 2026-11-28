using System;
using System.IO;
using UnityEngine;

namespace SheriffAmericanLife
{
    /// <summary>Shared local JSON save helpers for fictional gameplay ledgers.</summary>
    public static class LocalJsonSave
    {
        public static bool TryLoad<T>(string fileName, out T data) where T : class
        {
            data = null;
            string path = GetPath(fileName);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;

            try
            {
                data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                if (data != null) return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not read local save " + fileName + ": " + exception.Message);
            }

            PreserveInvalidFile(path, fileName);
            data = null;
            return false;
        }

        public static bool TrySave(string fileName, object data)
        {
            string path = GetPath(fileName);
            if (string.IsNullOrEmpty(path) || data == null) return false;
            string temporaryPath = path + ".tmp";

            try
            {
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
                File.Copy(temporaryPath, path, true);
                File.Delete(temporaryPath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not write local save " + fileName + ": " + exception.Message);
                try
                {
                    if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
                }
                catch (Exception cleanupException)
                {
                    Debug.LogWarning("Could not clean temporary save " + fileName + ": " + cleanupException.Message);
                }
                return false;
            }
        }

        private static string GetPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            {
                Debug.LogWarning("Rejected invalid local save filename.");
                return null;
            }
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        private static void PreserveInvalidFile(string path, string fileName)
        {
            try
            {
                string backupPath = path + ".invalid-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                File.Copy(path, backupPath, false);
                Debug.LogWarning("Preserved an unreadable local save copy for inspection: " + fileName);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not preserve unreadable local save " + fileName + ": " + exception.Message);
            }
        }
    }
}