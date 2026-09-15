using System.Collections.Generic;
using Photon.Pun;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class LobbyViewer : MonoBehaviour
{
    [Title("룸 내 플레이어 슬롯")]
    [SerializeField] private List<PlayerSlot> slots = new();

    [Title("그 외 구성 오브젝트")]
    [SerializeField] private TextMeshProUGUI inviteCodeText;

    public int SlotCount => slots.Count;

    private void Start()
    {
        UpdateInviteCode();
    }

    public void UpdateInviteCode()
    {
        inviteCodeText.text = PhotonNetwork.CurrentRoom.GetID();
    }

    public void CopyInviteCode()
    {
        string code = inviteCodeText.text;

        GUIUtility.systemCopyBuffer = code;
    }

    public void RefreshSlotsView()
    {
        int visits = 0;

        // 모든 플레이어에 대해서 순차적으로 UI 갱신
        foreach (var player in PhotonNetwork.PlayerList)
        {
            // 슬롯 설정에 필요한 데이터
            byte slotIdx = player.GetSlotNumber();
            byte classId = player.GetClassID();
            bool isReady = player.GetReadyState();
            bool isLocal = player.IsLocal;
            bool isMaster = player.IsMasterClient;
            bool imMaster = PhotonNetwork.LocalPlayer.IsMasterClient;
            var classData = ClassResource.Instance.GetClass(classId);
            var className = (classData == null) ? "" : classData.Name;

            // 슬롯 설정
            var slot = slots[slotIdx];
            slot.ShowPlayerInfo(true);
            slot.SetClassName(className);
            slot.SetReadyMark(isReady);
            slot.SetMasterMark(isMaster);
            slot.SetLocalMark(isLocal);
            slot.SetPlayerControlMenuButton(imMaster);

            // 플레이어 제어 메뉴는 방장이 아닐 때만 비활성화
            if (!imMaster) slot.SetPlayerControlMenu(false);

            // 방문 등록
            visits |= 1 << slotIdx;
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