using UnityEngine;

[RequireComponent (typeof(ClassPanelUI))]
public class ClassPanelManager : MonoBehaviour
{
    [Header("선택될 직업")]
    [SerializeField] private ClassData classData;

    [Header("참조 스크립트")]
    [SerializeField] private ClassPanelUI ui;
    [SerializeField] private SelectedClassManager selectManager;

    public void OnValidate()
    {
        if (classData != null)
        {
            // 직업을 넣은 경우 해당 직업 정보 띄우기
            ui.SetUI(classData);
        }
        else
        {
            // 없을 경우 정보 비활성화
            ui.DisabledUI();
        }
    }

    public void OnSelectClass()
    {
        if (ui.IsActive)
        {
            selectManager.UpdateSelectClass(classData);
        }
    }
}