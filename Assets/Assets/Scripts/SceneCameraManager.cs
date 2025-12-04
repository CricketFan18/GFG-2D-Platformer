using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

namespace Camera_AP
{
    public class SceneCameraManager : MonoBehaviour
    {
        // =====================================================================
        // ENUMS
        // =====================================================================
        public enum CameraMode
        {
            Follow,
            CenterBetweenTargets,
            CombatZoomOut,
            Cinematic,
            LockOn
        }

        // =====================================================================
        // CAMERA DATA
        // =====================================================================
        [System.Serializable]
        public class CameraData
        {
            [Header("Cinemachine Camera + Target")]
            public CinemachineCamera vCam;
            public Transform target;
            public Transform secondaryTarget;

            [Header("Deadzone Settings")]
            public float deadZoneX = 2f;
            public float deadZoneY = 1.5f;

            [Header("Bounds Collider (Disabled)")]
            public Collider2D boundsCollider;

            [Header("Collision Physics")]
            public LayerMask collisionMask;
            public float collisionRadius = 0.3f;

            [Header("Camera Rotation")]
            public float rotationSpeed = 180f;
            public float maxRotationAngle = 10f;

            [Header("Dynamic Zoom Settings")]
            public float minZoom = 6f;
            public float maxZoom = 14f;
            public float zoomLerpSpeed = 4f;

            [Header("Camera Shake")]
            public float shakeFrequency = 5f;

            [HideInInspector] public CinemachineConfiner2D confiner;
            [HideInInspector] public CinemachinePositionComposer composer;
            [HideInInspector] public CinemachineBasicMultiChannelPerlin noise;

            [HideInInspector] public CameraMode mode = CameraMode.Follow;
            [HideInInspector] public Transform lockOnTarget;
        }

        // Primary & secondary cameras (dual camera support)
        public CameraData camA;
        public CameraData camB;

        // =====================================================================
        // UNITY
        // =====================================================================
        private void Start()
        {
            InitCamera(camA);
            InitCamera(camB);
        }

        private void LateUpdate()
        {
            UpdateCamera(camA);
            UpdateCamera(camB);
        }

        // =====================================================================
        // INITIALIZATION
        // =====================================================================
        private void InitCamera(CameraData cam)
        {
            if (cam == null || cam.vCam == null) return;

            // Confiner
            cam.confiner = cam.vCam.GetComponent<CinemachineConfiner2D>();
            if (!cam.confiner)
                cam.confiner = cam.vCam.gameObject.AddComponent<CinemachineConfiner2D>();

            if (cam.boundsCollider != null)
            {
                cam.confiner.BoundingShape2D = cam.boundsCollider;
                DisableColliderPhysics(cam.boundsCollider);
            }

            cam.composer = cam.vCam.GetComponentInChildren<CinemachinePositionComposer>();
            cam.noise = cam.vCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();

            if (cam.noise != null)
            {
                cam.noise.AmplitudeGain = 0;
                cam.noise.FrequencyGain = 0;
            }
        }

        private void DisableColliderPhysics(Collider2D col)
        {
            col.enabled = false;
            col.isTrigger = true;

            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = false;
            }
        }

        // =====================================================================
        // UPDATE CAMERA
        // =====================================================================
        private void UpdateCamera(CameraData cam)
        {
            if (cam == null || cam.vCam == null || cam.target == null)
                return;

            Vector3 desired = cam.target.position;

            // ---------------------------------------------------------
            // CAMERA MODES
            // ---------------------------------------------------------
            switch (cam.mode)
            {
                case CameraMode.Follow:
                    desired = cam.target.position;
                    break;

                case CameraMode.CenterBetweenTargets:
                    if (cam.secondaryTarget)
                        desired = (cam.target.position + cam.secondaryTarget.position) * 0.5f;
                    break;

                case CameraMode.CombatZoomOut:
                    ZoomTo(cam, cam.maxZoom);
                    break;

                case CameraMode.Cinematic:
                    cam.deadZoneX = 0;
                    cam.deadZoneY = 0;
                    desired += new Vector3(Mathf.Sin(Time.time * .2f) * 2f, 1f);
                    break;

                case CameraMode.LockOn:
                    if (cam.lockOnTarget)
                    {
                        desired = (cam.target.position + cam.lockOnTarget.position) / 2f;
                        ZoomToTargets(cam, cam.target, cam.lockOnTarget);
                    }
                    break;
            }

            // Apply deadzone
            desired = ApplyDeadzone(cam, desired);

            // Pre-collision physics
            desired = ApplyCollision(cam, desired);

            // Push offset to Cinemachine
            cam.composer.TargetOffset = new Vector2(
                desired.x - cam.target.position.x,
                desired.y - cam.target.position.y
            );

            // Handle rotation
            ApplyRotation(cam);
        }

        // =====================================================================
        // DEADZONE
        // =====================================================================
        private Vector3 ApplyDeadzone(CameraData cam, Vector3 desired)
        {
            Vector3 camPos = cam.vCam.transform.position;

            float dx = desired.x - camPos.x;
            float dy = desired.y - camPos.y;

            if (Mathf.Abs(dx) > cam.deadZoneX)
                camPos.x = desired.x - Mathf.Sign(dx) * cam.deadZoneX;

            if (Mathf.Abs(dy) > cam.deadZoneY)
                camPos.y = desired.y - Mathf.Sign(dy) * cam.deadZoneY;

            return camPos;
        }

        // =====================================================================
        // COLLISION
        // =====================================================================
        private Vector3 ApplyCollision(CameraData cam, Vector3 desired)
        {
            Vector3 origin = cam.target.position;
            Vector3 dir = desired - origin;

            float dist = dir.magnitude;
            if (dist < 0.01f) return desired;

            RaycastHit2D hit = Physics2D.CircleCast(
                origin,
                cam.collisionRadius,
                dir.normalized,
                dist,
                cam.collisionMask
            );

            if (hit.collider != null)
                return (Vector3)hit.point - dir.normalized * cam.collisionRadius;

            return desired;
        }

        // =====================================================================
        // CAMERA ROTATION
        // =====================================================================
        private void ApplyRotation(CameraData cam)
        {
            if (cam.mode == CameraMode.Cinematic)
            {
                float angle = Mathf.Sin(Time.time * 0.5f) * cam.maxRotationAngle;
                RotateTo(cam, angle);
            }
        }

        private void RotateTo(CameraData cam, float angle)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            cam.vCam.transform.rotation = Quaternion.Slerp(
                cam.vCam.transform.rotation,
                rot,
                Time.deltaTime * cam.rotationSpeed
            );
        }

        // =====================================================================
        // CAMERA SHAKE
        // =====================================================================
        public void Shake(CameraData cam, float intensity, float duration)
        {
            StartCoroutine(ShakeRoutine(cam, intensity, duration));
        }

        private System.Collections.IEnumerator ShakeRoutine(CameraData cam, float intensity, float duration)
        {
            if (cam.noise == null) yield break;

            cam.noise.AmplitudeGain = intensity;
            cam.noise.FrequencyGain = cam.shakeFrequency;

            yield return new WaitForSeconds(duration);

            cam.noise.AmplitudeGain = 0;
            cam.noise.FrequencyGain = 0;
        }

        // =====================================================================
        // PUBLIC DYNAMIC ZOOM FUNCTIONS
        // =====================================================================

        /// <summary> Directly sets camera zoom. </summary>
        public void SetZoom(CameraData cam, float zoom)
        {
            zoom = Mathf.Clamp(zoom, cam.minZoom, cam.maxZoom);
            cam.vCam.Lens.OrthographicSize = zoom;
        }

        /// <summary> Smoothly zooms to a target level. </summary>
        public void ZoomTo(CameraData cam, float zoom)
        {
            zoom = Mathf.Clamp(zoom, cam.minZoom, cam.maxZoom);
            cam.vCam.Lens.OrthographicSize = Mathf.Lerp(
                cam.vCam.Lens.OrthographicSize,
                zoom,
                Time.deltaTime * cam.zoomLerpSpeed
            );
        }

        /// <summary> Zooms based on player speed. </summary>
        public void ZoomBasedOnSpeed(CameraData cam, float speed)
        {
            float t = Mathf.InverseLerp(0, 20f, speed);
            float zoom = Mathf.Lerp(cam.minZoom, cam.maxZoom, t);
            ZoomTo(cam, zoom);
        }

        /// <summary> Zooms out until both targets fit on screen. </summary>
        public void ZoomToTargets(CameraData cam, Transform a, Transform b)
        {
            float distance = Vector2.Distance(a.position, b.position);
            float zoom = Mathf.Lerp(cam.minZoom, cam.maxZoom, distance / 20f);
            ZoomTo(cam, zoom);
        }

        // =====================================================================
        // PUBLIC LOCK-ON FUNCTIONS
        // =====================================================================

        public void EnableLockOn(CameraData cam, Transform target)
        {
            cam.lockOnTarget = target;
            cam.mode = CameraMode.LockOn;
        }

        public void DisableLockOn(CameraData cam)
        {
            cam.lockOnTarget = null;
            cam.mode = CameraMode.Follow;
        }

        public bool IsLockedOn(CameraData cam)
        {
            return cam.mode == CameraMode.LockOn && cam.lockOnTarget != null;
        }

        // =====================================================================
        // DEBUG VISUALS
        // =====================================================================
        private void OnDrawGizmos()
        {
            DrawGizmos(camA);
            DrawGizmos(camB);
        }

        private void DrawGizmos(CameraData cam)
        {
            if (cam == null || cam.target == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cam.target.position, cam.collisionRadius);

            if (cam.boundsCollider)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(
                    cam.boundsCollider.bounds.center,
                    cam.boundsCollider.bounds.size
                );
            }
        }
    }
}