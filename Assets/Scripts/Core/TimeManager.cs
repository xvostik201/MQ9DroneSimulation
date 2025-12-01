using System;
using UnityEngine;


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public bool IsPaused { get; private set; }

    public bool IsGameplayFrozen { get; private set; }

    public bool IsGameLocked => IsGameplayFrozen;

    public event Action<bool> OnPauseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance.PauseAllSfx();
        OnPauseChanged?.Invoke(true);
        
        Debug.Log("Time scale -" + Time.timeScale);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance.ResumeAllSfx();
        OnPauseChanged?.Invoke(false);
        Debug.Log("Time scale -" + Time.timeScale);
    }

    public void FreezeGameplay()
    {
        IsGameplayFrozen = true;
    }

    public void UnfreezeGameplay()
    {
        IsGameplayFrozen = false;
    }

    public void SetSpeed(float scale)
    {
        Time.timeScale = scale;

        OnPauseChanged?.Invoke(IsPaused);
    }
}
