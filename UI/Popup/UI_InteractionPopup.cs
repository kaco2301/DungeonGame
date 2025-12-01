using UnityEngine;

/// <summary>
/// 상호작용(예: 상자 열기, 시체 파밍)으로 인해 열리는 팝업의
/// 기본 클래스입니다. UIManager가 이 타입의 팝업을 식별하여
/// 상호작용이 종료될 때 자동으로 닫을 수 있습니다.
/// </summary>
public abstract class UI_InteractionPopup : UI_Popup
{
    public override bool IsInteractionPopup => true;

}
