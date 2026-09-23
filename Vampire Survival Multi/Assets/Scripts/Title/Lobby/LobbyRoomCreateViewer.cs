using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public readonly struct RoomCreationParams
{
    public readonly string title;
    public readonly int maxPlayers;
    public readonly RoomType type;
    public readonly string password;

    public RoomCreationParams(string title, int maxPlayers, RoomType type, string password)
    {
        this.title = title;
        this.maxPlayers = maxPlayers;
        this.type = type;
        this.password = password;
    }
}

public class LobbyRoomCreateViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TMP_InputField titleInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Dropdown maxPlayerDropdown;
    [SerializeField] private TMP_Dropdown roomTypeDropdown;
    [SerializeField] private Button submitButton;

    private UniTaskCompletionSource<RoomCreationParams> completionSource;

    public async UniTask<RoomCreationParams> ShowInputAsync(CancellationToken ct)
    {
        if (completionSource != null)
        {
            throw new InvalidOperationException("방 생성 입력 대기 중입니다.");
        }

        completionSource = new();
        gameObject.SetActive(true);

        // 입력 필드 초기화 및 포커싱
        titleInput.text = string.Empty;
        passwordInput.text = string.Empty;
        passwordInput.interactable = false;
        maxPlayerDropdown.value = 0;
        roomTypeDropdown.value = 0;
        submitButton.interactable = false;
        titleInput.ActivateInputField();

        // Destroy에도 작동하도록 토큰 합하기
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            ct,
            this.GetCancellationTokenOnDestroy()
        );

        try
        {
            // 버튼 입력 대기
            return await completionSource.Task.AttachExternalCancellation(linkedCts.Token);
        }
        finally
        {
            completionSource = null;
            gameObject.SetActive(false);
        }
    }

    public void OnSubmit()
    {
        var result = new RoomCreationParams(
            titleInput.text,
            GetMaxPlayer(),
            (RoomType)roomTypeDropdown.value,
            passwordInput.text
        );

        completionSource.TrySetResult(result);
    }

    private int GetMaxPlayer()
    {
        int idx = maxPlayerDropdown.value;
        string str = maxPlayerDropdown.options[idx].text;

        return int.Parse(str);
    }

    public void UpdatePasswordFieldInteractable()
    {
        var curType = (RoomType)roomTypeDropdown.value;

        passwordInput.text = string.Empty;
        passwordInput.interactable = curType == RoomType.Private;
    }

    public void UpdateSubmitButtonInteractable()
    {
        var curType = (RoomType)roomTypeDropdown.value;

        submitButton.interactable = !string.IsNullOrWhiteSpace(titleInput.text) &&
            (curType != RoomType.Private || !string.IsNullOrWhiteSpace(passwordInput.text));
    }
}