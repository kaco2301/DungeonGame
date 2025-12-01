using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Kaco.CameraSystem
{
    public enum CameraMode
    {
        ThirdPerson,
        Dialogue
    }

    [System.Serializable]
    public class CameraEntry
    {
        public CameraMode mode;
        public CinemachineCamera virtualCamera;
    }

    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [SerializeField] private GameObject _player;
        [SerializeField] private CinemachineCamera _thirdPersonCamera;
        [SerializeField] private CinemachineOrbitalFollow _orbital;
        [SerializeField] private CinemachineInputAxisController _axisController;

        private Camera _mainCamera;
        private int _playerLayerMask;

        [SerializeField] private List<CameraEntry> cameras; 
        private Dictionary<CameraMode, CinemachineCamera> _virtualCameras = 
            new Dictionary<CameraMode, CinemachineCamera>(); 

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var entry in cameras)
            {
                _virtualCameras[entry.mode] = entry.virtualCamera;
            }

            _playerLayerMask = 1 << LayerMask.NameToLayer("Player");
        }

        private void Start()
        {
            _mainCamera = GetComponent<Camera>();
            _orbital = _orbital.GetComponent<CinemachineOrbitalFollow>();

            SetCameraPriority(CameraMode.ThirdPerson);
        }

        private void OnEnable()
        {
            // GameStateManager의 상태 변경 이벤트를 구독
            GameStateManager.OnMainStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameStateManager.OnMainStateChanged -= OnStateChanged;
        }

        /// <summary>
         /// 게임 상태 변경에 따라 카메라를 전환합니다.
         /// </summary>
        private void OnStateChanged(MainGameState state)
        {
            if (state == MainGameState.Dialogue)
            {
                _mainCamera.cullingMask &= ~_playerLayerMask;
                SetCameraPriority(CameraMode.Dialogue);
                _axisController.enabled = false;
            }
            else if (state == MainGameState.Gameplay)
            {
                _mainCamera.cullingMask |= _playerLayerMask;
                SetCameraPriority(CameraMode.ThirdPerson);
                _axisController.enabled = true;
            }
        }

        public void SetCameraPriority(CameraMode mode)
        {
            // 모든 카메라의 우선순위를 기본값(10)으로 초기화
            foreach (var cam in _virtualCameras.Values)
            {
                cam.Priority = 10;
            }

            // 원하는 카메라의 우선순위만 20으로 높여 활성화
            if (_virtualCameras.ContainsKey(mode))
            {
                _virtualCameras[mode].Priority = 20;
            }
        }

        public void SetCamera(CameraMode mode, Transform positionTarget = null, Transform lookAtTarget = null)
        {
            if (!_virtualCameras.ContainsKey(mode))
            {
                Debug.LogError($"{mode}에 해당하는 Virtual Camera가 등록되지 않았습니다.");
                return;
            }
            var targetCamera = _virtualCameras[mode];

            if (positionTarget != null)
            {
                targetCamera.transform.SetPositionAndRotation(positionTarget.position, positionTarget.rotation);
            }

            if (lookAtTarget != null)
            {
                targetCamera.LookAt = lookAtTarget;
            }

            SetCameraPriority(mode);
        }


        public Vector3 GetCameraPosition()
        {
            return _mainCamera.transform.position;
        }

        public Vector3 GetCameraForward()
        {
            return _mainCamera.transform.forward;
        }

        public Vector3 GetCameraForwardZeroedY()
        {
            return new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z);
        }

        public Vector3 GetCameraForwardZeroedYNormalized()
        {
            return GetCameraForwardZeroedY().normalized;
        }

        public Vector3 GetCameraRightZeroedY()
        {
            return new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z);
        }

        public Vector3 GetCameraRightZeroedYNormalized()
        {
            return GetCameraRightZeroedY().normalized;
        }

        /// <summary>
        /// 캐릭터의 위 아래 얼굴 각도 조절
        /// </summary>
        public float GetCameraTiltX()
        {
            float min = _orbital.VerticalAxis.Range.x;
            float max = _orbital.VerticalAxis.Range.y;
            float value = _orbital.VerticalAxis.Value;
            float normalized = 1f - Mathf.InverseLerp(min, max, value);
            float cameraTilt = Mathf.Lerp(-0.1f, 0.45f, normalized);
            return cameraTilt;
        }
    }
}