using Photon.Pun;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class RoomSettingsViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TMP_Dropdown maxPlayerOption;
    [SerializeField] private TMP_InputField roomTitleOption;
    private const int MinPlayerOffset = 2;

    private void Start()
    {
        InitOption();
    }

    private void InitOption()
    {
        var room = PhotonNetwork.CurrentRoom;

        // 기존 방 설정 가져오기
        int curMax = Mathf.Max(MinPlayerOffset, room.MaxPlayers);
        maxPlayerOption.value = curMax - MinPlayerOffset;
        roomTitleOption.text = room.GetName();
    }


    public void OnConfirm()
    {
        // 방장 외 조작 거르기
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            return;
        }

        if (!IsValidMaxPlayerOption(out int newCount))
        {
            Alert.Show("현재 인원보다 적은 인원으론 설정할 수 없습니다!", "닫기");
            return;
        }

        if (!IsValidTitleField(out string newTitle))
        {
            Alert.Show("방 제목을 입력하세요.", "닫기");
            return;
        }

        var room = PhotonNetwork.CurrentRoom;
        room.SetName(newTitle);

        int maxCount = maxPlayerOption.options.Count + MinPlayerOffset - 1;
        room.MaxPlayers = Mathf.Clamp(newCount, MinPlayerOffset, maxCount);

        // 설정이 완료된 경우에만 창 닫기
        gameObject.SetActive(false);
    }

    private bool IsValidMaxPlayerOption(out int settingsCount)
    {
        int curCount = PhotonNetwork.CurrentRoom.PlayerCount;
        settingsCount = maxPlayerOption.value + MinPlayerOffset;

        return curCount <= settingsCount;
    }

    private bool IsValidTitleField(out string settingsTitle)
    {
        settingsTitle = roomTitleOption.text?.Trim();

        return !string.IsNullOrEmpty(settingsTitle);
    }
}