using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pact.VirtualTour
{
    [DefaultExecutionOrder(-100)]
    public class PactTourInput : MonoBehaviour
    {
        public Camera viewCamera;
        public PactTourController tour;
        public Material pointerMaterial;
        [Tooltip("Alternativa sem comandos: olhar fixamente para um marcador.")]
        public bool gazeSelection = true;
        public float gazeSeconds = 2;
        readonly List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        readonly HashSet<XRInputSubsystem> configured = new HashSet<XRInputSubsystem>();
        PactTarget hovered, lastGazeSelection;
        LineRenderer laser;
        Transform reticle;
        float dwell, yaw, pitch;
        bool held, previousHeld, wasXR;
        float nextSubsystemCheck;
        float nextSelectionTime;
        Vector3 trackingOffset;

        void Start()
        {
            var go = new GameObject("Apontador"); go.transform.SetParent(transform, false);
            laser = go.AddComponent<LineRenderer>(); laser.positionCount = 2;
            laser.startWidth = .0025f; laser.endWidth = .0015f;
            laser.sharedMaterial = pointerMaterial; laser.enabled = false;
            reticle = PactVisuals.Shape(PrimitiveType.Sphere, transform, "Cursor", Vector3.zero,
                Vector3.one * .019f, pointerMaterial).transform;
            Application.onBeforeRender += UpdateHead;
        }
        bool XRActive()
        {
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            return head.isValid && head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) && tracked;
        }
        void ConfigureTracking()
        {
            if (Time.unscaledTime < nextSubsystemCheck) return;
            nextSubsystemCheck = Time.unscaledTime + 1;
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var sub in subsystems)
                if (sub.running && !configured.Contains(sub))
                {
                    sub.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
                    configured.Add(sub);
                }
            trackingOffset = Vector3.zero;
            foreach (var sub in subsystems)
                if (sub.running && sub.GetTrackingOriginMode() != TrackingOriginModeFlags.Floor)
                    trackingOffset = Vector3.up * 1.6f;
        }
        void UpdateHead()
        {
            if (!viewCamera || !XRActive()) return;
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 p))
                viewCamera.transform.localPosition = p + trackingOffset;
            if (head.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion q))
                viewCamera.transform.localRotation = q;
        }
        void Update()
        {
            if (!viewCamera || !tour || !reticle) return;
            ConfigureTracking();
            bool xr = XRActive();
            if (xr != wasXR) { ClearHover(); previousHeld = false; wasXR = xr; }
            Ray ray;
            bool controller = false;
            held = false;
            if (xr)
            {
                UpdateHead();
                controller = ControllerRay(XRNode.RightHand, out ray);
                if (!controller) controller = ControllerRay(XRNode.LeftHand, out ray);
                if (!controller) ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            }
            else
            {
                viewCamera.transform.localPosition = new Vector3(0, 1.6f, 0);
                Desktop(out ray);
            }
            laser.enabled = xr && controller;
            PactTarget target = null;
            Vector3 end = ray.GetPoint(4);
            if (Physics.Raycast(ray, out RaycastHit hit, 20))
            {
                end = hit.point;
                target = hit.collider.GetComponentInParent<PactTarget>();
            }
            if (tour.Busy || Time.unscaledTime < nextSelectionTime) target = null;
            if (target != lastGazeSelection) lastGazeSelection = null;
            if (target != hovered)
            {
                ClearHover(); hovered = target;
                if (hovered) hovered.Highlight(true);
            }
            bool click = held && !previousHeld;
            previousHeld = held;
            if (xr && !controller && gazeSelection && hovered && hovered != lastGazeSelection)
            {
                dwell += Time.unscaledDeltaTime;
                if (dwell >= Mathf.Max(.5f, gazeSeconds)) { click = true; lastGazeSelection = hovered; }
            }
            laser.SetPosition(0, ray.origin); laser.SetPosition(1, end);
            reticle.position = end;
            reticle.localScale = Vector3.one * (hovered ? .012f : .006f);
            reticle.gameObject.SetActive(!tour.Busy && (xr || hovered));
            if (click && hovered) { var selection = hovered; ClearHover(); nextSelectionTime=Time.unscaledTime+.75f; selection.Activate(); }
        }
        bool ControllerRay(XRNode hand, out Ray ray)
        {
            var device = InputDevices.GetDeviceAtXRNode(hand);
            ray = default;
            if (!device.isValid || !device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked ||
                !device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 p) ||
                !device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion q)) return false;
            ray = new Ray(transform.TransformPoint(p + trackingOffset), transform.TransformDirection(q * Vector3.forward));
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out held);
            return true;
        }
        void Desktop(out Ray ray)
        {
            Vector2 delta = Vector2.zero;
            Vector2 position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            bool rotate = false, replay = false, stop = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                rotate = Mouse.current.rightButton.isPressed;
                delta = Mouse.current.delta.ReadValue() * .13f;
                held = Mouse.current.leftButton.isPressed;
            }
            if (Keyboard.current != null)
            {
                replay = Keyboard.current.rKey.wasPressedThisFrame;
                stop = Keyboard.current.mKey.wasPressedThisFrame;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            position = UnityEngine.Input.mousePosition;
            rotate = UnityEngine.Input.GetMouseButton(1);
            delta = new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y")) * 2;
            held = UnityEngine.Input.GetMouseButton(0);
            replay = UnityEngine.Input.GetKeyDown(KeyCode.R);
            stop = UnityEngine.Input.GetKeyDown(KeyCode.M);
#endif
            if (rotate) { yaw += delta.x; pitch = Mathf.Clamp(pitch - delta.y, -80, 80); }
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
            if (replay) tour.Replay();
            if (stop) tour.StopVoice();
            ray = viewCamera.ScreenPointToRay(position);
        }
        void ClearHover() { if (hovered) hovered.Highlight(false); hovered = null; dwell = 0; }
        void OnDisable() { ClearHover(); if (laser) laser.enabled = false; }
        void OnDestroy() { Application.onBeforeRender -= UpdateHead; }
    }
}
