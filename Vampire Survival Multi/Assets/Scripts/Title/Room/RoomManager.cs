using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Title("참조 스크립트")]
    [SerializeField] private RoomViewer viewer;
    [SerializeField] private PasswordViewer pwViewer;

    private const string PROP_PASSWORD_HASH = "RoomPasswordHash";
    private const string PROP_ROOM_ID = "RoomId";
    private CancellationTokenSource cts;
    private string filterKeyword;

    private void OnDestroy()
    {
        ClearToken();
    }

    private void ClearToken()
    {
        if (cts == null) return;

        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 검색 필터 적용
        if (string.IsNullOrEmpty(filterKeyword))
        {
            roomList = GetFilterRooms(roomList);
        }

        // 방 목록 랜더
        viewer.RenderRooms(roomList, (id, isPrivate) => EnterRoom(id, isPrivate));
    }

    private List<RoomInfo> GetFilterRooms(List<RoomInfo> rooms)
    {
        if (string.IsNullOrEmpty(filterKeyword))
        {
            // 키워드가 없다면 본래 목록 리턴
            return rooms;
        }

        // 키워드가 포함된 방목록 리턴
        return rooms.Where(r => r.Name.IndexOf(filterKeyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }

    public void OnClickSearch()
    {
        filterKeyword = viewer.GetSearchKeyword();
    }

    public void OnClickClearSearch()
    {
        viewer.ClearSearchField();
        filterKeyword = "";
    }

    public void EnterRoom(string id, bool isPrivate)
    {
        if (isPrivate)
        {
            // 비공개 방은 비밀번호 입력 후 방 입장
            EnterRoomAsync(id, isPrivate).Forget();
        }
        else
        {
            // 그 외엔 그냥 입장
            PhotonNetwork.JoinRoom(id);
        }
    }

    private async UniTask EnterRoomAsync(string id, bool isPrivate)
    {
        // 새 토큰 할당
        ClearToken();
        cts = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            // Private 걸러내기
            if (!isPrivate)
            {
                PhotonNetwork.JoinRoom(id);
                return;
            }

            string inputPW = await pwViewer.ShowInputAsync(ct);
            string inputHash = SecurityUtility.GetPasswordHash(inputPW, id);

            var props = new Hashtable
            {
                { PROP_ROOM_ID, id },
                { PROP_PASSWORD_HASH, inputHash }
            };

            PhotonNetwork.JoinRandomRoom(props, expectedMaxPlayers: 0);
        }
        catch (OperationCanceledException)
        {
            // 토큰에 의한 작업 취소
        }
    }
}