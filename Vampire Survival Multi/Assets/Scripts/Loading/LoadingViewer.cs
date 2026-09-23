using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingViewer : MonoBehaviour
{
    [Title("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Slider loadingBar;

    [Title("로딩 애니메이션 설정")]
    public float loadingAnimationDelay = 0.5f;

    private readonly string[] loadingTexts = new string[]
    {
        "Loaindg",
        "Loaindg .",
        "Loaindg . .",
        "Loaindg . . .",
    };

    public void SetTipText(string tip)
    {
        tipText.text = tip;
    }

    public void SetProgress(float progress)
    {
        progressText.text = $"{progress} %";
        loadingBar.value = progress;
    }

    public void SetCompletedState(int count, int maxCount)
    {
        stateText.text = $"<{count}/{maxCount}> 준비 완료";
    }

    public async UniTask PlayLoadingAnimationAsync(CancellationToken ct)
    {
        int index = 0;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                loadingText.text = loadingTexts[index];
                index = (index + 1) % loadingTexts.Length;

                await UniTask.Delay(TimeSpan.FromSeconds(loadingAnimationDelay), cancellationToken: ct);
            }
        }
        catch (OperationCanceledException)
        {
            // 토큰 취소 무시
        }
    }
}