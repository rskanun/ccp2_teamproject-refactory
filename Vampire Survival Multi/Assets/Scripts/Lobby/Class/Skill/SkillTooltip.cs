using TMPro;
using UnityEngine;

public abstract class SkillTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;

    protected abstract string style { get; }
    protected abstract string skillType { get; }

    public void UpdateSkillTooltip(Skill skill)
    {
        skillNameText.text = $"<style={style}>{skillType} : </style>{skill.Name}";
        skillDescriptionText.text = skill.Description;
    }
}