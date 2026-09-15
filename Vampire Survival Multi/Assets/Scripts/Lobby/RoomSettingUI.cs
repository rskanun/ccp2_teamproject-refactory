using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSettingUI : MonoBehaviour
{
    [Header("방 설정 패널")]
    [SerializeField] private GameObject roomSettingPanel;
    [SerializeField] private GameObject noTouchPanel;

    [Header("참조 오브젝트")]
    [SerializeField] private TMP_Dropdown maxPlayerOption;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button applyButton;

    public void SetActiveRoomSetting(bool isActive)
    {
        roomSettingPanel.SetActive(isActive);
        noTouchPanel.SetActive(isActive);
    }

    public int GetMaxPlayer()
    {
        int index = maxPlayerOption.value;
        int maxPlayer = index + 2;

        return maxPlayer;
    }

    public void SetMaxPlayerOption(int index)
    {
        maxPlayerOption.value = index;
    }

    public void DisplayMaxPlayerWarning()
    {
        string warning = "현재 인원보다 적은 인원으론 설정할 수 없습니다!";

        SetWarning(warning);
    }

    public void SetWarning(string content)
    {
        warningText.text = content;
    }

    public void ClearWarning()
    {
        warningText.text = "";
    }

    public void SetActiveApplyButton(bool isActive)
    {
        applyButton.interactable = isActive;
    }
}