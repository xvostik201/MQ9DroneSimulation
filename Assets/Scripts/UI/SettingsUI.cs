using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private GameObject _settingsUI;

    [Header("Exit Button")]
    [SerializeField] private Button _exitButton;

    [Header("Sensitivity")]
    [SerializeField] private Slider _sensMinSlider;
    [SerializeField] private Slider _sensMaxSlider;
    [SerializeField] private TMP_Text _sensMinText;
    [SerializeField] private TMP_Text _sensMaxText;

    [Header("Scroll Speed")]
    [SerializeField] private Slider _scrollSlider;
    [SerializeField] private TMP_Text _scrollText;

    [Header("Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _musicText;
    [SerializeField] private TMP_Text _sfxText;

    private GameMenuUI _gameMenuUI;
    
    private bool _isGameplayContext = false;
    private bool _isOpen;
    public bool IsOpen => _isOpen;

    public event Action OnSettingsMenuClose;

    public void SetGameplayContext(bool value)
    {
        _isGameplayContext = value;
    }

    public void Init(GameMenuUI gameMenuUI)
    {
        _gameMenuUI = gameMenuUI;
    }

    private void Awake()
    {
        _settingsUI.SetActive(false);
    }

    private void Start()
    {
        _exitButton.onClick.AddListener(Close);
        _exitButton.onClick.AddListener(() => AudioManager.Instance.PlayUI(SoundID.UI_Click, AudioManager.Instance.SfxVolume));

        LoadValues();

        _sensMinSlider.onValueChanged.AddListener(OnSensMinChanged);
        _sensMaxSlider.onValueChanged.AddListener(OnSensMaxChanged);
        _scrollSlider.onValueChanged.AddListener(OnScrollChanged);

        _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    private void LoadValues()
    {
        _sensMinSlider.value = SettingsManager.Data.SensMin;
        _sensMaxSlider.value = SettingsManager.Data.SensMax;
        _scrollSlider.value = SettingsManager.Data.ScrollSpeed;

        _musicSlider.value = SettingsManager.Data.MusicVolume;
        _sfxSlider.value = SettingsManager.Data.SfxVolume;

        _sensMinText.text = SettingsManager.Data.SensMin.ToString("0.0000");
        _sensMaxText.text = SettingsManager.Data.SensMax.ToString("0.00");
        _scrollText.text = SettingsManager.Data.ScrollSpeed.ToString("0.0");

        _musicText.text = SettingsManager.Data.MusicVolume.ToString("0.00");
        _sfxText.text = SettingsManager.Data.SfxVolume.ToString("0.00");
    }

    public void Open()
    {
        _isOpen = true;
        _settingsUI.SetActive(true);

        if (_isGameplayContext)
        {
            TimeManager.Instance.Pause();
            CursorManager.Unlock();
        }
    }

    public void Close()
    {
        if (!_isOpen)
            return;

        _isOpen = false;
        _settingsUI.SetActive(false);

        if (_isGameplayContext)
        {
            TimeManager.Instance.Resume();
            CursorManager.Lock();
        }

        OnSettingsMenuClose?.Invoke();
    }

    private void OnSensMinChanged(float val)
    {
        SettingsManager.Data.SensMin = val;
        SettingsManager.Save();
        _sensMinText.text = val.ToString("0.0000");
    }

    private void OnSensMaxChanged(float val)
    {
        SettingsManager.Data.SensMax = val;
        SettingsManager.Save();
        _sensMaxText.text = val.ToString("0.00");
    }

    private void OnScrollChanged(float val)
    {
        SettingsManager.Data.ScrollSpeed = val;
        SettingsManager.Save();
        _scrollText.text = val.ToString("0.0");
    }

    private void OnMusicChanged(float val)
    {
        SettingsManager.Data.MusicVolume = val;
        SettingsManager.Save();
        _musicText.text = val.ToString("0.00");
        AudioManager.Instance.SetMusicVolume(val);
    }

    private void OnSfxChanged(float val)
    {
        SettingsManager.Data.SfxVolume = val;
        SettingsManager.Save();
        _sfxText.text = val.ToString("0.00");
        AudioManager.Instance.SetSfxVolume(val);
    }
}
