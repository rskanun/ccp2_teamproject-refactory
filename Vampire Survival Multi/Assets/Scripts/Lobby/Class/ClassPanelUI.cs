using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassPanelUI : MonoBehaviour
{
    [Header("구성 오브젝트")]
    [SerializeField] private TextMeshProUGUI className;
    [SerializeField] private Image classIcon;
    [SerializeField] private Toggle toggle;

    public bool IsActive
    {
        get { return toggle.isOn; }
    }

    public void SetUI(ClassData classData)
    {
        toggle.interactable = true;

        className.text = classData.Name;
        classIcon.sprite = classData.Icon;

        // 이미지 알파값 설정
        Color color = classIcon.color;
        color.a = 1f;
        classIcon.color = color;
    }

    public void DisabledUI()
    {
        className.text = "";
        toggle.interactable = false;

        // 이미지 알파값 설정
        Color color = classIcon.color;
        color.a = 0f;
        classIcon.color = color;
    }
}