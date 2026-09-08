using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PasswordViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TMP_InputField inputField;

    private UniTaskCompletionSource<string> completionSource;

    public async UniTask<string> ShowInputAsync(CancellationToken ct)
    {
        if (completionSource != null)
        {
            throw new InvalidOperationException("Password input is already awaiting.");
        }

        completionSource = new UniTaskCompletionSource<string>();
        gameObject.SetActive(true);

        // 입력 필드 초기화 및 포커싱
        inputField.text = string.Empty;
        inputField.ActivateInputField();

        // Destroy에도 작동하도록 토큰 합하기
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            ct,
            this.GetCancellationTokenOnDestroy()
        );

        try
        {
            // 입력 대기
            return await completionSource.Task.AttachExternalCancellation(linkedCts.Token);
        }
        finally
        {
            completionSource = null;
            gameObject.SetActive(false);
        }
    }

    // UI 버튼 OnClick 할당 함수
    public void OnSubmit()
    {
        OnSubmit(inputField.text);
    }

    // TMP_InputField의 OnSubmit 할당 함수
    public void OnSubmit(string text)
    {
        if (completionSource == null) return;

        completionSource.TrySetResult(text);
    }
}