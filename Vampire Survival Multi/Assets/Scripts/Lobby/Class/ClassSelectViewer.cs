using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class ClassSelectViewer : MonoBehaviour
{
    [SerializeField] private ClassCardItem cardPrefab;
    [SerializeField] private Transform cardTransform;

    [Title("참조 컴포넌트")]
    [SerializeField] private SkillDetailViewer activeDetail;
    [SerializeField] private SkillDetailViewer passiveDetail;
    [SerializeField] private SkillTooltip activeTooltip;
    [SerializeField] private SkillTooltip passiveTooltip;

    // 담아놨다 창 닫을 때 적용
    private ClassData selectedClass;

    private void Awake()
    {
        RefreshClassViewer();
    }

    private void RefreshClassViewer()
    {
        var classDatas = ClassResource.Instance.ClassList;
        int count = (classDatas.Count + 3) / 4 * 4;

        // 선택할 클래스 ID값
        int selectedID = PhotonNetwork.LocalPlayer.GetClassID();

        for (int i = 0; i < count; i++)
        {
            var cardItem = Instantiate(cardPrefab, cardTransform);

            // 줄에 맞춰 클래스 카드 생성
            // 부족한 카드는 빈 카드로 생성
            if (i < classDatas.Count)
            {
                cardItem.ActiveCard(classDatas[i]);

                if (selectedID == classDatas[i].ID)
                {
                    cardItem.SelectClass();
                }
            }
            else
            {
                cardItem.DeactiveCard();
            }
        }
    }

    public void SelectClass(ClassData classData)
    {
        selectedClass = classData;

        // 액티브 스킬 설명 업데이트
        activeDetail.UpdateSkillDetail(classData.ActiveSkill);
        activeTooltip.UpdateSkillTooltip(classData.ActiveSkill);

        // 패시브 스킬 설명 업데이트
        passiveDetail.UpdateSkillDetail(classData.PassiveSkill);
        passiveTooltip.UpdateSkillTooltip(classData.PassiveSkill);
    }

    public void ApplySelectedClass()
    {
        var local = PhotonNetwork.LocalPlayer;
        local.SetClassID((byte)selectedClass.ID);
    }
}