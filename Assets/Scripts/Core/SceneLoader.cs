using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private class SceneLoaderRunner : MonoBehaviour { }

    private static SceneLoaderRunner _runner;
    private static bool _isLoading;

    private static void EnsureRunner()
    {
        if (_runner != null) return;

        var go = new GameObject("[SceneLoader]");
        Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<SceneLoaderRunner>();
    }

    public static void Load(string sceneName)
    {
        EnsureRunner();
        if (_isLoading) return;

        _runner.StartCoroutine(LoadAsyncInternal(sceneName));
    }

    public static IEnumerator LoadAsync(string sceneName)
    {
        EnsureRunner();
        if (_isLoading) yield break;

        yield return LoadAsyncInternal(sceneName);
    }

    private static IEnumerator LoadAsyncInternal(string sceneName)
    {
        _isLoading = true;

        if (TimeManager.Instance != null)
            TimeManager.Instance.Resume();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;


        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        _isLoading = false;
    }
}