using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Game Settings Config")]
public class GameSettingsConfig : ScriptableObject
{
    [Header("Sensitivity Defaults")]
    public float DefaultMinSensitivity = 0.3f;
    public float DefaultMaxSensitivity = 0.3f;

    [Header("Scroll Speed Default")]
    public float DefaultScrollSpeed = 5f;

    [Header("Audio Defaults")]
    public float DefaultMusicVolume = 1f;
    public float DefaultSfxVolume = 1f;
}
