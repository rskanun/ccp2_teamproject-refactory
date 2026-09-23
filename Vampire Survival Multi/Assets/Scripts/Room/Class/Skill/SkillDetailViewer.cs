using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillDetailViewer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private Image skillIconFrame;

    public void SetSkillDetail(Skill skill)
    {
        skillNameText.text = skill.Name;
        skillIconFrame.sprite = skill.Icon;
    }
}