using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ClassDetailViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TextMeshProUGUI classNameText;
    [SerializeField] private SkillDetailViewer activeDetail;
    [SerializeField] private SkillDetailViewer passiveDetail;


}