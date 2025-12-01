using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SettingsUI _settingsUI;
    [SerializeField] private Button[] _menuButtons;
    [SerializeField] private RectTransform _buttonsParent;
    
    [Header("Buttons anim")]
    [SerializeField] private Vector2 _animAnchorPos;
    [SerializeField] private float _animDuration = 0.5f;
    private Vector2 _buttonsParentRectTransform;
    private bool _isMenuOpen = true;

    [Header("Drone")]
    [SerializeField] private MenuDronBehaviour _drone;

    private const int PLAY_INDEX = 0;
    private const int SETTINGS_INDEX = 1;
    private const int EXIT_INDEX = 2;

    private void OnEnable()
    {
        _settingsUI.OnSettingsMenuClose += ToggleButtons;
    }

    private void OnDisable()
    {
        _settingsUI.OnSettingsMenuClose -= ToggleButtons;
    }

    private void Awake()
    {
        _settingsUI.SetGameplayContext(false);
        
        _buttonsParentRectTransform = _buttonsParent.anchoredPosition;

        _menuButtons[PLAY_INDEX].onClick.AddListener(OnPlay);
        _menuButtons[SETTINGS_INDEX].onClick.AddListener(OnSettings);
        _menuButtons[EXIT_INDEX].onClick.AddListener(OnExit);
    }

    private void Start()
    {
        foreach (var button in _menuButtons)
        {
            button.onClick.AddListener(ToggleButtons);
            button.onClick.AddListener( () => AudioManager.Instance.PlayUI(SoundID.UI_Click, AudioManager.Instance.SfxVolume));
        }
    }

    private void ToggleButtons()
    {
        _isMenuOpen = !_isMenuOpen;
        
        Vector2 animPos = _isMenuOpen ? _buttonsParentRectTransform : _animAnchorPos;
        
        foreach (var b in _menuButtons)
            b.interactable = _isMenuOpen;

        _buttonsParent.DOAnchorPos(animPos, _animDuration);
    }

    private void OnPlay() => _drone.StartSequence();
    private void OnSettings()
    {
        _settingsUI.Open();
    }

    private void OnExit() => Application.Quit();
}