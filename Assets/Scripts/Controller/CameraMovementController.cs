using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace TowerDefense.Controller
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovementController : MonoBehaviour
    {
        [Header("Pan Settings")]
        public float PanSpeedTouch = 0.01f; // tek parmakla sürükleme hızı (mobil)
        public float PanSpeedMouse = 0.1f; // sağ‐tık sürükleme hızı (PC)

        [Header("Zoom Settings")]
        public float ZoomSpeedTouch = 0.02f; // iki parmak pinch hassasiyeti (mobil)
        public float ZoomSpeedWheel = 1f; // mouse wheel hassasiyeti (PC)
        public float MinZoom = 5f;
        public float MaxZoom = 20f;

        Camera _cam;
        Vector3 _lastPanPos;
        bool _isPanningMouse;
        float _lastPinchDist;

        void Awake()
        {
            // TouchSimulation.Enable(); // mouse/pen → Touchscreen
            // EnhancedTouchSupport.Enable(); // EnhancedTouch API’leriyle de çalışır
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
        }

        void LateUpdate()
        {
            #if UNITY_IOS || UNITY_ANDROID
                // Mobil: tek parmak pan, iki parmak pinch
                if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
                {
                    if (Touchscreen.current.touches.Count == 1)
                        HandleTouchPan();
                    else if (Touchscreen.current.touches.Count == 2)
                        HandlePinchZoom();
                }
                HandleScrollZoom(); // bazı mobil trackpad'ler de scroll gönderebilir

            #elif UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
                // // PC/Editör/WebGL: sağ‐tık drag + scroll wheel
                // HandleMousePan();
                // HandleScrollZoom();
            #endif
        }

        private void HandleMousePan()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _isPanningMouse = true;
                _lastPanPos = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                _isPanningMouse = false;
            }

            if (_isPanningMouse)
            {
                Vector3 curr = Mouse.current.position.ReadValue();
                Vector3 delta = curr - _lastPanPos;
                PanCamera(delta, PanSpeedMouse);
                _lastPanPos = curr;
            }
        }

        private void HandleTouchPan()
        {
            var touch = Touchscreen.current.touches[0];
            var phase = touch.phase.ReadValue();
            Vector2 pos = touch.position.ReadValue();

            if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                _lastPanPos = pos;
            else if (phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector3 delta = (Vector3)pos - _lastPanPos;
                PanCamera(delta, PanSpeedTouch);
                _lastPanPos = pos;
            }
        }

        private void HandlePinchZoom()
        {
            var t0 = Touchscreen.current.touches[0];
            var t1 = Touchscreen.current.touches[1];
            Vector2 p0 = t0.position.ReadValue();
            Vector2 p1 = t1.position.ReadValue();
            float dist = Vector2.Distance(p0, p1);

            if (t1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _lastPinchDist = dist;
                return;
            }

            float delta = dist - _lastPinchDist;
            ZoomCamera(delta, ZoomSpeedTouch);
            _lastPinchDist = dist;
        }

        private void HandleScrollZoom()
        {
            if (Mouse.current == null) return;
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
                ZoomCamera(scroll, ZoomSpeedWheel);
        }

        private void PanCamera(Vector3 delta, float speed)
        {
            Vector3 right = transform.right; right.y = 0;
            Vector3 forward = transform.forward; forward.y = 0;
            Vector3 move = -delta.x * right * speed
                             -delta.y * forward * speed;
            transform.position += move;
        }

        private void ZoomCamera(float delta, float speed)
        {
            float newSize = _cam.orthographicSize - delta * speed;
            _cam.orthographicSize = Mathf.Clamp(newSize, MinZoom, MaxZoom);
        }
    }
}