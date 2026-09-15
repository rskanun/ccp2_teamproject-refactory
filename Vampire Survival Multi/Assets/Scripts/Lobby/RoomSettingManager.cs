using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(RoomSettingUI))]
public class RoomSettingManager : MonoBehaviour
{
    [Header("참조 스크립트")]
    [SerializeField] private RoomSettingUI ui;
    [SerializeField] private LobbyManager2 lobbyManager;

    private void Start()
    {
        InitSetting();
    }

    private void InitSetting()
    {
        int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
        int index = maxPlayer - 2;

        ui.SetMaxPlayerOption(index);
    }

    public void OnUpdateMaxPlayer()
    {
        int setMaxPlayer = ui.GetMaxPlayer();
        int playerNum = PhotonNetwork.CurrentRoom.PlayerCount;

        // 현재 인원수보다 설정하려는 인원수가 더 적을 경우
        if (setMaxPlayer < playerNum)
        {
            // 경고문 띄우기
            ui.DisplayMaxPlayerWarning();

            // 버튼 비활성화
            ui.SetActiveApplyButton(false);
        }
        else
        {
            // 경고문 삭제
            ui.ClearWarning();

            // 버튼 활성화
            ui.SetActiveApplyButton(true);
        }
    }

    public void OnClickApply()
    {
        SetMaxPlayer();

        // 로비 매니져에 변경 사항 알리기
        lobbyManager.UpdateRoomSetting();

        // 적용 후 창 닫기
        ui.SetActiveRoomSetting(false);
    }

    private void SetMaxPlayer()
    {
        int num = ui.GetMaxPlayer();

        PhotonNetwork.CurrentRoom.MaxPlayers = num;
    }
}