using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button[] _menuButtons;
    [SerializeField] private SettingsUI _settingsUI;

    [Header("Menu Animation")]
    [SerializeField] private RectTransform _buttonsParent;
    [SerializeField] private Vector2 _openAnchoredPos;
    [SerializeField] private Vector2 _closeAnchoredPos;
    [SerializeField] private float _animTime = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutCubic;

    public event Action OnGameMenuOpen;

    private UIInput _uiInput;
    private DroneInput _droneInput;

    private bool _isMenuOpen;

    private const int RESUME_INDEX = 0;
    private const int SETTINGS_INDEX = 1;
    private const int EXIT_INDEX = 2;

    private Tween _menuTween;
    private GameMenuUI _gameMenuUI;

    public void Init(GameMenuUI gameMenuUI)
    {
        _gameMenuUI = gameMenuUI;
    }

    private void Awake()
    {
        _buttonsParent.anchoredPosition = _closeAnchoredPos;

        foreach (var b in _menuButtons)
            b.onClick.AddListener(() => AudioManager.Instance.PlayUI(SoundID.UI_Click, AudioManager.Instance.SfxVolume));

        _menuButtons[RESUME_INDEX].onClick.AddListener(OnResumePressed);
        _menuButtons[SETTINGS_INDEX].onClick.AddListener(OnSettingsPressed);
        _menuButtons[EXIT_INDEX].onClick.AddListener(OnExitPressed);
    }

    public void Init(UIInput ui, DroneInput di, SettingsUI settingsUI)
    {
        _uiInput = ui;
        _droneInput = di;
        _settingsUI = settingsUI;

        _uiInput.OnCancel += ToggleMenu;
        _droneInput.OnCancel += ToggleMenu;
        
        _settingsUI.SetGameplayContext(true);

        _settingsUI.OnSettingsMenuClose += ShowMenuOnly;
    }

    private void OnDestroy()
    {
        _uiInput.OnCancel -= ToggleMenu;
        _droneInput.OnCancel -= ToggleMenu;

        if (_settingsUI != null)
            _settingsUI.OnSettingsMenuClose -= ShowMenuOnly;
    }

    public void ToggleMenu()
    {
        if (TimeManager.Instance.IsGameLocked)
            return;

        if (_settingsUI != null && _settingsUI.IsOpen)
        {
            _settingsUI.Close();  
            return;
        }

        if (_isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        _isMenuOpen = true;

        TimeManager.Instance.Pause();
        CursorManager.Unlock();

        OnGameMenuOpen?.Invoke();

        Animate(_openAnchoredPos);
    }

    private void CloseMenu()
    {
        _isMenuOpen = false;

        TimeManager.Instance.Resume();
        CursorManager.Lock();

        Animate(_closeAnchoredPos);
    }

    private void ShowMenuOnly()
    {
        if (!_isMenuOpen)
            OpenMenu();
    }

    private void Animate(Vector2 target)
    {
        _menuTween?.Kill();

        _menuTween = _buttonsParent
            .DOAnchorPos(target, _animTime)
            .SetEase(_ease)
            .SetUpdate(true);
    }

    private void OnResumePressed() => CloseMenu();

    private void OnSettingsPressed()
    {
        CloseMenu();
        _settingsUI.Open();
    }

    private void OnExitPressed()
    {
        SceneLoader.Load("MainMenu");
    }
}
