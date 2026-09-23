using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class LoadingSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LoadingViewer loadingViewer;

    private AsyncOperation sceneOp;

    private void Start()
    {
        RunLoadingPipeline().Forget();
    }

    private async UniTask RunLoadingPipeline()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        var tracker = new LoadingProgressTracker();

        // 뷰어 셋팅
        SettingsViewer(tracker, ct);

        // 씬 로드 및 Addressable 에셋 로드 동시 진행
        await UniTask.WhenAll(
            LoadAssetAsync(tracker, ct),
            LoadSceneAsync(tracker, ct)
        );

        // 완료 알림
        PhotonNetwork.LocalPlayer.SetLoadingState(true);
    }

    private void SettingsViewer(LoadingProgressTracker tracker, CancellationToken ct)
    {
        // 랜덤 팁 띄우기
        string tip = TipResource.Instance.GetRandomTip();
        loadingViewer.SetTipText(tip);

        // 로딩 애니메이션 실행
        loadingViewer.PlayLoadingAnimationAsync(ct).Forget();

        // 로딩바 업데이트 이벤트 설정
        tracker.OnProgressChanged += loadingViewer.SetProgress;
    }

    private async UniTask LoadAssetAsync(LoadingProgressTracker tracker, CancellationToken ct)
    {
        var service = NetworkObserver.Instance.GetInGamePreloadService();
        var progress = new Progress<float>(tracker.SetAssetProgress);

        await service.LoadInGameAssets(progress, ct);
    }

    private async UniTask LoadSceneAsync(LoadingProgressTracker tracker, CancellationToken ct)
    {
        sceneOp = SceneType.InGame.LoadAsync();
        sceneOp.allowSceneActivation = false;

        while (sceneOp.progress < 0.9f)
        {
            tracker.SetSceneProgress(sceneOp.progress);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // 씬 로드 직전이 되었다면 완료로 취급
        tracker.SetSceneProgress(1.0f);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        // 뷰어 업데이트
        int count = GetCompletedPlayerCount();
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        loadingViewer.SetCompletedState(count, playerCount);

        // 방장은 모든 플레이어 준비 확인
        if (!PhotonNetwork.IsMasterClient || count < playerCount) return;

        // 인게임 씬 동시 진입
        photonView.RPC(nameof(ActiveInGameScene), RpcTarget.All);
    }

    private int GetCompletedPlayerCount()
    {
        int count = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            count += player.GetLoadingState() ? 1 : 0;
        }

        return count;
    }

    [PunRPC]
    private void ActiveInGameScene(PhotonMessageInfo info)
    {
        // 방장의 호출에만 반응
        if (!info.Sender.IsMasterClient) return;

        sceneOp.allowSceneActivation = true;
    }
}