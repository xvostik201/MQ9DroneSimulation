using System.Collections.Generic;
using UnityEngine;

public class HUDMessages : MonoBehaviour
{
    [SerializeField] private NotificationPanel _prefab;
    [SerializeField] private RectTransform _container;

    [Header("Layout Settings")]
    [SerializeField] private Vector2 _startPosition = new Vector2(100f, -100f);
    [SerializeField] private float _verticalSpacing = 80f;
    [SerializeField] private int _maxVisible = 3;

    private readonly List<NotificationPanel> _active = new();
    private readonly Queue<string> _queue = new();

    private readonly HashSet<string> _activePanelText = new();

    public void ShowMessage(string text)
    {
        if (_activePanelText.Contains(text))
            return;

        if (_active.Count >= _maxVisible)
        {
            _queue.Enqueue(text);
            return;
        }

        CreatePanel(text);
    }

    private void CreatePanel(string message)
    {
        var panel = Instantiate(_prefab, _container);
        Vector2 pos = _startPosition + Vector2.down * _verticalSpacing * _active.Count;

        panel.Init(this, message, pos);
        _active.Add(panel);
        _activePanelText.Add(message); 
    }

    public void OnPanelClosed(NotificationPanel panel)
    {
        _active.Remove(panel);
        _activePanelText.Remove(panel.Message); 

        RearrangePanels();

        if (_queue.Count > 0)
            CreatePanel(_queue.Dequeue());
    }

    private void RearrangePanels()
    {
        for (int i = 0; i < _active.Count; i++)
        {
            Vector2 targetPos = _startPosition + Vector2.down * _verticalSpacing * i;
            _active[i].MoveTo(targetPos);
        }
    }
}