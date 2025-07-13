using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TowerDefense.Core; // GridManager

namespace TowerDefense.Controller
{
    [RequireComponent(typeof(Camera))]
    public class CameraMovementController : MonoBehaviour
    {
        [Header("Grid Reference")]
        [SerializeField] private GridManager _gridManager;

        [Header("Pan Settings")]
        [Tooltip("Tek parmakla sürükleme hızı")]
        public float panSpeedTouch = 0.01f;

        [Header("Zoom Settings")]
        [Tooltip("İki parmakla pinch zoom hassasiyeti")]
        public float zoomSpeedTouch = 0.5f;
        [Tooltip("Minimum ortographic size")]
        public float minZoom = 2f;
        private float maxZoom;

        private Camera _cam;
        private Vector2 _lastPanPos;

        private bool _isPinching;
        private float _lastPinchDist;
        private Vector2 _zoomCenterScreen;
        private Vector3 _zoomCenterWorld;

        void Awake()
        {
            // Enhanced Touch System’i etkinleştir
            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();

            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
        }

        void Start()
        {
            // Grid boyutuna göre zoom sınırlarını hesapla
            RecalculateZoomLimits();
            // Kamera pozisyonunu clamp’le başlangıçta
            //ClampCameraPosition();
        }

        void Update()
        {
            var touches = Touch.activeTouches;

            if (touches.Count == 1)
            {
                // Tek parmak: pan
                _isPinching = false;
                HandlePan(touches[0]);
            }
            else if (touches.Count == 2)
            {
                // İki parmak: pinch zoom
                HandlePinch(touches[0], touches[1]);
            }
            // Diğer durumlarda modları temizle
        }

        private void HandlePan(Touch t)
        {
            if (t.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _lastPanPos = t.screenPosition;
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 delta = t.screenPosition - _lastPanPos;
                // Ekran delta’sını dünya delta’sına çevir
                Vector3 move = new Vector3(-delta.x * panSpeedTouch, 0f, -delta.y * panSpeedTouch);
                _cam.transform.position += move;
                _lastPanPos = t.screenPosition;
            }
            else if (t.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     t.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                //ClampCameraPosition();
            }
        }

        private void HandlePinch(Touch t0, Touch t1)
        {
            float dist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            Vector2 mid = (t0.screenPosition + t1.screenPosition) * 0.5f;

            if (!_isPinching ||
                t0.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                t1.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // Pinch başlarken referans mesafe ve merkez
                _isPinching = true;
                _lastPinchDist = dist;
                _zoomCenterScreen = mid;
                _zoomCenterWorld = ScreenToWorldPlane(mid);
                return;
            }

            if (_isPinching &&
               (t0.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                t1.phase == UnityEngine.InputSystem.TouchPhase.Moved))
            {
                float delta = dist - _lastPinchDist;
                float newSize = _cam.orthographicSize - delta * zoomSpeedTouch * Time.deltaTime;
                _cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
                _lastPinchDist = dist;

                // Zoom merkezini sabit tutmak için pozisyon düzelt
                Vector3 newWorld = ScreenToWorldPlane(_zoomCenterScreen);
                Vector3 shift = _zoomCenterWorld - newWorld;
                _cam.transform.position += shift;
            }
            else if (t0.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     t1.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     t0.phase == UnityEngine.InputSystem.TouchPhase.Canceled ||
                     t1.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                _isPinching = false;
                //ClampCameraPosition();
            }
        }

        // Ekran koordinatını world üzerindeki grid düzlemine çevirir
        private Vector3 ScreenToWorldPlane(Vector2 screenPos)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);
            float planeY = _cam.transform.position.y;
            float t = (planeY - ray.origin.y) / ray.direction.y;
            return ray.origin + ray.direction * t;
        }

        // Grid boyutuna göre maxZoom değeri hesaplanır
        private void RecalculateZoomLimits()
        {
            // Yarı yükseklik limiti: grid yüksekliğinin yarısı
            float hLimit = _gridManager.Height / 2f;
            // Yarı genişlik limiti: grid genişliği / (2 * aspect)
            float wLimit = _gridManager.Width / (2f * _cam.aspect);
            maxZoom = Mathf.Min(hLimit, wLimit);
        }

        // Kameranın x/z pozisyonunu grid sınırları içinde tutar
        private void ClampCameraPosition()
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            // 0.5f ekleyerek hücre merkezine yaslanıyoruz
            float minX = halfW - 0.5f;
            float maxX = _gridManager.Width - halfW - 0.5f;
            float minZ = halfH - 0.5f;
            float maxZ = _gridManager.Height - halfH - 0.5f;

            Vector3 p = _cam.transform.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.z = Mathf.Clamp(p.z, minZ, maxZ);
            _cam.transform.position = p;
        }
    }
}