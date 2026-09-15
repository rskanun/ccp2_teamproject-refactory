using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("로비 패널 오브젝트")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject roomSettingButton;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject readyButton;

    [Header("알림 패널")]
    [SerializeField] private GameObject disconnectConfirm;
    [SerializeField] private GameObject switchAdminConfirm;
    [SerializeField] private GameObject kickConfirm;

    [Header("캐릭터 선택창")]
    [SerializeField] private Button chrSelectButton;

    /***************************************************************
    * [ 로비 UI ]
    * 
    * 로비 내 UI 변경
    ***************************************************************/

    public void SetDisconnectedConfirm(bool isActive)
    {
        disconnectConfirm.SetActive(isActive);
    }

    public void SetRoomCode(string code)
    {
        roomCodeText.text = code;
    }

    public void CopyCode()
    {
        string code = roomCodeText.text;

        GUIUtility.systemCopyBuffer = code;
    }

    public void ChangeToStartButton()
    {
        readyButton.SetActive(false);
        startButton.SetActive(true);
    }

    public void ChangeToReadyButton()
    {
        readyButton.SetActive(true);
        startButton.SetActive(false);
    }

    public void SetUsableStartButton(bool isActive)
    {
        Button button = startButton.GetComponent<Button>();

        button.interactable = isActive;
    }

    /***************************************************************
    * [ 알림 패널 ]
    * 
    * 로비 내 알림 패널 활성화 여부 결정
    ***************************************************************/

    public void SetActiveAdminConfrim(bool isActive)
    {
        switchAdminConfirm.SetActive(isActive);
    }

    public void SetActiveKickConfirm(bool isActive)
    {
        kickConfirm.SetActive(isActive);
    }

    /***************************************************************
    * [ 다른 오브젝트 UI ]
    * 
    * 로비 내에 있는 타 오브젝트 활성화 여부 결정
    ***************************************************************/

    public void SetActiveSettingButton(bool isActive)
    {
        roomSettingButton.SetActive(isActive);
    }

    public void SetActiveChrSelectButton(bool isActive)
    {
        chrSelectButton.interactable = isActive;
    }
}