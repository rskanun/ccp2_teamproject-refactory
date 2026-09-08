using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BootstrapLoadingViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Title("로딩 속도")]
    [SerializeField] private float fillSpeed = 3.0f;

    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private CancellationTokenSource cts;

    private void Awake()
    {
        ClearProgress();
    }

    private void OnDestroy()
    {
        if (cts == null) return;

        cts.Cancel();
        cts.Dispose();
    }

    public void ClearProgress()
    {
        cts?.Cancel();
        currentProgress = 0f;
        targetProgress = 0f;
    }

    public void SetProgress(float progress, string status)
    {
        targetProgress = Mathf.Clamp01(progress);
        statusText.text = status;

        cts?.Cancel();
        cts = new CancellationTokenSource();
        UpdateProgressAsync(cts.Token).Forget();
    }

    private async UniTask UpdateProgressAsync(CancellationToken ct)
    {
        while (!Mathf.Approximately(currentProgress, targetProgress))
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, fillSpeed);
            SetProgressUI(currentProgress);

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }

    private void SetProgressUI(float value)
    {
        progressSlider.value = value;
        progressText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}