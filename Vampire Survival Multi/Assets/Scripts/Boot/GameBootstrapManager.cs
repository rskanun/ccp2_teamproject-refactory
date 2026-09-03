using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class GameBootstrapManager : MonoBehaviour
{
    [SerializeField] private BootstrapLoadingViewer loadingViewer;
    [SerializeField] private BootstrapAlertViewer alertViewer;

    private CancellationTokenSource cts;

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
        await UniTask.WaitForSeconds(2.0f);

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
                alertViewer.ViewAlert("서버 연결에 실패했습니다.", "종료", Application.Quit);
            }

            // 포톤 로비 연결(75% -> 100%)
            loadingViewer.SetProgress(1.0f, "로비 세션 동기화 중...");
            bool lobbyConnected = await ConnectPhotonLobbyAsync(cts.Token);
            if (!lobbyConnected)
            {
                alertViewer.ViewAlert("로비 연결에 실패했습니다.", "종료", Application.Quit);
            }

            Debug.Log("로딩 완료");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[Bootstrap] 작업이 취소되었습니다.");
        }
        catch (Exception e)
        {
            alertViewer.ViewAlert(e.Message, "종료", Application.Quit);
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
                alertViewer.ViewAlert(data.maintenance_msg, "종료", Application.Quit);
                break;
            case VersionCheckResult.NeedsUpdate:
                alertViewer.ViewAlert($"최신버전이 아닙니다: {Application.version}->{data.client_version}", "종료", Application.Quit);
                break;
        }
    }
}