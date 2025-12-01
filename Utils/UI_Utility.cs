using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UI_Utility
{
    public static bool IsPointerOverUI()
    {
        // 1. 현재 이벤트 시스템에 대한 포인터 이벤트 데이터를 생성합니다.
        var eventData = new PointerEventData(EventSystem.current);

        // 2. 현재 포인터 위치를 설정합니다.
        // Input System은 Pointer.current를 통해 현재 포인터에 접근하는 것이 좋습니다.
        eventData.position = UnityEngine.InputSystem.Pointer.current.position.ReadValue();

        // 3. 레이캐스트 결과를 담을 리스트를 생성합니다.
        var results = new List<RaycastResult>();

        // 4. 모든 그래픽 레이캐스터(씬에 있는 모든 캔버스)에 대해 레이캐스트를 실행합니다.
        EventSystem.current.RaycastAll(eventData, results);

        // 5. 결과가 하나라도 있다면, 포인터가 UI 위에 있다는 의미입니다.
        return results.Count > 0;
    }
}
