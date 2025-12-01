using UnityEngine;

public class EnemyStat : MonoBehaviour, IDamageable
{
    [SerializeField]
    protected EnemyStatSO statSO;
    private Animator _animator;
    [SerializeField] private Stat currentStats = new Stat();

    public Stat CurrentStats => currentStats;

    private static readonly int _dieHash = Animator.StringToHash("Die");
    private static readonly int _damagedHash = Animator.StringToHash("TakeDamage");
    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _animator = GetComponent<Animator>();
        CurrentStats.MaxHp = statSO.MaxHp;
        CurrentStats.PhysicalDmgBonus = statSO.Attack;
        CurrentStats.Defense = statSO.Defense;
        CurrentStats.MoveSpeed = statSO.Speed;
    }

    public virtual void TakeDamage(float damage)
    {
    //    if (CurrentStats.Hp <= 0) return;

    //    float finalDamage = Mathf.Max(damage - CurrentStats.Defense, 0);
    //    Debug.Log(finalDamage);

    //    if (CurrentStats.Hp - finalDamage <= 0)
    //    {
    //        CurrentStats.Hp = 0;
    //        Die();
    //    }
    //    else
    //    {
    //        CurrentStats.Hp -= finalDamage;
    //        _animator.SetTrigger(_damagedHash);
    //    }
    }

    protected virtual void Die()
    {
        _animator.SetTrigger(_dieHash);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject,5f);
    }
}
    