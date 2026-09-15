using TMPro;
using UnityEngine;

public class PlayerPanelUI : MonoBehaviour
{
    [Header("플레이어 메뉴")]
    [SerializeField] private GameObject playerMenuButton;
    [SerializeField] private GameObject playerMenuPanel;

    [Header("패널 구성 오브젝트")]
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameObject closedMark;
    [SerializeField] private GameObject playerName;
    [SerializeField] private TextMeshProUGUI className;

    [Header("현재 상태 오브젝트")]
    [SerializeField] private GameObject localMark;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private GameObject adminPanel;

    /***************************************************************
    * [ 플레이어 메뉴 ]
    * 
    * 플레이어 메뉴 활성화 상태 조정
    ***************************************************************/

    public void SetActivePlayerMenuBtn(bool isActive)
    {
        playerMenuButton.SetActive(isActive);
    }

    public void TogglePlayerMenu()
    {
        bool isActive = playerMenuPanel.activeSelf;

        playerMenuPanel.SetActive(!isActive);
    }

    /***************************************************************
    * [ 패널 구성 ]
    * 
    * 패널 구성 오브젝트 설정
    ***************************************************************/

    public void SetActiveCloseMark(bool isActive)
    {
        closedMark.SetActive(isActive);
    }

    public void SetActiveCharacter(bool isActive)
    {
        playerCharacter.SetActive(isActive);
        playerName.SetActive(isActive);
    }

    public void SetClassName(string name)
    {
        className.text = name;
    }

    public string GetClassName()
    {
        return className.text;
    }

    /***************************************************************
    * [ 상태 오브젝트 ]
    * 
    * 현재 플레이어의 상태를 나타내는 오브젝트 설정
    ***************************************************************/

    public void SetLocalMark(bool isActive)
    {
        localMark.SetActive(isActive);
    }

    public void SetReadyPanel(bool isActive)
    {
        if (adminPanel.activeSelf == false)
        {
            readyPanel.SetActive(isActive);
        }
    }

    public void SetAdminPanel(bool isActive)
    {
        adminPanel.SetActive(isActive);
        readyPanel.SetActive(false);
    }
}