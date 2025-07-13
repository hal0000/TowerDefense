using TowerDefense.Core;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace TowerDefense.Controller
{
    public class CameraTargetController : MonoBehaviour
    {
        [Header("Grid Reference")] [SerializeField]
        private GridManager _gridManager;

        [SerializeField] private GridInputHandler _gridInput;

        [Header("Pan Settings")] [Tooltip("Tek parmakla sürükleme hızı")]
        public float PanSpeedTouch = 0.02f;

        [Header("Zoom Settings")] [Tooltip("İki parmakla pinch zoom hassasiyeti")]
        public float ZoomSpeedTouch = 0.01f;

        public float MinZoom = 2f;
        public float MaxZoom;

        [SerializeField] private CinemachineCamera _cam;
        private bool _isPanning, _isPinching;
        private Vector2 _lastPanPos;
        private float _lastPinchDist;
        private Camera _unityCam;
        private Vector2 _zoomCenterScreen;
        private Vector3 _zoomCenterWorld;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();
            _unityCam = Camera.main;
        }

        private void Start()
        {
            Vector3 p = transform.position;
            p.x = _gridManager.Width * 0.5f;
            p.z = _gridManager.Height * 0.5f;
            transform.position = p;
        }

        private void Update()
        {
            ReadOnlyArray<Touch> touches = Touch.activeTouches;
            if (touches.Count == 2)
            {
                if (EventSystem.current.IsPointerOverGameObject(touches[0].touchId) ||
                    EventSystem.current.IsPointerOverGameObject(touches[1].touchId))
                    return;

                _isPanning = false;
                HandlePinch(touches[0], touches[1]);
                return;
            }

            if (touches.Count == 1)
            {
                if (EventSystem.current.IsPointerOverGameObject(touches[0].touchId))
                    return;
                if (_gridInput != null && _gridInput.CanIMoveCamera) return;
                _isPinching = false;
                HandlePan(touches[0]);
            }
            else
            {
                _isPanning = _isPinching = false;
            }
        }

        private void HandlePan(Touch t)
        {
            switch (t.phase)
            {
                case TouchPhase.Began:
                    _lastPanPos = t.screenPosition;
                    _isPanning = true;
                    break;
                case TouchPhase.Moved:
                    if (!_isPanning) return;
                    Vector2 delta = t.screenPosition - _lastPanPos;
                    Vector3 right = _cam.transform.right;
                    right.y = 0;
                    right.Normalize();
                    Vector3 forward = _cam.transform.forward;
                    forward.y = 0;
                    forward.Normalize();
                    transform.position += (-delta.x * right + -delta.y * forward) * PanSpeedTouch;
                    _lastPanPos = t.screenPosition;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _isPanning = false;
                    ClampPosition();
                    break;
            }
        }

        private void HandlePinch(Touch t0, Touch t1)
        {
            float dist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            Vector2 mid = (t0.screenPosition + t1.screenPosition) * 0.5f;

            if (!_isPinching || t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _isPinching = true;
                _lastPinchDist = dist;
                _zoomCenterScreen = mid;
                _zoomCenterWorld = ScreenToWorldPlane(mid);
                return;
            }

            if (_isPinching && (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
            {
                float delta = dist - _lastPinchDist;
                float newSize = _cam.Lens.OrthographicSize - delta * ZoomSpeedTouch * Time.deltaTime;
                _cam.Lens.OrthographicSize = Mathf.Clamp(newSize, MinZoom, MaxZoom);
                _lastPinchDist = dist;

                // Zoom merkezini sabit tutmak için pozisyon düzelt
                Vector3 newWorld = ScreenToWorldPlane(_zoomCenterScreen);
                Vector3 shift = _zoomCenterWorld - newWorld;
                transform.position += shift;
            }
            else if (t0.phase == TouchPhase.Ended ||
                     t1.phase == TouchPhase.Ended ||
                     t0.phase == TouchPhase.Canceled ||
                     t1.phase == TouchPhase.Canceled)
            {
                _isPinching = false;
                ClampPosition();
            }
        }

        private Vector3 ScreenToWorldPlane(Vector2 screenPos)
        {
            Ray ray = _unityCam.ScreenPointToRay(screenPos);
            float planeY = transform.position.y;
            float t = (planeY - ray.origin.y) / ray.direction.y;
            return ray.origin + ray.direction * t;
        }

        private void ClampPosition()
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, 0f, _gridManager.Width);
            p.z = Mathf.Clamp(p.z, 0f, _gridManager.Height);
            transform.position = p;
        }
    }
}