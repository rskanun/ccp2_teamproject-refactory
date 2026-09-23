using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectViewer : MonoBehaviour
{
    [SerializeField] private ClassCardItem cardPrefab;
    [SerializeField] private Transform cardTransform;

    [Title("참조 컴포넌트")]
    [SerializeField] private ClassDetailViewer detailViewer;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private ToggleGroup group;

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
            cardItem.SetToggleGroup(group);

            // 줄에 맞춰 클래스 카드 생성
            // 부족한 카드는 빈 카드로 생성
            if (i < classDatas.Count)
            {
                cardItem.ActiveCard(classDatas[i], SelectClass);

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

        // 직업 이름 업데이트
        detailViewer.SetClassName(classData);

        // 스킬 설명 업데이트
        detailViewer.SetActiveSkillDetail(classData.ActiveSkill);
        detailViewer.SetPassiveSkillDetail(classData.PassiveSkill);
    }

    public void ApplySelectedClass()
    {
        // 방장에게 직업 변경 요청만 보내기
        photonView.RPC(
            nameof(RoomSessionReceiver.RequestChangeClass),
            RpcTarget.MasterClient,
            selectedClass.ID
        );
    }
}