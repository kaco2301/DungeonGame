using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stat", menuName = "Stats/Enemy Stat")]
public class EnemyStatSO : ScriptableObject
{
    [Header("Common Stat")]
    public int level;
    public int MaxHp;
    public int Attack;
    public int Defense;
    public float Speed;

    [Header("Enemy Specific")]
    public int dropExp; // 처치 시 플레이어에게 줄 경험치
    // public LootTableSO lootTable; // 추후 아이템 드랍 로직을 위해 추가 가능

}


