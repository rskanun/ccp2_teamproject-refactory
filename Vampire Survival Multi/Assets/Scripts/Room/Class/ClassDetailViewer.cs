using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ClassDetailViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private SkillDetailViewer activeDetail;
    [SerializeField] private SkillDetailViewer passiveDetail;
    [SerializeField] private SkillTooltip activeTooltip;
    [SerializeField] private SkillTooltip passiveTooltip;

    public void SetClassName(ClassData classData)
    {
        classNameText.text = classData.Name;
    }

    public void SetActiveSkillDetail(Skill activeSkill)
    {
        activeDetail.SetSkillDetail(activeSkill);
        activeTooltip.SetSkillTooltip(activeSkill);
    }

    public void SetPassiveSkillDetail(Skill passiveSkill)
    {
        passiveDetail.SetSkillDetail(passiveSkill);
        passiveTooltip.SetSkillTooltip(passiveSkill);
    }
}