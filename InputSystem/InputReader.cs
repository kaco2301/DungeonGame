using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;

namespace Kaco.InputSystem
{
    public class InputReader : MonoBehaviour, Controls.IPlayerActions, Controls.IUIActions
    {
        [Header("Character Input Values")]
        public Vector2 _mouseDelta;
        public Vector2 _moveComposite;
        public float _movementInputDuration;
        public bool _movementInputDetected;
        public Vector2 MousePosition => Mouse.current.position.ReadValue();
        private Controls _controls;

        
        #region IPlayerActions
        public Action onAimActivated;
        public Action onAimDeactivated;

        public Action onCrouchActivated;
        public Action onCrouchDeactivated;

        public Action onJumpPerformed;

        public Action onAttackPerformed;

        public Action onInteractPerformed; // (기존) '탭' 전용으로 사용
        public Action onInteractHoldStarted; // [신규] '홀드 시작'
        public Action onInteractHoldCanceled;

        public Action onLockOnToggled;

        public Action onSprintActivated;
        public Action onSprintDeactivated;

        public Action onWalkToggled;
        #endregion

        #region IUIActions
        public Action onInventoryPerformed;
        public Action onEquipmentPerformed;

        public Action onMapActivated;
        public Action onMapDeactivated;

        public Action onEscPerformed;

        #endregion
        
        /// <summary>
        /// 【추가】 게임 상태가 변경될 때 호출될 핸들러입니다.
        /// </summary>
        

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
                _controls.UI.SetCallbacks(this);
            }

            _controls.Player.Enable();
            _controls.UI.Enable();

        }

        public void OnDisable()
        {
            _controls.Player.Disable();
            _controls.UI.Disable();

        }

        public void OnLook(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveComposite = context.ReadValue<Vector2>();
            _movementInputDetected = _moveComposite.magnitude > 0;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onJumpPerformed?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onSprintActivated?.Invoke();
            }
            else if (context.canceled)
            {
                onSprintDeactivated?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onCrouchActivated?.Invoke();
            }
            else if (context.canceled)
            {
                onCrouchDeactivated?.Invoke();
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onAimActivated?.Invoke();
            }

            if (context.canceled)
            {
                onAimDeactivated?.Invoke();
            }
        }

        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onLockOnToggled?.Invoke();
            onSprintDeactivated?.Invoke();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }
            onInventoryPerformed?.Invoke();
        }

        public void OnMap(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                onMapActivated?.Invoke();
            }
            else if (context.canceled)
            {
                onMapDeactivated?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) // 1. 액션이 "성공"했을 때
            {
                if (context.interaction is TapInteraction)
                {
                    // 1a. "탭"이 성공했을 때
                    Debug.Log("InputReader: Tap Performed");
                    onInteractPerformed?.Invoke();
                }
                else if (context.interaction is HoldInteraction)
                {
                    // 1b. [신규] "홀드"가 성공(지정된 시간을 채움)했을 때
                    Debug.Log("InputReader: Hold Performed");
                    onInteractHoldStarted?.Invoke();
                }
                // (참고: 기본 Press Interaction이 남아있다면 else { onInteractPerformed?.Invoke(); }가 될 수 있음)
            }
            else if (context.canceled) // 2. 액션이 "취소"되었을 때 (키를 뗌)
            {
                // "홀드"가 진행 중이었다가 취소된 경우
                Debug.Log("InputReader: Hold Canceled");
                onInteractHoldCanceled?.Invoke();
            }
        }

        

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }
            if (UI_Utility.IsPointerOverUI())
            {
                return;
            }
            onAttackPerformed?.Invoke();
        }

        public void OnEsc(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            onEscPerformed?.Invoke();
        }
    }
}
