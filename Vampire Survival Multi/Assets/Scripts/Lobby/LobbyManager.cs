using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private LobbyViewer viewer;

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps) => viewer.RefreshSlotsView();
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) => viewer.RefreshSlotsView();
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) => viewer.RefreshSlotsView();

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.MasterClient.IsLocal)
        {
            InitLockedSlot();
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

    public void TransferMasterClient(Photon.Realtime.Player newMaster)
    {
        // 방장 전용 함수
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.SetMasterClient(newMaster);
    }
}