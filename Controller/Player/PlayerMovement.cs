using Kaco.CameraSystem;
using Kaco.InputSystem;
using UnityEngine;

namespace Kaco.Player
{
    public enum GaitState
    {
        Idle,
        Walk,
        Run,
        Sprint
    }
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        // Exposed to inspector (copied/kept from original so behavior 그대로)
        [Header("External Components")]
        [SerializeField] private CameraManager _cameraController;

        [Header("Movement")]
        [SerializeField] private float _speedChangeDamping = 10f;
        [SerializeField] private float _rotationSmoothing = 10f;
        [SerializeField] private float _buttonHoldThreshold = 0.15f;

        [Header("Capsule")]
        [SerializeField] private float _capsuleStandingHeight = 1.8f;
        [SerializeField] private float _capsuleStandingCentre = 0.93f;
        [SerializeField] private float _capsuleCrouchingHeight = 1.2f;
        [SerializeField] private float _capsuleCrouchingCentre = 0.6f;

        [Header("Strafing")]
        [SerializeField] private float _forwardStrafeMinThreshold = -55.0f;
        [SerializeField] private float _forwardStrafeMaxThreshold = 125.0f;
        [SerializeField] private float _forwardStrafe = 1f;

        [Header("Ground")]
        [SerializeField] private Transform _rearRayPos;
        [SerializeField] private Transform _frontRayPos;
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private float _inclineAngle;
        [SerializeField] private float _groundedOffset = -0.14f;

        [Header("In-Air")]
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _gravityMultiplier = 2f;

        // internal references
        private InputReader _inputReader;
        private CharacterController _characterController; 
        
        private PlayerStat _playerStat;

        private float _walkSpeed ;
        private float _runSpeed ;
        private float _sprintSpeed ;
        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float SprintSpeed => _sprintSpeed;

        // PUBLIC PROPERTIES (animation controller or others will read)
        public Vector3 MoveDirection { get; private set; }
        public Vector3 Velocity;
        public float Speed2D { get;  set; }
        public bool IsGrounded { get;  set; }
        public bool IsCrouching { get;  set; }
        public bool IsSprinting { get;  set; }
        public bool IsWalking { get;  set; }
        public bool IsStrafing { get;  set; }
        public GaitState CurrentGait { get; private set; }

        // animator-usable inputs
        public bool MovementInputTapped { get; private set; }
        public bool MovementInputPressed { get; private set; }
        public bool MovementInputHeld { get; private set; }
        public float ShuffleDirectionX { get; private set; }
        public float ShuffleDirectionZ { get; private set; }
        public float ForwardStrafe => _forwardStrafe;
        public float CameraRotationOffset { get; private set; }
        public bool IsTurningInPlace { get; private set; }

        // falling
        public float FallingDuration { get; private set; }

        // inner variables
        private float _currentMaxSpeed;
        private Vector3 _targetVelocity;
        private float _targetMaxSpeed;
        private float _fallStartTime;
        private float _speed2DRounded;

        private void Awake()
        {
            _inputReader = GetComponent<InputReader>();
            _characterController = GetComponent<CharacterController>();
            _playerStat = GetComponent<PlayerStat>();

            // default strafing
            IsStrafing = true; // default true like original's _alwaysStrafe = true in animator.
        }

        private void Start()
        {
            UpdateMoveSpeeds(_playerStat.CurrentResult);
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.onSprintActivated += OnSprintActivated;
                _inputReader.onSprintDeactivated += OnSprintDeactivated;
                _inputReader.onCrouchActivated += OnCrouchActivated;
                _inputReader.onCrouchDeactivated += OnCrouchDeactivated;
            }
            if (_playerStat != null)
                _playerStat.OnStatsUpdated += UpdateMoveSpeeds;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.onSprintActivated -= OnSprintActivated;
                _inputReader.onSprintDeactivated -= OnSprintDeactivated;
                _inputReader.onCrouchActivated -= OnCrouchActivated;
                _inputReader.onCrouchDeactivated -= OnCrouchDeactivated;
            }
            if (_playerStat != null)
                _playerStat.OnStatsUpdated -= UpdateMoveSpeeds;
        }

        private void UpdateMoveSpeeds(StatResult result)
        {
            Stat stat = result.FinalStat;

            _walkSpeed = stat.MoveSpeed;
            _runSpeed = stat.RunSpeed;
            _sprintSpeed = stat.SprintSpeed;
        }

        private void OnSprintActivated()
        {
            if (!IsCrouching)
            {
                IsWalking = false;
                IsSprinting = true;
                IsStrafing = false;
            }
        }

        private void OnSprintDeactivated()
        {
            IsSprinting = false;

            // if you want to base on some PlayerController flags for locked/on or always strafe,
            // keep it simple: restore strafing
            IsStrafing = true;
        }

        private void OnCrouchActivated()
        {
            if (IsGrounded)
            {
                CapsuleCrouchingSize(true);
                OnSprintDeactivated();
                IsCrouching = true;
            }
        }

        public void OnCrouchDeactivated()
        {
            // try to stand up if there's room
            if (!CeilingHasObstacle())
            {
                CapsuleCrouchingSize(false);
                IsCrouching = false;
            }
        }

        // Public API to be used by animation controller for Jump
        public void DoJump()
        {
            Velocity = new Vector3(Velocity.x, _jumpForce, Velocity.z);
        }

        // Called each frame from animation controller
        public void Tick()
        {
            GroundedCheck(); // set IsGrounded and InclineAngle
            ReadInputAndCalculateDirection();
            CalculateSpeedAndVelocity();
            FaceMoveDirection();
            ApplyGravity(); // apply vertical gravity
            // Move must be called from here or separately; we expose Move() so animation controller may call Move() at its timing.
            Move();
            UpdateFallingDurationIfNeeded();
        }

        private void ReadInputAndCalculateDirection()
        {
            // Input classification (tap/press/hold)
            if (_inputReader._movementInputDetected)
            {
                if (_inputReader._movementInputDuration == 0)
                {
                    MovementInputTapped = true;
                    MovementInputPressed = false;
                    MovementInputHeld = false;
                }
                else if (_inputReader._movementInputDuration > 0 && _inputReader._movementInputDuration < _buttonHoldThreshold)
                {
                    MovementInputTapped = false;
                    MovementInputPressed = true;
                    MovementInputHeld = false;
                }
                else
                {
                    MovementInputTapped = false;
                    MovementInputPressed = false;
                    MovementInputHeld = true;
                }
                _inputReader._movementInputDuration += Time.deltaTime;
            }
            else
            {
                _inputReader._movementInputDuration = 0f;
                MovementInputTapped = MovementInputPressed = MovementInputHeld = false;
            }

            // Move direction in world (camera relative)
            MoveDirection = (_cameraController.GetCameraForwardZeroedYNormalized() * _inputReader._moveComposite.y)
                          + (_cameraController.GetCameraRightZeroedYNormalized() * _inputReader._moveComposite.x);
        }

        private void CalculateSpeedAndVelocity()
        {
            if (!IsGrounded)
            {
                _targetMaxSpeed = _currentMaxSpeed;
            }
            else if (IsCrouching)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else if (IsSprinting)
            {
                _targetMaxSpeed = _sprintSpeed;
            }
            else if (IsWalking)
            {
                _targetMaxSpeed = _walkSpeed;
            }
            else
            {
                _targetMaxSpeed = _runSpeed;
            }

            _currentMaxSpeed = Mathf.Lerp(_currentMaxSpeed, _targetMaxSpeed, 5f * Time.deltaTime); // slightly different damping param - ok
            _targetVelocity.x = MoveDirection.x * _currentMaxSpeed;
            _targetVelocity.z = MoveDirection.z * _currentMaxSpeed;

            Velocity.z = Mathf.Lerp(Velocity.z, _targetVelocity.z, _speedChangeDamping * Time.deltaTime);
            Velocity.x = Mathf.Lerp(Velocity.x, _targetVelocity.x, _speedChangeDamping * Time.deltaTime);

            Speed2D = new Vector3(Velocity.x, 0f, Velocity.z).magnitude;
            Speed2D = Mathf.Round(Speed2D * 1000f) / 1000f;

            CalculateGait();
        }

        private void CalculateGait()
        {
            float runThreshold = (_walkSpeed + _runSpeed) / 2f;
            float sprintThreshold = (_runSpeed + _sprintSpeed) / 2f;

            if (Speed2D < 0.01f)
                CurrentGait = GaitState.Idle;
            else if (Speed2D < runThreshold)
                CurrentGait = GaitState.Walk;
            else if (Speed2D < sprintThreshold)
                CurrentGait = GaitState.Run;
            else
                CurrentGait = GaitState.Sprint;
        }

        private void FaceMoveDirection()
        {
            Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 directionForward = new Vector3(MoveDirection.x, 0f, MoveDirection.z).normalized;

            Vector3 cameraForward = _cameraController.GetCameraForwardZeroedYNormalized();
            Quaternion strafingTargetRotation = Quaternion.identity;
            if (cameraForward != Vector3.zero)
                strafingTargetRotation = Quaternion.LookRotation(cameraForward);

            float strafeAngle = characterForward != directionForward ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up) : 0f;
            IsTurningInPlace = false;

            if (IsStrafing)
            {
                if (MoveDirection.magnitude > 0.01f)
                {
                    if (cameraForward != Vector3.zero)
                    {
                        // shuffle
                        ShuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                        ShuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                        UpdateStrafeDirection(ShuffleDirectionZ, ShuffleDirectionX);
                        CameraRotationOffset = Mathf.Lerp(CameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                        float targetValue = strafeAngle > _forwardStrafeMinThreshold && strafeAngle < _forwardStrafeMaxThreshold ? 1f : 0f;

                        if (Mathf.Abs(_forwardStrafe - targetValue) <= 0.001f)
                        {
                            _forwardStrafe = targetValue;
                        }
                        else
                        {
                            float t = Mathf.Clamp01(20f * Time.deltaTime);
                            _forwardStrafe = Mathf.SmoothStep(_forwardStrafe, targetValue, t);
                        }
                    }

                    transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
                }
                else
                {
                    UpdateStrafeDirection(1f, 0f);

                    float t = 20f * Time.deltaTime;
                    float newOffset = 0f;

                    if (characterForward != cameraForward)
                    {
                        newOffset = Vector3.SignedAngle(characterForward, cameraForward, Vector3.up);
                    }

                    CameraRotationOffset = Mathf.Lerp(CameraRotationOffset, newOffset, t);

                    if (Mathf.Abs(CameraRotationOffset) > 10f)
                    {
                        IsTurningInPlace = true;
                    }
                }
            }
            else
            {
                UpdateStrafeDirection(1f, 0f);
                CameraRotationOffset = Mathf.Lerp(CameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                ShuffleDirectionZ = 1f;
                ShuffleDirectionX = 0f;

                Vector3 faceDirection = new Vector3(Velocity.x, 0f, Velocity.z);

                if (faceDirection == Vector3.zero)
                    return;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(faceDirection), _rotationSmoothing * Time.deltaTime);
            }
        }

        private void UpdateStrafeDirection(float targetZ, float targetX)
        {
            float damp = 5f;
            ShuffleDirectionZ = Mathf.Lerp(ShuffleDirectionZ, targetZ, damp * Time.deltaTime);
            ShuffleDirectionX = Mathf.Lerp(ShuffleDirectionX, targetX, damp * Time.deltaTime);
            ShuffleDirectionZ = Mathf.Round(ShuffleDirectionZ * 1000f) / 1000f;
            ShuffleDirectionX = Mathf.Round(ShuffleDirectionX * 1000f) / 1000f;
        }

        private void ApplyGravity()
        {
            if (Velocity.y > Physics.gravity.y)
            {
                Velocity += Physics.gravity * _gravityMultiplier * Time.deltaTime;
            }
        }

        public void Move()
        {
            _characterController.Move(Velocity * Time.deltaTime);
        }

        #region Ground/Ceiling/Incline

        public void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(
                _characterController.transform.position.x,
                _characterController.transform.position.y - _groundedOffset,
                _characterController.transform.position.z
            );
            IsGrounded = Physics.CheckSphere(spherePosition, _characterController.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);

            if (IsGrounded)
               GroundInclineCheck();
        }

        private void GroundInclineCheck()
        {
            float rayDistance = Mathf.Infinity;
            _rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
            _frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);

            Physics.Raycast(_rearRayPos.position, _rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, _groundLayerMask);
            Physics.Raycast(_frontRayPos.position, _frontRayPos.TransformDirection(-Vector3.up), out RaycastHit frontHit, rayDistance, _groundLayerMask);

            Vector3 hitDifference = frontHit.point - rearHit.point;
            float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

            _inclineAngle = Mathf.Lerp(_inclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
            
        }

        public bool CeilingHasObstacle()
        {
            float rayDistance = Mathf.Infinity;
            float minimumStandingHeight = _capsuleStandingHeight - _frontRayPos.localPosition.y;

            Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + _frontRayPos.localPosition.y, transform.position.z);
            if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, _groundLayerMask))
            {
                return ceilingHit.distance < minimumStandingHeight;
            }
            return false;
        }

        public void CapsuleCrouchingSize(bool crouching)
        {
            if (crouching)
            {
                _characterController.center = new Vector3(0f, _capsuleCrouchingCentre, 0f);
                _characterController.height = _capsuleCrouchingHeight;
            }
            else
            {
                _characterController.center = new Vector3(0f, _capsuleStandingCentre, 0f);
                _characterController.height = _capsuleStandingHeight;
            }
        }

        #endregion

        #region Falling Duration
        public void ResetFallingDuration()
        {
            _fallStartTime = Time.time;
            FallingDuration = 0f;
        }

        private void UpdateFallingDurationIfNeeded()
        {
            if (!_characterController.isGrounded)
            {
                FallingDuration = Time.time - _fallStartTime;
            }
        }
        #endregion
    }
}
