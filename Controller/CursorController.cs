using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private enum CursorRequester
    {
        MainState,
        SubState
    }

    private HashSet<CursorRequester> _unlockRequesters = new HashSet<CursorRequester>();

    private void OnEnable()
    {
        // 【수정】 각 이벤트에 맞는 별도의 핸들러를 연결합니다.
        GameStateManager.OnMainStateChanged += OnMainStateChanged;
        GameStateManager.OnSubStateChanged += OnSubStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnMainStateChanged -= OnMainStateChanged;
        GameStateManager.OnSubStateChanged -= OnSubStateChanged;
    }


    private void OnMainStateChanged(MainGameState state)
    {
        // 상태가 Gameplay가 아니라면 (Dialogue, Paused 등)
        if (state != MainGameState.Gameplay)
        {
            // 'MainState'가 커서 잠금 해제를 요청했다고 목록에 추가
            _unlockRequesters.Add(CursorRequester.MainState);
        }
        else
        {
            // 'MainState'의 요청을 목록에서 제거
            _unlockRequesters.Remove(CursorRequester.MainState);
        }

        // 상태가 변경되었으므로 커서 상태를 즉시 업데이트
        UpdateCursorState();
    }

    private void OnSubStateChanged(SubGameState state)
    {
        // 상태가 UI 팝업이라면
        if (state == SubGameState.UI)
        {
            // 'SubState'가 커서 잠금 해제를 요청했다고 목록에 추가
            _unlockRequesters.Add(CursorRequester.SubState);
        }
        else
        {
            // 'SubState'의 요청을 목록에서 제거
            _unlockRequesters.Remove(CursorRequester.SubState);
        }

        // 상태가 변경되었으므로 커서 상태를 즉시 업데이트
        UpdateCursorState();
    }

    private void UpdateCursorState()
    {
        // 요청 목록에 한 명이라도 남아있다면 (Count > 0) 커서 잠금을 해제합니다.
        if (_unlockRequesters.Count > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else // 아무도 요청하지 않을 때만 (오직 순수한 Gameplay 상태일 때만) 커서를 잠급니다.
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}