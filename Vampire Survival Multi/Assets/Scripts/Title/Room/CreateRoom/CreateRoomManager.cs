using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(CreateRoomUI))]
public class CreateRoomManager : MonoBehaviour
{
    [Header("참조 스크립트")]
    [SerializeField] private CreateRoomUI ui;

    public void CreateNewRoom()
    {
        // 방 만들기
        var properties = new Hashtable();

        string id = Guid.NewGuid().ToString();
        string title = ui.GetRoomName();
        var type = ui.GetRoomType();
        string pw = ui.GetPassword();
        string hashPw = SecurityUtility.GetPasswordHash(pw, id);

        string[] keys = new string[4] { "RoomId", "RoomName", "RoomType", "RoomPasswordHash" };

        properties.Add(keys[0], id);
        properties.Add(keys[1], title);
        properties.Add(keys[2], type);
        properties.Add(keys[3], hashPw);

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = ui.GetMaxPlayer(),
            IsVisible = type != RoomType.Hidden,
            CustomRoomProperties = properties,
            CustomRoomPropertiesForLobby = keys
        };

        PhotonNetwork.CreateRoom(id, options, null);
    }

    public void EnablePassword()
    {
        RoomType type = ui.GetRoomType();

        if (type.Equals(RoomType.Private))
        {
            // 방 타입이 비공개이면 비밀번호 입력칸 활성화
            ui.SetActivePwField(true);
        }
        else
        {
            // 그 외엔 비활성화
            ui.SetActivePwField(false);
        }
    }

    public void EnableCreateButton()
    {
        RoomType type = ui.GetRoomType();

        if (type.Equals(RoomType.Private))
        {
            bool isActive = ui.IsInputPw && ui.IsInputTitle;

            ui.SetCreateButton(isActive);
        }
        else
        {
            bool isActive = ui.IsInputTitle;

            ui.SetCreateButton(isActive);
        }
    }
}