using UnityEngine;

public class Define
{
    public enum UIEvent
    {
        Click,
        Drag, 
        BeginDrag,    // <--- 추가
        EndDrag,      // <--- 추가
        Drop,        // <--- 추가
        PointerEnter, // <--- 추가
        PointerExit
    }

    public enum EquipmentType
    {
        Weapon,
        Helmet,
        Armor,
        Gloves,
        Boots,
        Amulet,
        Ring01,
        Ring02,
        SubWeapon,
    }

    public enum ItemRarity
    {
        Common,
        Rare,
        Unique,
        Epic,
        Legendary
    }

    public enum PrimaryStats
    {
        Strength,//물리 공격력, 장비 착용 한도에 영향
        Spirit,//받는 피해 감소
        Intelligence,//마나 효율, 마법 방어, 버프 지속 시간
        Dexterity,//공격 속도, 회피율, 크리티컬 확률
        Vitality//체력(HP), HP 회복량, 상태이상 저항
    }

    public enum StatAttribute
    {
        Strength,
        Dexterity,
        Intelligence,
        Vitality,
        Agility,
        MaxHp,   // 최대 체력 증가
        MaxMana, // 최대 마나 증가
        Defense
    }

    public enum ResourceAttribute
    {
        CurrentHp,   // 체력 회복
        CurrentMana  // 마나 회복
    }

    public enum WeaponType
    {
        Unarmed = 0, // 맨손
        OneHandSword = 1,
        TwoHandSword = 2,
        Bow = 3,
    }

    public enum ObjectiveType
    {
        Kill,
        Collect,
        TalkTo,
    }

    public enum QuestStatus 
    {
        NotStarted, // 시작 안 함
        InProgress, // 진행 중
        Completed,  // 목표는 달성했으나 보상은 아직 받지 않음
        TurnedIn    // 보상까지 모두 완료
    }

    public enum UILayer
    {
        System,
        Modal,
        Popup,
        Scene,
    }

    public enum InteractionUIType
    {
        None,
        Prompt, // 단순 텍스트용
        Indicator, // 아이콘, 이름, 텍스트가 모두 있는 아이템용
    }

    public enum InteractionType
    {
        Tap, // 즉시 실행 (아이템 줍기 등)
        Hold // 게이지를 채워 실행 (문 열기 등)
    }
}
