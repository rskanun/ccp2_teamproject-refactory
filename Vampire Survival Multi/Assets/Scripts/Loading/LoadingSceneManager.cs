using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadingSceneManager : MonoBehaviour
{
    private void Start()
    {
        RunLoadingPipeline().Forget();
    }

    private async UniTask RunLoadingPipeline()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        // 씬 로드 및 Addressable 에셋 로드 동시 진행
        await UniTask.WhenAll(
            LoadAssetAsync(ct),
            LoadSceneAsync(ct)
        );

        // 인게임 씬 로드

        // 다른 플레이어 로드 대기
    }

    private async UniTask LoadAssetAsync(CancellationToken ct)
    {

    }

    private async UniTask LoadSceneAsync(CancellationToken ct)
    {

    }
}