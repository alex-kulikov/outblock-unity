using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Interactive camera on the level.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class InteractiveCamera : MonoBehaviour
    {

        [SerializeField]
        private Camera cam = null;
        [SerializeField]
        private Vector2 anglesLimits = new Vector2(40, 60);
        [SerializeField]
        private float zoomFovMultiplier = 2;

        private float initFov;
        private float angleX, angleY;
        private Vector3 startRot;

        /// <summary>
        /// Active camera now. Can be nulled.
        /// </summary>
        public static InteractiveCamera activeCamera { get; private set; } = null;
        
        private bool active = false;
        /// <summary>
        /// Is this camera active?
        /// </summary>
        public bool Active
        {
            get => active;
            private set
            {
                if (active == value)
                    return;

                active = value;

                if (Player.Instance)
                {
                    if (active)
                    {
                        Player.Instance.onDeath += OnPlayerDied;
                        InputManager.Instance().onAction += OnAction;
                    }
                    else
                    {
                        Player.Instance.onDeath -= OnPlayerDied;
                        InputManager.Instance().onAction -= OnAction;
                    }
                    Player.Instance.busy = active;
                }

                activeCamera = active ? this : null;

                GameUI.Instance().SetCameraUI(active);

                cam.enabled = active;
            }
        }

        private void Awake()
        {
            cam.enabled = false;
            startRot = transform.localEulerAngles;
            angleX = transform.localEulerAngles.x;
            angleY = transform.localEulerAngles.y;
            initFov = cam.fieldOfView;
        }

        private void OnDisable()
        {
            if (InputManager.Instance(true))
                InputManager.Instance().onAction -= OnAction;

            if (Player.Instance)
                Player.Instance.onDeath -= OnPlayerDied;
        }

        private void Update()
        {
            if (!Active)
                return;

            Vector2 look = CharacterInput.GetPlayerAim();
            angleX += look.y;
            angleY += look.x;
            angleX = Utils.ClampAngle(angleX, -anglesLimits.x + startRot.x, anglesLimits.x + startRot.x);
            angleY = Utils.ClampAngle(angleY, -anglesLimits.y + startRot.y, anglesLimits.y + startRot.y);
            transform.localEulerAngles = new Vector3(angleX, angleY, 0);

            bool aiming = InputManager.Instance().Aim;
            cam.fieldOfView = initFov * (aiming ? 1f / zoomFovMultiplier : 1);
        }

        private void OnDrawGizmosSelected()
        {
            float aspectRatio = anglesLimits.x / anglesLimits.y;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, aspectRatio, 1));
            Gizmos.color = Color.red;
            Gizmos.DrawFrustum(Vector3.zero, anglesLimits.y, 10, 0, 1);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward * 2);
        }

        private void OnAction()
        {
            if (Active)
                Active = false;
        }

        private void OnPlayerDied(IDamageable damageable)
        {
            Active = false;
        }

        /// <summary>
        /// Activate this camera.
        /// </summary>
        public void Activate()
        {
            Active = true;
        }

    }
}