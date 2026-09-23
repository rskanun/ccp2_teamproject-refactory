using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Alert : MonoBehaviour
{
    private struct PopupData
    {
        public string content;
        public string confirmText;
        public Action handler;

        public PopupData(string content, string confirmText, Action handler)
        {
            this.content = content;
            this.confirmText = confirmText;
            this.handler = handler;
        }
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private AlertViewer popup;

    // 팝업 실행 제어 변수
    private UniTaskCompletionSource completionSource;
    private Queue<PopupData> readyQueue = new();
    private bool isDisplay;

    public static Alert Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            Destroy(canvas.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(canvas.gameObject);
    }

    public static void Show(string content, string confirmText, Action handler = null)
    {
        if (Instance == null) return;

        Instance.RequestPopup(content, confirmText, handler);
    }

    private void RequestPopup(string content, string confirmText, Action handler)
    {
        var data = new PopupData(content, confirmText, handler);

        // 디스플레이 할 데이터 큐에 등록
        readyQueue.Enqueue(data);

        // 이미 팝업이 띄워진 경우 이후 종료
        if (isDisplay) return;

        // 캔슬토큰 만들기
        var ct = this.GetCancellationTokenOnDestroy();

        // 팝업 띄우기
        ShowNextAsync(ct).Forget();
    }

    private async UniTask ShowNextAsync(CancellationToken ct)
    {
        isDisplay = true;

        try
        {
            while (readyQueue.Count > 0)
            {
                var nextData = readyQueue.Dequeue();

                completionSource = new();

                string content = nextData.content;
                string confirmText = nextData.confirmText;
                var handler = nextData.handler;

                popup.Show(content, confirmText, () => OnConfirm(handler));

                await completionSource.Task.AttachExternalCancellation(ct);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            popup.gameObject.SetActive(false);
            completionSource = null;
            isDisplay = false;
        }
    }

    private void OnConfirm(Action handler)
    {
        try
        {
            handler?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
        finally
        {
            completionSource.TrySetResult();
        }
    }
}