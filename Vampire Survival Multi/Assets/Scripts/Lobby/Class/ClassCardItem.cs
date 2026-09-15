using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassCardItem : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private Image classIconFrame;
    [SerializeField] private Toggle toggle;

    public ClassData ClassData { get; private set; }

    public void ActiveCard(ClassData classData)
    {
        ClassData = classData;

        // 캔버스 그룹 설정
        group.interactable = true;
        group.alpha = 1.0f;

        // 직업 정보 설정
        classNameText.text = classData.Name;
        classIconFrame.sprite = classData.Icon;
    }

    public void DeactiveCard()
    {
        ClassData = null;

        // 캔버스 그룹 설정
        group.interactable = false;
        group.alpha = 0.0f;
    }

    public void SelectClass()
    {
        toggle.isOn = true;
    }
}