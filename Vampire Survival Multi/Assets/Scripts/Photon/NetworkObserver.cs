using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkObserver : MonoBehaviourPunCallbacks
{
    public static NetworkObserver Instance { get; private set; }

    private InGamePreloadService preloadService;

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

    public override void OnLeftRoom()
    {
        preloadService?.Dispose();
        preloadService = null;
    }

    public InGamePreloadService GetInGamePreloadService()
    {
        preloadService?.Dispose();
        return preloadService = new InGamePreloadService();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 비정상적인 이유로 종료된 경우만 고려
        if (cause == DisconnectCause.ApplicationQuit ||
            cause == DisconnectCause.DisconnectByClientLogic)
        {
            return;
        }

        string bootScene = SceneResource.Instance.GetSceneName(SceneType.Booting);
        var scene = SceneManager.GetSceneByName(bootScene);
        if (scene.IsValid() && scene.isLoaded)
        {
            // 네트워크 연결 씬이 현재 씬인 경우
            Alert.Show(
                $"네트워크 연결이 끊어졌습니다: {cause}",
                "게임 종료",
                ExitGame
            );
        }
        else
        {
            // 그 외에 씬에선 부팅 씬으로 돌아오기
            Alert.Show(
                $"네트워크 연결이 끊어졌습니다: {cause}",
                "처음 화면으로",
                ReturnBootScene
            );
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ReturnBootScene()
    {
        SceneType.Booting.Load();
    }
}