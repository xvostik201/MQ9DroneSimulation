using UnityEngine;

public class SettingsBootstrap : MonoBehaviour
{
    [SerializeField] private GameSettingsConfig _config;
    [SerializeField] private AudioManager _audio;

    private void Awake()
    {
        SettingsManager.Init(_config);
        SettingsManager.ApplyAudio(_audio);

        Debug.Log("Settings initialized.");
    }
}
