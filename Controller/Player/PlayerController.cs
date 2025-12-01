using Kaco.InputSystem;
using UnityEngine;

//onAttackPerformed->Attack->OnAttack->Animation
public class PlayerController : MonoBehaviour
{
    private InputReader _inputReader;

    private PlayerCombat _combat;

    private void Awake()
    {
        _inputReader = GetComponent<InputReader>();
        _combat = GetComponent<PlayerCombat>();
    }

    private void OnEnable()
    {
        _inputReader.onAttackPerformed += HandleAttackInput;
    } 

    private void OnDisable()
    {
        _inputReader.onAttackPerformed -= HandleAttackInput;
    }

    private void HandleAttackInput()
    {
        // 전투 로직은 Combat에게 전적으로 위임
        _combat.OnAttackInput();
    }

}
