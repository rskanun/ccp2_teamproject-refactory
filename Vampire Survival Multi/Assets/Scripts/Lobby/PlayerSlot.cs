using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [Title("구성 오브젝트")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI className;

    [Title("상태 오브젝트")]
    [SerializeField] private Image closedMark;
    [SerializeField] private GameObject localMark;
    [SerializeField] private GameObject readyMark;
    [SerializeField] private GameObject adminMark;

    [Title("플레이어 조작 메뉴")]
    [SerializeField] private GameObject playerControlMenu;

    public void ShowPlayerInfo(bool isActive)
    {
        infoPanel.SetActive(isActive);
    }

    public void SetClassName(string name)
    {
        className.text = name;
    }

    public void SetClosedMark(bool isActive)
    {
        closedMark.gameObject.SetActive(isActive);
    }

    public void SetLocalMark(bool isActive)
    {
        localMark.SetActive(isActive);
    }

    public void SetReadyMark(bool isActive)
    {
        readyMark.SetActive(isActive);
    }

    public void SetAdminMark(bool isActive)
    {
        adminMark.SetActive(isActive);
    }

    public void SetPlayerControlMenu(bool isActive)
    {
        playerControlMenu.SetActive(isActive);
    }
}