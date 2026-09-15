using UnityEngine;

[CreateAssetMenu(menuName = "Game Object/Player/Class Data", fileName = "ClassData")]
public class ClassData : ObjectData
{
    [SerializeField]
    private float _attackSpeed;
    public float AttackSpeed
    {
        get { return _attackSpeed; }
    }

    [SerializeField]
    private float _lifeSteal;
    public float LifeSteal
    {
        get { return _lifeSteal; }
    }

    [Header("공격 및 스킬")]
    [SerializeField]
    private Skill _passiveSkill;
    public Skill PassiveSkill
    {
        get { return _passiveSkill; }
    }

    [SerializeField]
    private Skill _activeSkill;
    public Skill ActiveSkill
    {
        get { return _activeSkill; }
    }

    [Header("클래스 정보")]
    [SerializeField]
    private int _id;
    public int ID
    {
        get { return _id; }
    }

    [SerializeField]
    private string _name;
    public string Name
    {
        get { return _name; }
    }

    [SerializeField]
    private Sprite _icon;
    public Sprite Icon
    {
        get { return _icon; }
    }
}
