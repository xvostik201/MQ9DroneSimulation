using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationPanel : MonoBehaviour
{
    [SerializeField] private RectTransform _root;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Timing")]
    [SerializeField] private float _duration = 2.5f;
    [SerializeField] private float _slideDistance = -400f;
    [SerializeField] private float _slideInTime = 0.4f;
    [SerializeField] private float _slideOutTime = 0.4f;

    public string Message { get; private set; }
    
    private HUDMessages _manager;

    public void Init(HUDMessages manager, string message, Vector2 targetPos)
    {
        _manager = manager;
        Message = message;
        _text.text = message;
        _canvasGroup.alpha = 0f;

        Vector2 start = targetPos + Vector2.right * _slideDistance;
        _root.anchoredPosition = start;

        Sequence seq = DOTween.Sequence();

        seq.Append(_root.DOAnchorPosX(targetPos.x, _slideInTime).SetEase(Ease.OutCubic));
        seq.Join(_canvasGroup.DOFade(1f, 0.3f));

        seq.AppendInterval(_duration);

        seq.Append(_canvasGroup.DOFade(0f, 0.3f));
        seq.Join(_root.DOAnchorPosX(start.x, _slideOutTime).SetEase(Ease.InCubic));

        seq.OnComplete(() =>
        {
            _manager.OnPanelClosed(this);
            Destroy(gameObject);
        });
    }

    public void MoveTo(Vector2 newPos)
    {
        _root.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
    }
}