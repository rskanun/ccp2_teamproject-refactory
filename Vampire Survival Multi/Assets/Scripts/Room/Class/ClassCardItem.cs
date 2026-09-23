using System;
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

    private Action<ClassData> selectHandler;

    public void SetToggleGroup(ToggleGroup group)
    {
        toggle.group = group;
    }

    public void ActiveCard(ClassData classData, Action<ClassData> selectHandler = null)
    {
        ClassData = classData;
        this.selectHandler = selectHandler;

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
        selectHandler?.Invoke(ClassData);
    }

    public void OnToggleStateChanged(bool isSelected)
    {
        if (!isSelected) return;

        SelectClass();
    }
}