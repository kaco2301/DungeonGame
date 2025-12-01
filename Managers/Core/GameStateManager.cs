using System;
using UnityEngine;

public enum MainGameState
{
    Gameplay,   // 일반 플레이 상태
    Dialogue,   // 대화 중
    Paused      // 일시정지 메뉴
}

public enum SubGameState
{
    None,
    UI,
}

public interface IPausable
{
    void Pause();
    void Resume();
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public MainGameState MainState { get; private set; } = MainGameState.Gameplay;
    public SubGameState SubState { get; private set; } = SubGameState.None;

    public static event Action<MainGameState> OnMainStateChanged;
    public static event Action<SubGameState> OnSubStateChanged;

    private void Awake() => Instance = this;

    /// <summary>
    /// Gameplay 일반 플레이 상태, Dialogue 대화 중, Inventory, Paused
    /// </summary>
    /// <param name="newState"></param>
    public void SetMainState(MainGameState state)
    {
        if (MainState == state) return;

        MainState = state;
        OnMainStateChanged?.Invoke(state); // 상태 변경을 모두에게 알림
        Debug.Log($"게임 상태 변경: {state}");
    }

    public void SetSubState(SubGameState state)
    {
        if (SubState == state) return;

        SubState = state;
        OnSubStateChanged?.Invoke(state); // 상태 변경을 모두에게 알림
        Debug.Log($"게임 상태 변경: {state}");
    }
}
