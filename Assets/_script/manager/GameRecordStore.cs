using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRecord
{
    public int distance;
    public int kills;
    public string date;
    public float playTimeSeconds;
}

[Serializable]
public class GameRecordCollection
{
    public List<GameRecord> records = new List<GameRecord>();
}

public static class GameRecordStore
{
    private const string RecordsKey = "GameHistoryRecords";

    public static List<GameRecord> LoadRecords()
    {
        string json = PlayerPrefs.GetString(RecordsKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new List<GameRecord>();
        }

        GameRecordCollection collection = JsonUtility.FromJson<GameRecordCollection>(json);
        if (collection == null || collection.records == null)
        {
            return new List<GameRecord>();
        }

        SortRecords(collection.records);
        return collection.records;
    }

    public static void AddRecord(int distance, int kills, float playTimeSeconds)
    {
        List<GameRecord> records = LoadRecords();
        records.Add(new GameRecord
        {
            distance = Mathf.Max(0, distance),
            kills = Mathf.Max(0, kills),
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            playTimeSeconds = Mathf.Max(0f, playTimeSeconds)
        });

        SaveRecords(records);
    }

    public static void ClearRecords()
    {
        PlayerPrefs.DeleteKey(RecordsKey);
        PlayerPrefs.Save();
    }

    public static void SortRecords(List<GameRecord> records)
    {
        records.Sort((a, b) =>
        {
            int distanceCompare = b.distance.CompareTo(a.distance);
            if (distanceCompare != 0) return distanceCompare;

            int killsCompare = b.kills.CompareTo(a.kills);
            if (killsCompare != 0) return killsCompare;

            return b.playTimeSeconds.CompareTo(a.playTimeSeconds);
        });
    }

    private static void SaveRecords(List<GameRecord> records)
    {
        SortRecords(records);
        GameRecordCollection collection = new GameRecordCollection { records = records };
        PlayerPrefs.SetString(RecordsKey, JsonUtility.ToJson(collection));
        PlayerPrefs.Save();
    }
}
