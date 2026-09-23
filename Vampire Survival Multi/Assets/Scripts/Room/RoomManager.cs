using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomViewer viewer;
    [SerializeField] private PhotonView sessionPhotonView;
    private bool isRefreshScheduled;

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps) => RequestRefresh();
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) => RequestRefresh();

    public void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.MasterClient.IsLocal)
        {
            InitLockedSlot();

            InitEnterPlayer(PhotonNetwork.LocalPlayer);
        }
    }

    private void InitLockedSlot()
    {
        byte slotState = 0;
        int unlockCount = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 4321 순서
        for (int i = viewer.SlotCount - 1; i >= 0; i--)
        {
            slotState = (byte)(slotState << 1 + (unlockCount > i ? 0 : 1));
        }

        // 슬롯 정보 설정
        PhotonNetwork.CurrentRoom.SetSlotState(slotState);
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        // 새로 방장된 플레이어의 준비 상태 변경
        if (newMasterClient.IsLocal && newMasterClient.GetReadyState())
        {
            // 이후 방장이 아니게 된 경우 준비 상태로 변환되는 버그 방지
            newMasterClient.SetReadyState(false);
        }

        RequestRefresh();
    }

    private void RequestRefresh()
    {
        // 뷰어 갱신까지 대기
        if (isRefreshScheduled) return;

        isRefreshScheduled = true;
        RunRefreshViewer().Forget();
    }

    private async UniTask RunRefreshViewer()
    {
        // 마지막 프레임에 모아서 갱신
        await UniTask.Yield(PlayerLoopTiming.LastUpdate);

        viewer.RefreshViewer();
        isRefreshScheduled = false;
    }

    public override void OnLeftRoom()
    {
        // 네트워크가 튕겼을 경우 작동하지 않도록 방어
        if (!PhotonNetwork.InLobby) return;

        // 모종의 이유로 방에서 나가진 경우 타이틀 씬으로 이동
        SceneType.Title.Load();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        InitEnterPlayer(newPlayer);
    }

    private void InitEnterPlayer(Photon.Realtime.Player newPlayer)
    {
        // 방장 처리 로직
        if (!PhotonNetwork.IsMasterClient) return;

        // 플레이어 초기 프로퍼티 값
        byte slotIdx = GetEmptySlot();
        byte defaultClassID = (byte)ClassResource.Instance.GetDefaultClass().ID;

        var props = PlayerPropertyExtensions.CreateBuilder()
            .SetSlotNumber(slotIdx)
            .SetClass(defaultClassID)
            .SetReadyState(false)
            .SetLoadingState(false);

        // 프로퍼티 설정
        newPlayer.SetCustomProperties(props);

        // 방 슬롯 상태 설정
        UpdateSlot(slotIdx, true);
    }

    /// <summary>
    /// 왼쪽부터 가장 가까운 빈 슬롯 번호 가져오기
    /// </summary>
    /// <returns></returns>
    private byte GetEmptySlot()
    {
        byte state = PhotonNetwork.CurrentRoom.GetSlotState();

        // 해당 칸의 슬롯이 비어있는지, 왼쪽부터 확인
        for (int i = 0; i < viewer.SlotCount; i++)
        {
            // 0 = 비어있음, 1 = 닫혀있거나, 누군가 있음
            if (((state >> i) & 1) == 1)
            {
                continue;
            }

            return (byte)i;
        }

        return byte.MaxValue;
    }

    /// <summary>
    /// 방의 슬롯 상태 프로퍼티 업데이트
    /// </summary>
    private void UpdateSlot(byte index, bool isOccupied)
    {
        byte newState = PhotonNetwork.CurrentRoom.GetSlotState();
        if (isOccupied)
        {
            // index 자리를 1로 설정
            newState = (byte)(newState | (1 << index));
        }
        else
        {
            // index 자리를 0으로 해제
            newState = (byte)(newState & ~(1 << index));
        }

        // 상태 업데이트
        PhotonNetwork.CurrentRoom.SetSlotState(newState);
    }

    /// <summary>
    /// 현재 있는 방 나가기
    /// </summary>
    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// 준비 버튼 핸들러
    /// </summary>
    public void ReadyGame()
    {
        sessionPhotonView.RPC(
            nameof(RoomSessionReceiver.RequestToggleReady),
            RpcTarget.MasterClient
        );
    }

    /// <summary>
    /// 게임 시작 여부 판단 및 게임 시작 씬 진입
    /// </summary>
    public void GameStart()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 모든 플레이어 준비 여부 확인
        if (!IsReadyAllPlayers()) return;

        // 방 잠궈서 유입 배제
        PhotonNetwork.CurrentRoom.IsOpen = false;

        // 로딩 씬 동시 진입
        string loadingSceneName = SceneType.Loading.GetName();
        PhotonNetwork.LoadLevel(loadingSceneName);
    }

    private bool IsReadyAllPlayers()
    {
        var currentRoom = PhotonNetwork.CurrentRoom;
        if (currentRoom == null) return false;

        foreach (var player in currentRoom.Players.Values)
        {
            // 방장 제외
            if (player.IsMasterClient) continue;

            bool isReady = player.GetReadyState();
            if (!isReady) return false;
        }

        return true;
    }
}