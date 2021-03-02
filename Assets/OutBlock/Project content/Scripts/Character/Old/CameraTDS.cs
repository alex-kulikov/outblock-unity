using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    public class CameraTDS : MonoBehaviour
    {

        [Header("Components")]
        public Camera cam;
        [Tooltip("Camera target object")]
        public Player target;
        public Transform pivot;
        [Header("Stats")]
        public float sensitivity;
        public Vector3 offset;
        [Tooltip("Camera min and max angles")]
        public Vector2 angleClamp;
        [Tooltip("When aiming. Camera FoV + aimFoV")]
        public float aimFov;
        [Tooltip("When running. Camera FoV + runFoV")]
        public float runFov;
        [Tooltip("Camera FoV change speed")]
        public float fovSpeed;
        [Header("Shake")]
        [Tooltip("Shake magnitude when running")]
        public float runMagnitude;
        [Tooltip("Shake frequency when running")]
        public float runFrequency;
        [System.NonSerialized]
        public bool aiming;
        private float fireTime;
        private float angle;
        private float startFov;
        private float targetFov;

        void Awake()
        {
            startFov = targetFov = cam.fieldOfView;
        }

        void Update()
        {
            if (GameUI.pause)
                return;

            if (target)
            {
                if (Input.GetButton("Fire") || Input.GetAxis("Fire_Joystick") >= 0.5f)
                    fireTime = 0.2f;

                aiming = Input.GetButton("Aim") || Input.GetAxis("Aim_Joystick") >= 0.5f || fireTime > 0;

                if (fireTime > 0)
                    fireTime -= Time.deltaTime;

                targetFov = startFov;
                if (aiming)
                    targetFov += aimFov;
                if (target.Character.running)
                    targetFov += runFov;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovSpeed);

                Vector3 origin = pivot.position + pivot.right * offset.x + Vector3.up * offset.y;
                Vector3 end = origin + pivot.forward * offset.z;
                Vector3 dir = ((end - origin).normalized - transform.forward).normalized;

                float shake = 0;
                if (target.Character.running)
                    shake = Mathf.Sin(Time.time * runFrequency * target.GetCurrentWeapon().Speed) * runMagnitude;

                if (Physics.SphereCast(origin - dir * 0.2f, 0.2f, dir, out RaycastHit hit, Mathf.Abs(offset.z), LayerMask.GetMask("Default")))
                {
                    transform.position = hit.point + hit.normal * 0.2f + transform.up * shake;
                }
                else
                {
                    transform.position = origin + dir * Mathf.Abs(offset.z) + transform.up * shake;
                }

                angle -= Input.GetAxisRaw("Mouse Y") * sensitivity;
                angle = Utils.ClampAngle(angle, angleClamp.x, angleClamp.y);

                Vector3 finalAngle = pivot.eulerAngles;
                finalAngle.x = angle;
                transform.eulerAngles = finalAngle;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public void Recoil(float recoil)
        {
            angle -= recoil;
        }
    }
}