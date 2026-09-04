using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleNetworkObserver : MonoBehaviourPunCallbacks
{
    [Title("네트워크 연결 씬")]
#if UNITY_EDITOR
    [SerializeField, OnValueChanged(nameof(OnValidateScenes))]
    private SceneAsset bootScene;
#endif
    [SerializeField, ReadOnly]
    private string bootSceneName;

    [Title("참조 컴포넌트")]
    [SerializeField] private Alert alert;

#if UNITY_EDITOR
    private void OnValidateScenes()
    {
        if (bootScene == null) return;

        bootSceneName = bootScene.name;
    }
#endif

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 비정상적인 이유로 종료된 경우만 고려
        if (cause == DisconnectCause.ApplicationQuit ||
            cause == DisconnectCause.DisconnectByClientLogic)
        {
            return;
        }

        alert.Show(
            $"네트워크 연결이 끊어졌습니다: {cause}",
            "처음 화면으로",
            () => SceneManager.LoadScene(bootSceneName)
        );
    }
}