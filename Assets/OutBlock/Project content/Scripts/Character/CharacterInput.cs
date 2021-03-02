using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace OutBlock
{

    /// <summary>
    /// Input overlay for the player.
    /// </summary>
    public class CharacterInput : MonoBehaviour
    {

        [SerializeField, Header("Components")]
        private Player player = null;
        [SerializeField]
        private CinemachineVirtualCamera cinemachineCam = null;
        [SerializeField]
        private Camera cam = null;
        [SerializeField, Tooltip("Sensitivity multiplier when aiming.")]
        private float aimingSensitivityMultiplier = 0.75f;
        [SerializeField, Tooltip("Object which camera would follow")]
        private Transform camFollowTarget = null;
        [SerializeField]
        private Vector2 lookAngleRange = new Vector2(-45, 45);
        [SerializeField, Header("Aim"), Tooltip("Field of view offset when aiming, e.g. zoom")]
        private float fovOffset = -15;
        [SerializeField, Tooltip("Additional offset when the player is not holding a gun")]
        private float fovEmptyOffset = -5;
        [SerializeField, Tooltip("Field of view change speed")]
        private float fovSpeed = 5;

        /// <summary>
        /// Is player is busy?
        /// </summary>
        public bool busy { get; set; }

        /// <summary>
        /// Aiming the weapon?
        /// </summary>
        public bool aiming { get; private set; }

        /// <summary>
        /// Firing the weapon?
        /// </summary>
        public bool fire { get; private set; }

        /// <summary>
        /// Reloading the weapon?
        /// </summary>
        public bool reload { get; private set; }

        /// <summary>
        /// Switching weapon?
        /// </summary>
        public float weaponSwitch { get; private set; }

        /// <summary>
        /// Is jump button pressed now?
        /// </summary>
        public bool jump { get; private set; }

        /// <summary>
        /// Is crouching now?
        /// </summary>
        public bool crouchSwitch { get; set; }

        private const float sensitivityCompensation = 0.25f;
        private const float aimCheckTime = 0.1f;

        private float aimCheckTimeout;
        private bool aim;
        private bool oldCrouch;
        private Vector2 look;
        private float startFov;
        private float targetFov;
        private float camYPos;
        private float aimX;
        private float aimY;
        private bool aimingAtEnemy;
        private Cinemachine3rdPersonFollow cinemachineFollow;

        private void Start()
        {
            if (cinemachineCam)
            {
                cinemachineFollow = cinemachineCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                targetFov = startFov = cinemachineCam.m_Lens.FieldOfView;

                camYPos = 0;
            }

            aimX = transform.eulerAngles.y;
        }

        private void OnEnable()
        {
            if (player)
                player.onDeath += OnDeath;

            InputManager.Instance().onCrouchSwitch += CrouchSwitch;
            InputManager.Instance().onSwitchCamera += SwitchCamera;
        }

        private void OnDisable()
        {
            if (player)
                player.onDeath -= OnDeath;

            if (InputManager.Instance(true))
            {
                InputManager.Instance().onCrouchSwitch -= CrouchSwitch;
                InputManager.Instance().onSwitchCamera -= SwitchCamera;
            }
        }

        private void FixedUpdate()
        {
            if (aimCheckTimeout > 0)
            {
                aimCheckTimeout -= Time.fixedDeltaTime;
            }
            else
            {
                CheckAim();
            }
        }

        private void Update()
        {
            if (player.Character == null || GameUI.pauseDelayed)
            {
                return;
            }

            if (player.Dead)
                return;

            if (InteractiveCamera.activeCamera != null)
                return;

            if (DevTools.Instance().Noclip)
            {
                if (player.Character.Active)
                    player.Character.Active = false;
            }
            else
            {
                if (!player.Character.Active && !player.Character.RagdollRoot.RagdollEnabled)
                    player.Character.Active = true;
            }

            Vector2 move;
            bool run, crouch;
            run = crouch = jump = false;

            move = InputManager.Instance().Move;
            run = InputManager.Instance().Run;
            crouch = InputManager.Instance().Crouch;
            if (oldCrouch != crouch)
            {
                oldCrouch = crouch;
                player.PlayerEvents.OnCrouch?.Invoke();
            }
            jump = InputManager.Instance().Jump;
            aim = InputManager.Instance().Aim;
            fire = InputManager.Instance().Fire;
            weaponSwitch = InputManager.Instance().ChangeWeapon;

            aiming = aim || fire;

            if (run && (crouchSwitch || crouch))
            {
                crouchSwitch = false;
                crouch = false;
            }

            #region CameraLook
            look = Vector2.Lerp(look, GetPlayerAim(), Time.deltaTime * 35f);
            float sens = aimingAtEnemy ? Mathf.Clamp(1 - Settings.AimAssistCoefficient, 0.05f, 1) : 1 * (aiming ? aimingSensitivityMultiplier : 1);
            aimX += look.x * sens;
            aimY += look.y * sens;
            aimY = Utils.ClampAngle(aimY, lookAngleRange.x, lookAngleRange.y);

            Vector3 newCamFollowPos = Vector3.zero;
            camYPos = Mathf.Lerp(camYPos, player.Character.Crouching ? 1 : 0, Time.deltaTime * 7);
            newCamFollowPos.y = Mathf.Lerp(1.4f, 0.9f, camYPos);
            if (player.inGrass && player.Character.Crouching)
                newCamFollowPos.y += 0.25f;

            camFollowTarget.localPosition = newCamFollowPos;
            camFollowTarget.eulerAngles = new Vector3(aimY, aimX, 0);
            #endregion

            if (!busy)
            {
                Vector3 forward = cam.transform.forward;
                Vector3 right = cam.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                Vector3 moveDir = right * move.x + forward * move.y;
                if (!DevTools.Instance().Noclip)
                {
                    player.Character.Move(moveDir, run);
                    player.Character.Crouch(crouch || crouchSwitch);
                    if (jump)
                        player.Jump();
                }
                else
                {
                    moveDir = cam.transform.TransformDirection(new Vector3(move.x, 0, move.y));
                    float speed = run ? 9 : 5;
                    moveDir *= speed;
                    if (jump)
                        moveDir += Vector3.up * speed;
                    else if (crouch)
                        moveDir += Vector3.down * speed;
                    Vector3 newPos = transform.position + moveDir * Time.deltaTime;
                    player.Character.SetPosition(newPos);
                }
            }
            else
            {
                player.Character.Move(Vector3.zero, false);
            }

            float newFov = aiming ? startFov + fovOffset : startFov;
            if (aiming && Player.Instance && Player.Instance.GetCurrentWeapon() is Weapon_Empty)
                newFov += fovEmptyOffset;
            targetFov = Mathf.Lerp(targetFov, newFov, Time.deltaTime * fovSpeed);
            cinemachineCam.m_Lens.FieldOfView = targetFov;
        }

        private void CheckAim()
        {
            aimCheckTimeout = aimCheckTime;
            aimingAtEnemy = false;

            if (InteractiveCamera.activeCamera != null)
                return;

            if (!Settings.AimAssist)
                return;

            Weapon currentWeapon = player.GetCurrentWeapon();
            if (currentWeapon is Weapon_Empty || currentWeapon is Weapon_Grenade)
                return;

            if (!player.GetCurrentWeapon().aiming)
                return;

            RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, cam.transform.forward, 50, LayerMask.GetMask("Default", "Entity"));
            hits = hits.OrderBy(x => x.distance).ToArray();
            foreach(RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("Player"))
                {
                    continue;
                }
                else if (hit.transform.CompareTag("Enemy"))
                {
                    aimingAtEnemy = true;
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        private void CrouchSwitch()
        {
            crouchSwitch = !crouchSwitch;
            player.PlayerEvents.OnCrouch?.Invoke();
        }

        private void SwitchCamera()
        {
            cinemachineFollow.CameraSide *= -1;
        }

        private void OnDeath(IDamageable damageable)
        {
            if (cinemachineCam)
            {
                cinemachineCam.LookAt = player.Character.RagdollRoot.transform.GetChild(0);
                cinemachineCam.AddCinemachineComponent<CinemachineHardLookAt>();
            }
        }

        /// <summary>
        /// Returns player mouse/joy aim values
        /// </summary>
        public static Vector2 GetPlayerAim()
        {
            Vector2 result = Vector2.zero;

            Vector2 look = InputManager.Instance().Look;
            float sens = InputManager.joyNow ? Settings.JoySensitivity : Settings.Sensitivity * sensitivityCompensation;
            result.x += look.x * sens;
            result.y -= look.y * sens;

            return result;
        }

    }
}