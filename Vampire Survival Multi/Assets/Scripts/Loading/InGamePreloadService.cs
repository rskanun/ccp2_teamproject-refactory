using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InGamePreloadService : IDisposable
{
    private const string ASSET_LABEL = "InGame";

    private AsyncOperationHandle loadAssetHandle;
    public bool IsLoaded => loadAssetHandle.IsValid() &&
        loadAssetHandle.Status == AsyncOperationStatus.Succeeded;

    public void Dispose()
    {
        UnloadInGameAssets();
    }

    public async UniTask<bool> LoadInGameAssets(IProgress<float> progress, CancellationToken ct)
    {
        // 이미 로드된 상태면 결과만 리턴
        if (IsLoaded)
        {
            progress?.Report(1.0f);
            return true;
        }

        try
        {
            // 에셋 로드 시작
            loadAssetHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(ASSET_LABEL, null);
            await loadAssetHandle.ToUniTask(
                progress: progress,
                cancellationToken: ct
            );

            // 상태 검증
            if (loadAssetHandle.Status != AsyncOperationStatus.Succeeded)
            {
                // 실패 시 자원 해제
                Debug.LogError($"[InGamePreloadService] 에셋 로드 실패: {loadAssetHandle.OperationException}");
                UnloadInGameAssets();
                return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            // 토큰 취소의 경우 상위 시퀀서로 던지기
            UnloadInGameAssets();
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InGamePreloadService] 에셋 로드 중 예외 발생: {ex}");
            UnloadInGameAssets();
            return false;
        }
    }

    public void UnloadInGameAssets()
    {
        if (!loadAssetHandle.IsValid()) return;

        Addressables.Release(loadAssetHandle);
        loadAssetHandle = default;
    }
}