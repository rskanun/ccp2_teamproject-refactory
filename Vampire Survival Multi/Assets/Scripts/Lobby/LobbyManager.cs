using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<PlayerSlot> slots = new();

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.MasterClient.IsLocal)
        {
            InitLockedSlot();
        }

        RefreshSlotsView();
    }

    private void InitLockedSlot()
    {
        byte slotState = 0;
        int unlockCount = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 4321 순서
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            slotState = (byte)(slotState << 1 + (unlockCount > i ? 0 : 1));
        }

        // 슬롯 정보 설정
        PhotonNetwork.CurrentRoom.SetSlotState(slotState);
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
            .SetReadyState(false);

        // 프로퍼티 설정
        newPlayer.SetCustomProperties(props);

        // 방 슬롯 상태 설정
        UpdateSlot(slotIdx, true);

        // UI 전체 적용
        photonView.RPC(nameof(RefreshSlotsView), RpcTarget.All);
    }

    /// <summary>
    /// 왼쪽부터 가장 가까운 빈 슬롯 번호 가져오기
    /// </summary>
    /// <returns></returns>
    private byte GetEmptySlot()
    {
        byte state = PhotonNetwork.CurrentRoom.GetSlotState();

        // 해당 칸의 슬롯이 비어있는지, 왼쪽부터 확인
        for (int i = 0; i < slots.Count; i++)
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

    [PunRPC]
    private void RefreshSlotsView()
    {
        int visits = 0;

        // 모든 플레이어에 대해서 순차적으로 UI 갱신
        var players = PhotonNetwork.CurrentRoom.Players.Values;
        foreach (var player in players)
        {
            // 슬롯 설정에 필요한 데이터
            byte slotIdx = player.GetSlotNumber();
            byte classId = player.GetClassID();
            bool isReady = player.GetReadyState();
            bool isLocal = player.IsLocal;
            bool isAdmin = player.IsMasterClient;
            var classData = ClassResource.Instance.GetClass(classId);
            var className = (classData == null) ? "" : classData.Name;

            // 슬롯 설정
            var slot = slots[slotIdx];
            slot.ShowPlayerInfo(true);
            slot.SetClassName(className);
            slot.SetReadyMark(isReady);
            slot.SetAdminMark(isAdmin);
            slot.SetLocalMark(isLocal);

            // 방문 등록
            visits &= 1 << slotIdx;
        }

        byte slotsState = PhotonNetwork.CurrentRoom.GetSlotState();
        for (int i = 0; i < slots.Count; i++)
        {
            // 플레이어가 있는 슬롯 건너뛰기
            if (((visits >> i) & 1) == 1) continue;

            bool isOccupied = ((slotsState >> i) & 1) == 1;

            var slot = slots[i];
            slot.ShowPlayerInfo(false);
            slot.SetClosedMark(isOccupied);
        }
    }


}