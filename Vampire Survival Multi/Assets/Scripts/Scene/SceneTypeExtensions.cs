using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTypeExtensions
{
    public static Scene Get(this SceneType type)
    {
        string sceneName = SceneResource.Instance.GetSceneName(type);

        // 씬 유효성 검사
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[SceneExtensions] 유효하지 않은 씬입니다: {type}");
            return default;
        }

        return SceneManager.GetSceneByName(sceneName);
    }

    public static string GetName(this SceneType type)
    {
        return SceneResource.Instance.GetSceneName(type);
    }

    public static void Load(this SceneType type, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        string sceneName = SceneResource.Instance.GetSceneName(type);

        // 씬 유효성 검사
        if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"[SceneExtensions] 유효하지 않은 씬입니다: {type}({sceneName})");
            return;
        }

        SceneManager.LoadScene(sceneName, loadMode);
    }

    public static AsyncOperation LoadAsync(this SceneType type, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        string sceneName = SceneResource.Instance.GetSceneName(type);

        // 씬 유효성 검사
        if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning($"[SceneExtensions] 유효하지 않은 씬입니다: {type}({sceneName})");
            return null;
        }

        return SceneManager.LoadSceneAsync(sceneName, loadMode);
    }
}