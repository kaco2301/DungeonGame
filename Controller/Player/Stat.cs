using UnityEngine;
//값이 계속 바뀌는 런타임 전용 스냅샷
[System.Serializable]
public class Stat
{
    [Header("Primary Stats")]
    [Tooltip("힘: 데미지 보너스 증가")]
    protected int _strength;
    public int Strength { get => _strength; set { _strength = value; } }

    [Tooltip("민첩: 아이템 장착속도, 공격속도, 이동속도 증가")]
    protected int _dexterity;
    public int Dexterity { get => _dexterity; set { _dexterity = value; } }

    [Tooltip("의지: 마법 피해 보너스, 마나최대치 증가")]
    protected int _intelligence;
    public int Intelligence { get => _intelligence; set { _intelligence = value; } }

    [Tooltip("활력: 최대체력 증가")]
    protected int _vitality;
    public int Vitality { get => _vitality; set { _vitality = value; } }

    [Tooltip("정신력")]
    protected int _spirit;
    public int Spirit { get => _spirit; set { _spirit = value; } }


    [Header("Base Derived Stats")]
    [SerializeField] protected int _level;
    [SerializeField] protected float _physicalDmgBonus;
    [SerializeField] protected float _spellDmgBonus;
    [SerializeField] protected float _maxHp;
    [SerializeField] protected float _maxMana;
    [SerializeField] protected float _defense;
    [SerializeField] protected float _attackSpeed;
    [SerializeField] protected float _actionSpeed;
    [SerializeField] protected float _moveSpeed;

    public int Level { get { return _level; } set { _level = value; } }
    public float PhysicalDmgBonus { get { return _physicalDmgBonus; } set { _physicalDmgBonus = value; } }
    public float SpellDmgBonus { get { return _spellDmgBonus; } set { _spellDmgBonus = value; } }
    public float MaxHp { get { return _maxHp; } set { _maxHp = value; } }
    public float MaxMana { get { return _maxMana; } set { _maxMana = value; } }
    public float Defense { get { return _defense; } set { _defense = value; } }
    public float AttackSpeed { get { return _attackSpeed; } set { _attackSpeed = value; } }
    public float ActionSpeed { get { return _actionSpeed; } set { _actionSpeed = value; } }
    public float MoveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float RunSpeed { get; set; }
    public float SprintSpeed { get; set; }

    public Stat(Stat other)
    {
        this.Strength = other.Strength;
        this.Dexterity = other.Dexterity;
        this.Intelligence = other.Intelligence;
        this.Vitality = other.Vitality;
        this.Spirit = other.Spirit;

        this.Level = other.Level;
        this.PhysicalDmgBonus = other.PhysicalDmgBonus;
        this.SpellDmgBonus = other.SpellDmgBonus;
        this.MaxHp = other.MaxHp;
        this.MaxMana = other.MaxMana;
        this.Defense = other.Defense;
        this.AttackSpeed = other.AttackSpeed;
        this.ActionSpeed = other.ActionSpeed;
        this.MoveSpeed = other.MoveSpeed;
        this.RunSpeed = other.RunSpeed;
        this.SprintSpeed = other.SprintSpeed;
    }

    public Stat(int str, int dex, int @int, int vit, int spi, 
        int level, 
        float maxHp, float maxMana, float pDmg, float sDmg, float def, float moveSpd, float atkSpd, float actSpd)
    {
        Strength = str; 
        Dexterity = dex; 
        Intelligence = @int; 
        Vitality = vit;
        Spirit = spi;
        Level = level; 
        MaxHp = maxHp; 
        MaxMana = maxMana;
        PhysicalDmgBonus = pDmg; 
        SpellDmgBonus = sDmg; 
        Defense = def;        
        MoveSpeed = moveSpd;      
        AttackSpeed = atkSpd; 
        ActionSpeed = actSpd;
    }
    //생성
    public Stat() { }
    //복제
    public Stat Clone() => new Stat(this);
}