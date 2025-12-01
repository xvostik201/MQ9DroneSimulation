using System.IO;
using UnityEngine;

public static class ProgressManager
{
    private const string FileName = "progress.json";

    public static ProgressData Data { get; private set; }

    static ProgressManager()
    {
        Load();
    }

    public static void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, FileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Data = JsonUtility.FromJson<ProgressData>(json);
        }
        else
        {
            Data = new ProgressData();
            Save();
        }
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        string path = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(path, json);
    }
}