using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Binary-backed key/value store for persistent player data.
/// Supports multiple primitive value types for general player progress storage.
/// </summary>
public static class PlayerDataStore
{
    private const string FileName = "playerdata.bin";
    private const int CurrentVersion = 1;

    private static readonly Dictionary<string, int> IntValues = new Dictionary<string, int>();
    private static readonly Dictionary<string, float> FloatValues = new Dictionary<string, float>();
    private static readonly Dictionary<string, string> StringValues = new Dictionary<string, string>();
    private static readonly Dictionary<string, bool> BoolValues = new Dictionary<string, bool>();

    private static bool isLoaded;

    private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static int GetInt(string key, int defaultValue = 0)
    {
        EnsureLoaded();
        return IntValues.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public static void SetInt(string key, int value)
    {
        EnsureLoaded();
        IntValues[key] = value;
        Save();
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        EnsureLoaded();
        return FloatValues.TryGetValue(key, out float value) ? value : defaultValue;
    }

    public static void SetFloat(string key, float value)
    {
        EnsureLoaded();
        FloatValues[key] = value;
        Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        EnsureLoaded();
        return StringValues.TryGetValue(key, out string value) ? value : defaultValue;
    }

    public static void SetString(string key, string value)
    {
        EnsureLoaded();
        StringValues[key] = value ?? string.Empty;
        Save();
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        EnsureLoaded();
        return BoolValues.TryGetValue(key, out bool value) ? value : defaultValue;
    }

    public static void SetBool(string key, bool value)
    {
        EnsureLoaded();
        BoolValues[key] = value;
        Save();
    }

    public static bool HasKey(string key)
    {
        EnsureLoaded();
        return IntValues.ContainsKey(key)
            || FloatValues.ContainsKey(key)
            || StringValues.ContainsKey(key)
            || BoolValues.ContainsKey(key);
    }

    public static void RemoveKey(string key)
    {
        EnsureLoaded();
        bool removed = IntValues.Remove(key);
        removed = FloatValues.Remove(key) || removed;
        removed = StringValues.Remove(key) || removed;
        removed = BoolValues.Remove(key) || removed;

        if (removed)
        {
            Save();
        }
    }

    public static void Save()
    {
        EnsureLoaded();

        try
        {
            string path = SavePath;
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(CurrentVersion);

                WriteIntDictionary(writer, IntValues);
                WriteFloatDictionary(writer, FloatValues);
                WriteStringDictionary(writer, StringValues);
                WriteBoolDictionary(writer, BoolValues);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlayerDataStore] Failed to save data: {ex.Message}");
        }
    }

    public static void Reload()
    {
        isLoaded = false;
        EnsureLoaded();
    }

    private static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        IntValues.Clear();
        FloatValues.Clear();
        StringValues.Clear();
        BoolValues.Clear();

        string path = SavePath;
        if (!File.Exists(path))
        {
            isLoaded = true;
            return;
        }

        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                int version = reader.ReadInt32();
                if (version > CurrentVersion)
                {
                    throw new InvalidDataException($"Unsupported save version: {version}");
                }

                ReadIntDictionary(reader, IntValues);
                ReadFloatDictionary(reader, FloatValues);
                ReadStringDictionary(reader, StringValues);
                ReadBoolDictionary(reader, BoolValues);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[PlayerDataStore] Failed to load data, using defaults: {ex.Message}");
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();
            BoolValues.Clear();
        }

        isLoaded = true;
    }

    private static void WriteIntDictionary(BinaryWriter writer, Dictionary<string, int> values)
    {
        writer.Write(values.Count);
        foreach (KeyValuePair<string, int> pair in values)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    private static void ReadIntDictionary(BinaryReader reader, Dictionary<string, int> values)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            int value = reader.ReadInt32();
            values[key] = value;
        }
    }

    private static void WriteFloatDictionary(BinaryWriter writer, Dictionary<string, float> values)
    {
        writer.Write(values.Count);
        foreach (KeyValuePair<string, float> pair in values)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    private static void ReadFloatDictionary(BinaryReader reader, Dictionary<string, float> values)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            float value = reader.ReadSingle();
            values[key] = value;
        }
    }

    private static void WriteStringDictionary(BinaryWriter writer, Dictionary<string, string> values)
    {
        writer.Write(values.Count);
        foreach (KeyValuePair<string, string> pair in values)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value ?? string.Empty);
        }
    }

    private static void ReadStringDictionary(BinaryReader reader, Dictionary<string, string> values)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            string value = reader.ReadString();
            values[key] = value;
        }
    }

    private static void WriteBoolDictionary(BinaryWriter writer, Dictionary<string, bool> values)
    {
        writer.Write(values.Count);
        foreach (KeyValuePair<string, bool> pair in values)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }
    }

    private static void ReadBoolDictionary(BinaryReader reader, Dictionary<string, bool> values)
    {
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = reader.ReadString();
            bool value = reader.ReadBoolean();
            values[key] = value;
        }
    }
}