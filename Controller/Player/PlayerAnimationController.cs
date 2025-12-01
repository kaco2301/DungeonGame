using Kaco.CameraSystem;
using Kaco.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaco.Player
{
    public class PlayerAnimationController : MonoBehaviour, IPausable
    {
        #region Enum
        private enum AnimationState
        {
            Base,
            Locomotion,
            Jump,
            Fall,
            Crouch,
        }
        #endregion

        #region Animation Variable Hashes
        private readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
        private readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
        private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
        private readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
        private readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");

        private static readonly int _weaponTypeHash = Animator.StringToHash("WeaponType");
        private static readonly int _attackHash = Animator.StringToHash("Attack");
        private static readonly int _comboIndexHash = Animator.StringToHash("ComboIndex");

        private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
        private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");

        private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
        private readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");

        private readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");

        private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
        private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");

        private readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
        private readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
        private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
        private readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");

        private readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");

        private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
        private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
        private readonly int _isStartingHash = Animator.StringToHash("IsStarting");

        private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

        private readonly int _leanValueHash = Animator.StringToHash("LeanValue");
        private readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
        private readonly int _headLookYHash = Animator.StringToHash("HeadLookY");

        private readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
        private readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");

        private readonly int _locomotionStartDirectionHash = Animator.StringToHash("LocomotionStartDirection");
        #endregion

        #region Player Settings Variables
        #region Scripts/Objects
        [Header("External Components")]
        [SerializeField] private CameraManager _cameraController;
        private InputReader _inputReader;
        private Animator _animator;
        private CharacterController _characterController;
        private PlayerMovement _movement;
        #endregion

        #region Locomotion Settings
        [Header("Player Locomotion")]
        [Header("Main Settings")]
        [Tooltip("Whether the character always faces the camera facing direction")]
        [SerializeField] private bool _alwaysStrafe = true;
        [Tooltip("Offset for camera rotation.")]
        [SerializeField] private float _cameraRotationOffset;
        #endregion

        #region Shuffle Settings
        [Header("Shuffles")]
        [Tooltip("Direction of shuffling on the X-axis.")]
        [SerializeField] private float _shuffleDirectionX;
        [Tooltip("Direction of shuffling on the Z-axis.")]
        [SerializeField] private float _shuffleDirectionZ;
        #endregion

        #region Head Look Settings
        [Header("Player Head Look")]
        [Tooltip("Flag indicating if head turning is enabled.")]
        [SerializeField] private bool _enableHeadTurn = true;
        [Tooltip("Delay for head turning.")]
        [SerializeField] private float _headLookDelay;
        [Tooltip("X-axis value for head turning.")]
        [SerializeField] private float _headLookX;
        [Tooltip("Y-axis value for head turning.")]
        [SerializeField] private float _headLookY;
        [Tooltip("Curve for X-axis head turning.")]
        [SerializeField] private AnimationCurve _headLookXCurve;
        #endregion

        #region Body Look Settings
        [Header("Player Body Look")]
        [Tooltip("Flag indicating if body turning is enabled.")]
        [SerializeField] private bool _enableBodyTurn = true;
        [Tooltip("Delay for body turning.")]
        [SerializeField] private float _bodyLookDelay;
        [Tooltip("X-axis value for body turning.")]
        [SerializeField] private float _bodyLookX;
        [Tooltip("Y-axis value for body turning.")]
        [SerializeField] private float _bodyLookY;
        [Tooltip("Curve for X-axis body turning.")]
        [SerializeField] private AnimationCurve _bodyLookXCurve;
        #endregion

        #region Lean Settings
        [Header("Player Lean")]
        [Tooltip("Flag indicating if leaning is enabled.")]
        [SerializeField] private bool _enableLean = true;
        [Tooltip("Delay for leaning.")]
        [SerializeField] private float _leanDelay;
        [Tooltip("Current value for leaning.")]
        [SerializeField] private float _leanValue;
        [Tooltip("Curve for leaning.")]
        [SerializeField] private AnimationCurve _leanCurve;
        [Tooltip("Delay for head leaning looks.")]
        [SerializeField] private float _leansHeadLooksDelay;
        [Tooltip("Flag indicating if an animation clip has ended.")]
        [SerializeField] private bool _animationClipEnd;
        #endregion
        #endregion

        #region Runtime Properties
        private readonly List<GameObject> _currentTargetCandidates = new List<GameObject>();
        private AnimationState _currentState = AnimationState.Base;
        private bool _cannotStandUp;
        private bool _crouchKeyPressed;
        // movement-related flags are now read from _movement
        private bool _isStarting;
        private bool _isStopped = true;
        private bool _isTurningInPlace;
        private bool _isWalking;
        private bool _movementInputHeld;
        private bool _movementInputPressed;
        private bool _movementInputTapped;
        private float _currentMaxSpeed;
        private float _locomotionStartDirection;
        private float _locomotionStartTimer;
        private float _newDirectionDifferenceAngle;
        private Vector3 _currentRotation = new Vector3(0f, 0f, 0f);
        private Vector3 _previousRotation;
        #endregion

        #region Base State Variables
        private const float _ANIMATION_DAMP_TIME = 5f;
        private const float _STRAFE_DIRECTION_DAMP_TIME = 20f;
        private float _targetMaxSpeed;
        private float _fallStartTime;
        private float _rotationRate;
        private float _initialLeanValue;
        private float _initialTurnValue;
        private Vector3 _cameraForward;
        private Vector3 _targetVelocity;
        private bool _isPaused;
        #endregion

        #region Animation Controller
        private void Start()
        {
            _inputReader = GetComponent<InputReader>();
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _movement = GetComponent<PlayerMovement>();

            // Ensure movement initial strafing mode matches animator setting
            if (_movement != null)
            {
                _movement.IsStrafing = _alwaysStrafe;
            }

            SwitchState(AnimationState.Locomotion);
        }

        #endregion

        #region Sprinting / Crouch Wrappers
        // These wrappers are kept for compatibility with other systems that may call them.
        public void ActivateSprint()
        {
            // movement handles flags; animator just reads
            if (_movement != null && !_movement.IsCrouching)
            {
                _movement.IsWalking = false;
                _movement.IsSprinting = true;
                _movement.IsStrafing = false;
            }
        }

        public void DeactivateSprint()
        {
            if (_movement != null)
            {
                _movement.IsSprinting = false;
                if (_alwaysStrafe)
                    _movement.IsStrafing = true;
            }
        }

        public void ActivateCrouch()
        {
            _crouchKeyPressed = true;

            if (_movement != null && _movement.IsGrounded)
            {
                _movement.CapsuleCrouchingSize(true); // made public below via reflection? If not, movement will manage via input events.
                DeactivateSprint();
                // mark movement crouch flag
                _movement.IsCrouching = true;
            }
        }

        public void DeactivateCrouch()
        {
            _crouchKeyPressed = false;

            if (!_cannotStandUp)
            {
                _movement.CapsuleCrouchingSize(false);
                _movement.IsCrouching = false;
            }
        }
        #endregion

        #region Shared State
        #region State Change
        private void SwitchState(AnimationState newState)
        {
            ExitCurrentState();
            EnterState(newState);
        }

        private void EnterState(AnimationState stateToEnter)
        {
            _currentState = stateToEnter;
            switch (_currentState)
            {
                case AnimationState.Base:
                    EnterBaseState();
                    break;
                case AnimationState.Locomotion:
                    EnterLocomotionState();
                    break;
                case AnimationState.Jump:
                    EnterJumpState();
                    break;
                case AnimationState.Fall:
                    EnterFallState();
                    break;
                case AnimationState.Crouch:
                    EnterCrouchState();
                    break;
            }
        }

        private void ExitCurrentState()
        {
            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    ExitLocomotionState();
                    break;
                case AnimationState.Jump:
                    ExitJumpState();
                    break;
                case AnimationState.Crouch:
                    ExitCrouchState();
                    break;
            }
        }
        #endregion

        #region Updates
        private void Update()
        {
            if (_isPaused) return;
            switch (_currentState)
            {
                case AnimationState.Locomotion:
                    UpdateLocomotionState();
                    break;
                case AnimationState.Jump:
                    UpdateJumpState();
                    break;
                case AnimationState.Fall:
                    UpdateFallState();
                    break;
                case AnimationState.Crouch:
                    UpdateCrouchState();
                    break;
            }
        }

        private void UpdateAnimatorController()
        {
            if (_movement == null || _animator == null) return;

            _animator.SetFloat(_leanValueHash, _leanValue);
            _animator.SetFloat(_headLookXHash, _headLookX);
            _animator.SetFloat(_headLookYHash, _headLookY);
            _animator.SetFloat(_bodyLookXHash, _bodyLookX);
            _animator.SetFloat(_bodyLookYHash, _bodyLookY);

            _animator.SetFloat(_isStrafingHash, _movement.IsStrafing ? 1.0f : 0.0f);

            _animator.SetFloat(_inclineAngleHash, _movement.CurrentGait == GaitState.Idle ? 0f : _movement.Speed2D); // keep incline if needed
            _animator.SetFloat(_moveSpeedHash, _movement.Speed2D);
            _animator.SetInteger(_currentGaitHash, (int)_movement.CurrentGait);

            _animator.SetFloat(_strafeDirectionXHash, _movement.ShuffleDirectionX);
            _animator.SetFloat(_strafeDirectionZHash, _movement.ShuffleDirectionZ);
            _animator.SetFloat(_forwardStrafeHash, _movement.ForwardStrafe);
            _animator.SetFloat(_cameraRotationOffsetHash, _movement.CameraRotationOffset);

            _animator.SetBool(_movementInputHeldHash, _movement.MovementInputHeld);
            _animator.SetBool(_movementInputPressedHash, _movement.MovementInputPressed);
            _animator.SetBool(_movementInputTappedHash, _movement.MovementInputTapped);
            _animator.SetFloat(_shuffleDirectionXHash, _movement.ShuffleDirectionX);
            _animator.SetFloat(_shuffleDirectionZHash, _movement.ShuffleDirectionZ);

            _animator.SetBool(_isTurningInPlaceHash, _movement.IsTurningInPlace);
            _animator.SetBool(_isCrouchingHash, _movement.IsCrouching);

            _animator.SetFloat(_fallingDurationHash, _movement.FallingDuration);
            _animator.SetBool(_isGroundedHash, _movement.IsGrounded);

            _animator.SetBool(_isWalkingHash, _movement.IsWalking);
            _animator.SetBool(_isStoppedHash, _isStopped);

            _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
        }
        #endregion
        #endregion

        #region Base State Helpers
        private void EnterBaseState()
        {
            _previousRotation = transform.forward;
        }

        // input classification is now mostly done in PlayerMovement; these are kept for locomotion-start checks
        private void CalculateInput()
        {
            // we use the movement's input flags for animator purposes
            if (_movement != null)
            {
                _movementInputTapped = _movement.MovementInputTapped;
                _movementInputPressed = _movement.MovementInputPressed;
                _movementInputHeld = _movement.MovementInputHeld;
            }
        }
        #endregion

        #region Locomotion State
        private void EnterLocomotionState()
        {
            _inputReader.onJumpPerformed += LocomotionToJumpState;
        }

        private void UpdateLocomotionState()
        {
            // movement tick will update movement internals
            _movement?.Tick();

            if (_movement != null && !_movement.IsGrounded)
            {
                SwitchState(AnimationState.Fall);
                return;
            }

            if (_movement != null && _movement.IsCrouching)
            {
                SwitchState(AnimationState.Crouch);
                return;
            }

            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(_enableLean, _enableHeadTurn, _enableBodyTurn);

            // use movement values for checks
            CalculateMoveDirectionAndGaitForAnimation();

            CheckIfStarting();
            CheckIfStopped();
            // rotation already handled in movement.FaceMoveDirection inside Tick
            // movement.Move called inside Tick

            UpdateAnimatorController();
        }

        private void ExitLocomotionState()
        {
            _inputReader.onJumpPerformed -= LocomotionToJumpState;
        }

        private void LocomotionToJumpState()
        {
            SwitchState(AnimationState.Jump);
        }
        #endregion

        #region Jump State
        private void EnterJumpState()
        {
            _animator.SetBool(_isJumpingAnimHash, true);
            _movement?.DoJump();
        }

        private void UpdateJumpState()
        {
            // movement handles gravity/apply/move
            _movement?.Tick();

            if (_movement != null && _movement.Velocity.y <= 0f)
            {
                _animator.SetBool(_isJumpingAnimHash, false);
                SwitchState(AnimationState.Fall);
            }

            _movement?.GroundedCheck();
            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            CalculateMoveDirectionAndGaitForAnimation();
            UpdateAnimatorController();
        }

        private void ExitJumpState()
        {
            _animator.SetBool(_isJumpingAnimHash, false);
        }
        #endregion

        #region Fall State
        private void EnterFallState()
        {
            _movement?.ResetFallingDuration();
            if (_movement != null) _movement.Velocity = new Vector3(_movement.Velocity.x, 0f, _movement.Velocity.z);
        }

        private void UpdateFallState()
        {
            _movement?.GroundedCheck();
            CalculateRotationalAdditives(false, _enableHeadTurn, _enableBodyTurn);
            // movement Tick already handles move/apply gravity/rotation
            _movement?.Tick();

            if (_characterController.isGrounded)
            {
                SwitchState(AnimationState.Locomotion);
            }
            UpdateAnimatorController();
        }
        #endregion

        #region Crouch State
        private void EnterCrouchState()
        {
            _inputReader.onJumpPerformed += CrouchToJumpState;
        }

        private void UpdateCrouchState()
        {
            _movement?.GroundedCheck();

            if (_movement != null && !_movement.IsGrounded)
            {
                // force uncrouch & fall
                _movement.IsCrouching = false;
                _movement.CapsuleCrouchingSize(false);
                SwitchState(AnimationState.Fall);
                return;
            }

            // ceiling check: movement has method
            _cannotStandUp = _movement != null && _movement.CeilingHasObstacle();

            if (!_crouchKeyPressed && !_cannotStandUp)
            {
                _movement.IsCrouching = false;
                SwitchToLocomotionState();
                return;
            }

            if (!_movement.IsCrouching)
            {
                _movement.CapsuleCrouchingSize(false);
                SwitchToLocomotionState();
                return;
            }

            CheckEnableTurns();
            CheckEnableLean();
            CalculateRotationalAdditives(false, _enableHeadTurn, false);
            CalculateMoveDirectionAndGaitForAnimation();
            CheckIfStarting();
            CheckIfStopped();
            UpdateAnimatorController();
        }

        private void ExitCrouchState()
        {
            _inputReader.onJumpPerformed -= CrouchToJumpState;
        }

        private void CrouchToJumpState()
        {
            if (!_cannotStandUp)
            {
                _movement.IsCrouching = false;
                SwitchState(AnimationState.Jump);
            }
        }

        private void SwitchToLocomotionState()
        {
            _movement.IsCrouching = false;
            SwitchState(AnimationState.Locomotion);
        }
        #endregion

        #region Utility / Checks used by animator
        private void CalculateMoveDirectionAndGaitForAnimation()
        {
            // compute newDirectionDifferenceAngle for locomotion start logic
            if (_movement != null)
            {
                Vector3 playerForwardVector = transform.forward;
                Vector3 moveDir = _movement.MoveDirection;

                _newDirectionDifferenceAngle = playerForwardVector != moveDir
                    ? Vector3.SignedAngle(playerForwardVector, moveDir, Vector3.up)
                    : 0f;
            }
        }

        private void CheckIfStopped()
        {
            if (_movement == null) return;
            _isStopped = _movement.MoveDirection.magnitude == 0 && _movement.Speed2D < .5f;
        }

        private void CheckIfStarting()
        {
            _locomotionStartTimer = VariableOverrideDelayTimer(_locomotionStartTimer);

            bool isStartingCheck = false;

            if (_locomotionStartTimer <= 0.0f)
            {
                if (_movement != null && _movement.MoveDirection.magnitude > 0.01f && _movement.Speed2D < 1 && !_movement.IsStrafing)
                {
                    isStartingCheck = true;
                }

                if (isStartingCheck)
                {
                    if (!_isStarting)
                    {
                        _locomotionStartDirection = _newDirectionDifferenceAngle;
                        _animator.SetFloat(_locomotionStartDirectionHash, _locomotionStartDirection);
                    }

                    float delayTime = 0.2f;
                    _leanDelay = delayTime;
                    _headLookDelay = delayTime;
                    _bodyLookDelay = delayTime;

                    _locomotionStartTimer = delayTime;
                }
            }
            else
            {
                isStartingCheck = true;
            }

            _isStarting = isStartingCheck;
            _animator.SetBool(_isStartingHash, _isStarting);
        }

        private void UpdateFallingDuration()
        {
            // movement updates falling duration itself
        }

        private void CheckEnableTurns()
        {
            _headLookDelay = VariableOverrideDelayTimer(_headLookDelay);
            _enableHeadTurn = _headLookDelay == 0.0f && !_isStarting;
            _bodyLookDelay = VariableOverrideDelayTimer(_bodyLookDelay);
            _enableBodyTurn = _bodyLookDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        private void CheckEnableLean()
        {
            _leanDelay = VariableOverrideDelayTimer(_leanDelay);
            _enableLean = _leanDelay == 0.0f && !(_isStarting || _isTurningInPlace);
        }

        private void CalculateRotationalAdditives(bool leansActivated, bool headLookActivated, bool bodyLookActivated)
        {
            if (headLookActivated || leansActivated || bodyLookActivated)
            {
                _currentRotation = transform.forward;

                _rotationRate = _currentRotation != _previousRotation
                    ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                    : 0f;
            }
            _initialLeanValue = leansActivated ? _rotationRate : 0f;

            float leanSmoothness = 5;
            float maxLeanRotationRate = 275.0f;

            float currentSprintSpeed = (_movement != null) ? _movement.SprintSpeed : 1f;
            if (currentSprintSpeed <= 0.001f) currentSprintSpeed = 1f; // 0이 되지 않도록 보정

            float referenceValue = (_movement != null) ? _movement.Speed2D / currentSprintSpeed : 0f;
            _leanValue = CalculateSmoothedValue(
                _leanValue,
                _initialLeanValue,
                maxLeanRotationRate,
                leanSmoothness,
                _leanCurve,
                referenceValue,
                true
            );

            float headTurnSmoothness = 5f;

            if (headLookActivated && (_movement != null && _movement.IsTurningInPlace))
            {
                _initialTurnValue = _movement.CameraRotationOffset;
                _headLookX = Mathf.Lerp(_headLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
            }
            else
            {
                _initialTurnValue = headLookActivated ? _rotationRate : 0f;
                _headLookX = CalculateSmoothedValue(
                    _headLookX,
                    _initialTurnValue,
                    maxLeanRotationRate,
                    headTurnSmoothness,
                    _headLookXCurve,
                    _headLookX,
                    false
                );
            }

            float bodyTurnSmoothness = 5f;

            _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;

            _bodyLookX = CalculateSmoothedValue(
                _bodyLookX,
                _initialTurnValue,
                maxLeanRotationRate,
                bodyTurnSmoothness,
                _bodyLookXCurve,
                _bodyLookX,
                false
            );

            float cameraTilt = _cameraController.GetCameraTiltX();
            _headLookY = cameraTilt;
            _bodyLookY = cameraTilt;

            _previousRotation = _currentRotation;
        }

        private float CalculateSmoothedValue(
            float mainVariable,
            float newValue,
            float maxRateChange,
            float smoothness,
            AnimationCurve referenceCurve,
            float referenceValue,
            bool isMultiplier
        )
        {
            float changeVariable = newValue / maxRateChange;

            changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

            if (isMultiplier)
            {
                float multiplier = referenceCurve.Evaluate(referenceValue);
                changeVariable *= multiplier;
            }
            else
            {
                changeVariable = referenceCurve.Evaluate(changeVariable);
            }

            if (!changeVariable.Equals(mainVariable))
            {
                changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
            }

            return changeVariable;
        }

        private float VariableOverrideDelayTimer(float timeVariable)
        {
            if (timeVariable > 0.0f)
            {
                timeVariable -= Time.deltaTime;
                timeVariable = Mathf.Clamp(timeVariable, 0.0f, 1.0f);
            }
            else
            {
                timeVariable = 0.0f;
            }
            return timeVariable;
        }
        #endregion

        #region Attack / Animation control
        public void SetWeaponType(int weaponType)
        {
            _animator.SetInteger(_weaponTypeHash, weaponType);
        }

        public void PlayAttackAnimation(int comboIndex)
        {
            _animator.SetInteger(_comboIndexHash, comboIndex);
            _animator.SetTrigger(_attackHash);
        }

        public int GetInteger(int parameterHash)
        {
            return _animator.GetInteger(parameterHash);
        }

        public void SetComboIndex(int value)
        {
            _animator.SetInteger(_comboIndexHash, value);
        }
        #endregion

        private void OnEnable()
        {
            GameStateManager.OnMainStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            GameStateManager.OnMainStateChanged -= OnGameStateChanged;
        }

        public void Pause()
        {
            _isPaused = true;
            if (_animator != null) _animator.speed = 0f;
        }

        public void Resume()
        {
            _isPaused = false;
            if (_animator != null) _animator.speed = 1f;
        }

        private void OnGameStateChanged(MainGameState state)
        {
            if (state == MainGameState.Gameplay)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
}
