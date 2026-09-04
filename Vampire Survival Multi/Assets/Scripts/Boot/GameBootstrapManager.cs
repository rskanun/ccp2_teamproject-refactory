using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameBootstrapManager : MonoBehaviour
{
    [Title("타이틀 씬")]
#if UNITY_EDITOR
    [SerializeField, OnValueChanged(nameof(SceneAssetChanged))]
    private SceneAsset titleScene;
#endif
    [SerializeField, ReadOnly] private string titleSceneName;

    [Title("참조 컴포넌트")]
    [SerializeField] private BootstrapLoadingViewer loadingViewer;
    [SerializeField] private Alert alert;

    private CancellationTokenSource cts;

#if UNITY_EDITOR
    private void SceneAssetChanged()
    {
        if (titleScene == null) return;

        titleSceneName = titleScene.name;
    }
#endif

    private void OnDestroy()
    {
        LoadingCancel();
    }

    private void LoadingCancel()
    {
        if (cts == null) return;

        cts.Cancel();
        ClearToken();
    }

    private void ClearToken()
    {
        if (cts == null) return;

        cts.Dispose();
        cts = null;
    }

    private void Awake()
    {
        BootstrapAsync().Forget();
    }

    private async UniTask BootstrapAsync()
    {
        // 로딩 취소 토큰 할당
        LoadingCancel(); // 이전 토큰 초기화
        cts = new CancellationTokenSource();

        try
        {
            loadingViewer.ClearProgress();

            // CDN 버전 체크 (0% -> 35%)
            loadingViewer.SetProgress(0.35f, "게임 업데이트 확인 중...");
            var (result, data) = await VersionCheckService.FetchVersionAsync(cts.Token);
            if (result != VersionCheckResult.Success)
            {
                AlertContent(result, data);
                return;
            }

            // 포톤 서버 연결(35% -> 75%)
            loadingViewer.SetProgress(0.75f, "서버 연결 확인 중...");
            bool photonConnected = await ConnectPhotonAsync(cts.Token);
            if (!photonConnected)
            {
                alert.Show("서버 연결에 실패했습니다.", "종료", Application.Quit);
            }

            // 포톤 로비 연결(75% -> 100%)
            loadingViewer.SetProgress(1.0f, "로비 세션 동기화 중...");
            bool lobbyConnected = await ConnectPhotonLobbyAsync(cts.Token);
            if (!lobbyConnected)
            {
                alert.Show("로비 연결에 실패했습니다.", "종료", Application.Quit);
            }

            // 타이틀 씬 로드
            await SceneManager.LoadSceneAsync(titleSceneName);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[Bootstrap] 작업이 취소되었습니다.");
        }
        catch (Exception e)
        {
            alert.Show(e.Message, "종료", Application.Quit);
        }
        finally
        {
            ClearToken();
        }
    }

    private async UniTask<bool> ConnectPhotonAsync(CancellationToken ct)
    {
        PhotonNetwork.GameVersion = Application.version;
        bool success = PhotonNetwork.ConnectUsingSettings();

        if (!success) return false;

        // 토큰 타임아웃 적용
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        // 서버 접속 및 끊길 때까지 대기
        await UniTask.WaitUntil(() =>
            PhotonNetwork.IsConnectedAndReady &&
            PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.ConnectedToMasterServer ||
            PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected,
            cancellationToken: linkedCts.Token
        );

        return PhotonNetwork.IsConnectedAndReady;
    }

    private async UniTask<bool> ConnectPhotonLobbyAsync(CancellationToken ct)
    {
        bool success = PhotonNetwork.JoinLobby();
        if (!success) return false;

        // 토큰 타임아웃 적용
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        // 로비 진입 대기
        await UniTask.WaitUntil(() =>
            PhotonNetwork.InLobby ||
            !PhotonNetwork.IsConnected,
            cancellationToken: linkedCts.Token
        );

        return PhotonNetwork.InLobby;
    }

    private void AlertContent(VersionCheckResult result, VersionData data)
    {
        switch (result)
        {
            case VersionCheckResult.Maintenance:
                alert.Show(data.maintenance_msg, "종료", Application.Quit);
                break;
            case VersionCheckResult.NeedsUpdate:
                alert.Show($"최신버전이 아닙니다: {Application.version}->{data.client_version}", "종료", Application.Quit);
                break;
        }
    }
}