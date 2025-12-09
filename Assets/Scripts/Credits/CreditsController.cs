using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private RectTransform _creditsRoot;
    [SerializeField] private float _scrollSpeed = 50f;
    [SerializeField] private float _endYOffset = 1200f;

    private bool _isFinished;

    private void Update()
    {
        if (_isFinished)
            return;

        _creditsRoot.anchoredPosition += Vector2.up * (_scrollSpeed * Time.deltaTime);

        if (_creditsRoot.anchoredPosition.y >= _endYOffset)
        {
            _isFinished = true;
            SceneLoader.LoadSceneDelayed("MainMenu", 1f);

        }

        if (Input.anyKeyDown)
        {
            SceneLoader.LoadSceneDelayed("MainMenu", 1f);
        }
    }
}