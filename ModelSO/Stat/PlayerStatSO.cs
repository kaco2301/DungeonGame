using UnityEngine;

//PlayerStatSO는 플레이어의 성장 베이스로 필요한 값
[CreateAssetMenu(fileName = "New Player Stat", menuName = "Stats/Player Stat")]
public class PlayerStatSO : ScriptableObject
{
    [Header("Base Primary Stats")]
    public int Strength = 5;
    public int Dexterity = 6;
    public int Intelligence = 7;
    public int Vitality = 8;
    public int Spirit = 9;

    [Header("Base Derived Stats")]
    public float BaseMaxHp = 100f;
    public float BaseMaxMana = 100f;
    public float BasePhysicalDmgBonus = 0f;
    public float BaseSpellDmg = 10f;
    public float BaseDefense = 5f;

    [Header("Base Speed Settings")]
    public float BaseMoveSpeed = 1.0f;
    public float BaseAttackSpeed = 1.0f;
    public float BaseActionSpeed = 1.0f;

    [Header("Movement Multipliers")]
    public float RunMultiplier = 2.0f;
    public float SprintMultiplier = 3.0f;

    [Header("Initialization")]
    public int Level = 1;
    public int initialExp = 0;
    public int initialGold = 0;
}