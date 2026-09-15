using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedClassUI : MonoBehaviour
{
    [Header("직업 선택창")]
    [SerializeField] private GameObject classSelectPanel;
    [SerializeField] private TextMeshProUGUI className;
    [SerializeField] private TextMeshProUGUI activeName;
    [SerializeField] private Image activeIcon;
    [SerializeField] private TextMeshProUGUI passiveName;
    [SerializeField] private Image passiveIcon;

    [Header("기본 스킬 설명창")]
    [SerializeField] private GameObject activeDescriptionPanel;
    [SerializeField] private TextMeshProUGUI activeTitle;
    [SerializeField] private TextMeshProUGUI activeDescription;

    [Header("패시브 스킬 설명창")]
    [SerializeField] private GameObject passiveDescriptionPanel; 
    [SerializeField] private TextMeshProUGUI passiveTitle;
    [SerializeField] private TextMeshProUGUI passiveDescription;

    public void SetActiveClassPanel(bool isActive)
    {
        classSelectPanel.SetActive(isActive);
    }

    /***************************************************************
    * [ 직업 선택창 ]
    * 
    * 직업 선택창 UI 설정
    ***************************************************************/

    public void SetClassInfo(ClassData classData)
    {
        className.text = classData.Name;

        // 스킬 UI 설정
        SetActiveInfo(classData.ActiveSkill);
        SetPassiveInfo(classData.PassiveSkill);
    }

    private void SetActiveInfo(Skill activeSkill)
    {
        activeName.text = activeSkill.Name;
        activeIcon.sprite = activeSkill.Icon;

        SetActiveDescript(activeSkill);
    }

    private void SetActiveDescript(Skill activeSkill)
    {
        activeTitle.text = $"<style=ActiveSkill>기본 스킬 : </style>{activeSkill.Name}";
        activeDescription.text = activeSkill.Description;
    }

    private void SetPassiveInfo(Skill passiveSkill)
    {
        passiveName.text = passiveSkill.Name;
        passiveIcon.sprite = passiveSkill.Icon;

        SetPassiveDescript(passiveSkill);
    }

    private void SetPassiveDescript(Skill passiveSkill)
    {
        passiveTitle.text = $"<style=PassiveSkill>패시브 스킬 : </style>{passiveSkill.Name}";
        passiveDescription.text = passiveSkill.Description;
    }

    public void SetActiveSkillPanel(bool isActive)
    {
        activeDescriptionPanel.SetActive(isActive);
    }

    public void SetPassiveSkillPanel(bool isActive)
    {
        passiveDescriptionPanel.SetActive(isActive);
    }
}