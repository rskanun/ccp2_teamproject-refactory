using UnityEngine;

[RequireComponent(typeof(SelectedClassUI))]
public class SelectedClassManager : MonoBehaviour
{
    [Header("참조 스크립트")]
    [SerializeField] private SelectedClassUI ui;
    [SerializeField] private LobbyManager lobbyManager;

    public void Start()
    {
        // 초기 클래스 설정
        ClassData initClass = ClassResource.Instance.GetDefaultClass();
        if (initClass == null)
        {
            Debug.LogError("ClassList is either null or empty");
            return;
        }

        UpdateSelectClass(initClass);
    }

    public void UpdateSelectClass(ClassData classData)
    {
        // 로컬 플레이어의 직업 설정
        LocalPlayerData.Instance.SetClass(classData);

        ui.SetClassInfo(classData);
    }
}