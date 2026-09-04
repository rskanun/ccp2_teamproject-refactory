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

public class RoomEntry : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI typeText;

    // 비공개 방 정보
    private string id;
    private string password;

    private Action<string, string> enterCallback;

    private const string PROP_ROOM_NAME = "RoomName";
    private const string PROP_ROOM_TYPE = "RoomType";

    public void SetRoomInfo(RoomInfo info, Action<string, string> enterHandler)
    {
        if (info != null && info.CustomProperties != null)
        {
            // 방 이름 정보 설정
            if (info.CustomProperties.TryGetValue(PROP_ROOM_NAME, out var nameObj) &&
                nameObj is string roomName)
            {
                titleText.text = roomName;
            }

            // 방 타입 정보 설정
            if (info.CustomProperties.TryGetValue(PROP_ROOM_TYPE, out var typeObj) &&
                typeObj is RoomType roomType)
            {
                typeText.text = roomType switch
                {
                    RoomType.Public => "공개",
                    RoomType.Private => "비공개",
                    _ => ""
                };
            }
        }

        // 방 인원 수 설정
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";

        // 입장 로직 설정
        enterCallback = enterHandler;
    }

    public void OnEnter()
    {
        enterCallback?.Invoke(id, password);
    }
}