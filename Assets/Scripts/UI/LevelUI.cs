using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [Header("General")] 
    [SerializeField] private MissionManager _manager;
    [SerializeField] private Image _background;
    [SerializeField] private float _fadeDuration = 1.2f;

    [Header("Intro Elements")]
    [SerializeField] private TMP_Text _missionName;
    [SerializeField] private TMP_Text _briefing;
    [SerializeField] private TMP_Text _risk;
    [SerializeField] private Slider _difficulty;
    [SerializeField] private Button _startButton;

    [Header("Outro Elements")]
    [SerializeField] private TMP_Text _completeTitle;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _exitButton;
    
    [Header("Keyboard Clicks Settings")]
    [SerializeField] private AudioClip[] _typingClips;
    [SerializeField] private AudioSource _typingSource;


    public event Action OnContinueButtonPressed;
    
    private bool _skipTyping;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartMissionPressed);
        _continueButton.onClick.AddListener(ContinueButton);
        _exitButton.onClick.AddListener(OnExitMissionPressed);
        
        _manager.OnMissionStarted += ShowMissionIntro;
        _manager.OnMissionCompleted += ShowMissionComplete;
        
        HideAll();
    }

    private void Start()
    {
        AudioManager.Instance.RegisterSource(_typingSource);
    }
    

    private void HideAll()
    {
        _missionName.gameObject.SetActive(false);
        _briefing.gameObject.SetActive(false);
        _risk.gameObject.SetActive(false);
        _difficulty.gameObject.SetActive(false);
        _startButton.gameObject.SetActive(false);

        _completeTitle.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
    }

    public void ShowMissionIntro()
    {
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        MissionData data = _manager.MissionData;
        
        CursorManager.Unlock();
        TimeManager.Instance.FreezeGameplay();
        TimeManager.Instance.Pause();

        _missionName.gameObject.SetActive(true);
        yield return Typewrite(_missionName, data.missionName, 0.035f);

        yield return new WaitForSecondsRealtime(0.25f);

        _briefing.gameObject.SetActive(true);
        yield return Typewrite(_briefing, data.briefingText, 0.045f);

        yield return new WaitForSecondsRealtime(0.25f);

        _risk.gameObject.SetActive(true);
        yield return Typewrite(_risk, "RISK LEVEL");

        _difficulty.gameObject.SetActive(true);
        _difficulty.value = 0;
        _difficulty.DOValue(data.difficulty, 1.2f).SetUpdate(true);
        
        _difficulty.fillRect.GetComponent<Image>().color = SetSliderColor(data.difficulty);

        _startButton.gameObject.SetActive(true);
        _startButton.image.color = new Color(1,1,1,0);
        _startButton.image.DOFade(1, 0.4f).SetUpdate(true);
    }

    public void OnStartMissionPressed()
    {
        StartCoroutine(HideIntro());
    }
    
    private void OnExitMissionPressed() => SceneLoader.Load("MainMenu");
    
    private void ContinueButton()
    { 
        OnContinueButtonPressed?.Invoke();
        
        _completeTitle.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
        _continueButton.gameObject.SetActive(false);
    } 

    private IEnumerator HideIntro()
    {
        HideAll();

        yield return _background.DOFade(0f, _fadeDuration)
            .SetUpdate(true);
        
        TimeManager.Instance.UnfreezeGameplay(); 
        TimeManager.Instance.Resume();
        CursorManager.Lock();
    }

    void PlayRandomKeyClick()
    {
        if (_typingClips.Length == 0) return;

        int index = UnityEngine.Random.Range(0, _typingClips.Length);
        _typingSource.PlayOneShot(_typingClips[index], AudioManager.Instance.SfxVolume);
    }
    
    public void ShowMissionComplete()
    {
        StartCoroutine(OutroRoutine());
    }

    private IEnumerator OutroRoutine()
    {
        yield return new WaitForSeconds(1f);
        
        CursorManager.Unlock();
        TimeManager.Instance.FreezeGameplay();

        yield return _background.DOFade(1f, _fadeDuration)
            .SetUpdate(true)
            .WaitForCompletion();

        _completeTitle.gameObject.SetActive(true);
        yield return Typewrite(_completeTitle, "MISSION COMPLETED");

        yield return new WaitForSecondsRealtime(0.25f);

        _continueButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);

        _continueButton.transform.localScale = Vector3.one * 0.8f;
        _continueButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        _exitButton.transform.localScale = Vector3.one * 0.8f;
        _exitButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private IEnumerator Typewrite(TMP_Text text, string value, float delay = 0.25f)
    {
        _skipTyping = false;
        text.text = "";

        for (int i = 0; i < value.Length; i++)
        {
            if (_skipTyping)
            {
                text.text = value;
                yield break;
            }

            char c = value[i];
            text.text += c;

            if (c != ' ' && c != '\n' && c != '\t')
                PlayRandomKeyClick();
            
            yield return new WaitForSecondsRealtime(delay);
        }
    }
    private void OnDestroy()
    {
        if (_manager == null) return;

        _manager.OnMissionStarted -= ShowMissionIntro;
        _manager.OnMissionCompleted -= ShowMissionComplete;
        
        AudioManager.Instance.UnregisterSource(_typingSource);
    }

    private Color SetSliderColor(int difficult)
    {
        switch (difficult)
        {
            case 1:
            return Color.green;
            case 2:
            return Color.yellow;
            case 3:
            return Color.red;
            default:
                return Color.white;
        }
    }

    public void SkipTyping() => _skipTyping = true;
}
