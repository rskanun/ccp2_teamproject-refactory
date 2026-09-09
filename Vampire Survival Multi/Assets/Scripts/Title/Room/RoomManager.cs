using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Title("방 내부 씬")]
#if UNITY_EDITOR
    [SerializeField, OnValueChanged(nameof(SceneAssetChanged))]
    private SceneAsset roomScene;
#endif
    [SerializeField, ReadOnly] private string roomSceneName;

    [Title("참조 스크립트")]
    [SerializeField] private RoomViewer viewer;
    [SerializeField] private RoomCreateViewer createViewer;
    [SerializeField] private PasswordViewer pwViewer;
    [SerializeField] private Alert alert;

    private CancellationTokenSource cts;

    private string enterRoomId;
    private string filterKeyword;
    private bool isEnterable; // 방 입장 중 혹은 다른 패널이 활성화 된 경우 방 입장 막기

    private readonly Dictionary<string, RoomInfo> cachedRooms = new(); // 방 입장 실패 탐색용
    private readonly List<RoomInfo> filterRooms = new(); // 방 목록 갱신용

#if UNITY_EDITOR
    private void SceneAssetChanged()
    {
        if (roomScene == null) return;

        roomSceneName = roomScene.name;
    }
#endif

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

    /// <summary>
    /// 방 목록 변경에 따른 정보 업데이트
    /// </summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 캐시 데이터 갱신
        UpdateCachedRooms(roomList);

        // 방 목록 갱신
        RefreashRoomListView();
    }

    private void UpdateCachedRooms(List<RoomInfo> roomList)
    {
        foreach (var room in roomList)
        {
            if (room.RemovedFromList)
            {
                cachedRooms.Remove(room.Name);
            }
            else
            {
                cachedRooms.Add(room.Name, room);
            }
        }
    }

    private void RefreashRoomListView()
    {
        filterRooms.Clear();

        // 검색 필터 적용
        foreach (var room in cachedRooms.Values)
        {
            if (room.RemovedFromList) continue;

            if (!string.IsNullOrEmpty(filterKeyword) &&
                room.Name.IndexOf(filterKeyword, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            filterRooms.Add(room);
        }

        // 방 목록 랜더
        viewer.RenderRooms(filterRooms, EnterRoom);
    }

    /// <summary>
    /// 정보 입력 패널 활성화 및 포톤 서버를 통한 방 생성
    /// </summary>
    public void OnClickCreateRoom()
    {
        CreateRoomAsync().Forget();
    }

    private async UniTask CreateRoomAsync()
    {
        // 새 토큰 할당
        ClearToken();
        cts = new CancellationTokenSource();
        var ct = cts.Token;

        try
        {
            // 패널로부터 방 생성 정보 얻어오기
            var roomParams = await createViewer.ShowInputAsync(ct);

            // 프로퍼티에 필요한 값
            string id = Guid.NewGuid().ToString();
            string hashPassword = SecurityUtility.GetPasswordHash(roomParams.password, id);

            // 방 생성
            var props = new Hashtable()
                .SetID(id)
                .SetName(roomParams.title)
                .SetType(roomParams.type)
                .SetPasswordHash(hashPassword);

            var options = new RoomOptions
            {
                MaxPlayers = roomParams.maxPlayers,
                IsVisible = roomParams.type != RoomType.Hidden,
                CustomRoomProperties = props,
                CustomRoomPropertiesForLobby = RoomPropertyExtensions.LobbyProps
            };

            PhotonNetwork.CreateRoom(id, options, null);
        }
        catch (OperationCanceledException)
        {
            // 토큰에 의한 작업 취소
        }
    }

    /// <summary>
    /// 검색 버튼 핸들러
    /// </summary>
    public void OnClickSearch()
    {
        filterKeyword = viewer.GetSearchKeyword();

        // 방 목록 다시 불러오기
        RefreashRoomListView();
    }

    /// <summary>
    /// 검색 삭제 버튼 핸들러
    /// </summary>
    public void OnClickClearSearch()
    {
        viewer.ClearSearchField();
        filterKeyword = "";

        // 방 목록 다시 불러오기
        RefreashRoomListView();
    }

    /// <summary>
    /// 방 클릭 시 실행되는 방 입장 로직
    /// </summary>
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

            var props = new Hashtable()
                .SetID(id)
                .SetPasswordHash(inputHash);

            PhotonNetwork.JoinRandomRoom(props, expectedMaxPlayers: 0);
        }
        catch (OperationCanceledException)
        {
            // 토큰에 의한 작업 취소
        }
    }

    /// <summary>
    /// 비공개 방 입장에 실패한 경우 실행되는 이벤트 핸들러
    /// </summary>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string content = GetFailedReasonWithRandomJoin(enterRoomId);

        alert.Show(content, "확인");
    }

    private string GetFailedReasonWithRandomJoin(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            return "방에 입장할 수 없습니다.";
        }

        if (!cachedRooms.TryGetValue(targetId, out var room) || room.RemovedFromList)
        {
            return "존재하지 않는 방입니다.";
        }

        if (room.MaxPlayers <= room.PlayerCount)
        {
            return "방 정원이 가득 찼습니다.";
        }

        if (!room.IsOpen)
        {
            return "입장이 마감된 방입니다.";
        }

        return "비밀번호가 일치하지 않습니다.";
    }

    /// <summary>
    /// 비공개 이외의 방 입장에 실패한 경우 실행되는 이벤트 핸들러
    /// </summary>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        string content = returnCode switch
        {
            ErrorCode.GameDoesNotExist => "존재하지 않는 방입니다.",
            ErrorCode.GameFull => "방 정원이 가득 찼습니다.",
            ErrorCode.GameClosed => "입장이 마감된 방입니다.",
            ErrorCode.ServerFull => "서버가 혼잡하여 입장할 수 없습니다.",
            _ => $"방 입장에 실패했습니다. ({returnCode})"
        };

        alert.Show(content, "확인");
    }

    /// <summary>
    /// 방 입장 핸들러
    /// </summary>
    public override void OnJoinedRoom()
    {
        // 방 입장 시, 방 정보가 있는 씬으로 이동
        SceneManager.LoadScene(roomSceneName);
    }
}