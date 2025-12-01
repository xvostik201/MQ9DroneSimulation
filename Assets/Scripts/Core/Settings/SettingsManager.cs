using UnityEngine;
using System.IO;

public static class SettingsManager
{
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

    public static SettingsData Data { get; private set; }
    private static GameSettingsConfig _config;

    public static void Init(GameSettingsConfig config)
    {
        _config = config;

        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            Data = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            Data = new SettingsData();

            Data.SensMin = config.DefaultMinSensitivity;
            Data.SensMax = config.DefaultMaxSensitivity;

            Data.ScrollSpeed = config.DefaultScrollSpeed;

            Data.MusicVolume = config.DefaultMusicVolume;
            Data.SfxVolume   = config.DefaultSfxVolume;

            Save();
        }
    }

    public static void Save()
    {
        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(FilePath, json);
    }

    public static void ApplyAudio(AudioManager audio)
    {
        audio.SetMusicVolume(Data.MusicVolume);
        audio.SetSfxVolume(Data.SfxVolume);
    }
}
