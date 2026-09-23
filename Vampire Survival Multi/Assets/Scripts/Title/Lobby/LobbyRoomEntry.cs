using System;
using Photon.Realtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public enum RoomType
{
    Public,
    Private,
    Hidden
}

public class LobbyRoomEntry : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI typeText;

    // 비공개 방 정보
    private string id;
    private bool isPrivate;

    private Action<string, bool> enterCallback;

    public void SetRoomInfo(RoomInfo info, Action<string, bool> enterHandler)
    {
        if (info != null && info.CustomProperties != null)
        {
            // 방 이름 설정
            titleText.text = info.GetName();

            // 방 타입 설정
            var type = info.GetRoomType();
            typeText.text = type switch
            {
                RoomType.Public => "공개",
                RoomType.Private => "비공개",
                _ => ""
            };

            isPrivate = type == RoomType.Private;

            // 방 ID 설정
            id = info.GetID();
        }

        // 방 인원 수 설정
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        // 입장 로직 설정
        enterCallback = enterHandler;
    }

    public void OnEnter()
    {
        enterCallback?.Invoke(id, isPrivate);
    }
}