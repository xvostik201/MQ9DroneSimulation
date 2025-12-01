using UnityEngine;

[CreateAssetMenu(menuName = "Audio/AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
    public SoundEntry[] Entries;
}

[System.Serializable]
public class SoundEntry
{
    public SoundID ID;
    public AudioClip Clip;
}