using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource _musicSource;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
    [SerializeField] private AudioDatabase _database;

    private Dictionary<SoundID, AudioClip> _clipDict;
    private readonly List<AudioSource> _registeredSources = new List<AudioSource>();

    public float SfxVolume => _sfxVolume;

    public event Action<float> OnSfxVolumeChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        _clipDict = new Dictionary<SoundID, AudioClip>();
        foreach (var entry in _database.Entries)
        {
            if (!_clipDict.ContainsKey(entry.ID))
                _clipDict.Add(entry.ID, entry.Clip);
        }
    }

    public void RegisterSource(AudioSource src)
    {
        if (src == null) return;
        if (!_registeredSources.Contains(src))
            _registeredSources.Add(src);
    }

    public void UnregisterSource(AudioSource src)
    {
        if (src == null) return;
        if (_registeredSources.Contains(src))
            _registeredSources.Remove(src);
    }

    public AudioClip Get(SoundID id)
    {
        if (_clipDict.TryGetValue(id, out var clip))
            return clip;

        Debug.LogWarning($"AudioManager: Clip not found for {id}");
        return null;
    }

    public void Play2D(SoundID id, float volume = 1f)
    {
        AudioClip clip = Get(id);
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(
            clip,
            Camera.main.transform.position,
            volume * _sfxVolume
        );
    }

	public void PlayUI(SoundID id, float volume = 1f)
	{
    AudioClip clip = Get(id);
    if (clip == null) return;

    GameObject go = new GameObject("UI_SFX_" + id);
    var src = go.AddComponent<AudioSource>();

    src.ignoreListenerPause = true;
    src.playOnAwake = false;
    src.spatialBlend = 0f;
    src.volume = volume * _sfxVolume;
    src.clip = clip;

    src.Play();
    Destroy(go, clip.length + 0.1f);
	}

    public void Play3D(SoundID id, Vector3 pos, float volume = 1f)
    {
        AudioClip clip = Get(id);
        if (clip == null) return;

        GameObject go = new GameObject("SFX_" + id);
        go.transform.position = pos;

        AudioSource src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.volume = volume * _sfxVolume;
        src.clip = clip;
        src.Play();

        Destroy(go, clip.length + 0.1f);
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    public void SetMusicVolume(float v)
    {
        if (_musicSource != null)
            _musicSource.volume = v;
    }

    public void PauseAllSfx()
    {
        foreach (var src in _registeredSources)
        {
            if (src == null) continue;
            if (src.isPlaying)
                src.Pause();
        }
    }

    public void ResumeAllSfx()
    {
        foreach (var src in _registeredSources)
        {
            if (src == null) continue;
            src.UnPause();
        }
    }
}
