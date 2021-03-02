using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Root of the ragdoll, the main class.
    /// </summary>
    public class RagdollRoot : MonoBehaviour
    {

        /// <summary>
        /// Pair of the bone in the animator and the bone in the ragdoll.
        /// </summary>
        public class BonePair
        {

            /// <summary>
            /// Bone from animator.
            /// </summary>
            public Transform original;

            /// <summary>
            /// Old position of the original bone.
            /// </summary>
            public Vector3 oldPos;

            /// <summary>
            /// Old rotation of the original bone.
            /// </summary>
            public Quaternion oldRot;

            /// <summary>
            /// Same bone from the ragdoll.
            /// </summary>
            public Transform copy;

            public BonePair(Transform original, Transform copy)
            {
                oldPos = Vector3.zero;
                oldRot = Quaternion.identity;
                this.original = original;
                this.copy = copy;
            }

            /// <summary>
            /// Saves ragdoll bone position and rotation.
            /// </summary>
            public void SaveBone()
            {
                oldPos = copy.position;
                oldRot = copy.rotation;
            }
        }

        [SerializeField]
        private bool active = true;
        [SerializeField, Header("Components")]
        private GameObject character = null;
        [SerializeField]
        private Transform bonesRoot = null;
        [SerializeField]
        private Transform ragdollRoot = null;
        [SerializeField, Header("Parameters"), Tooltip("Min speed difference while stopping")]
        private float velocityDiffThreshold = 0.5f;
        [SerializeField, Tooltip("Velocity difference damage multiplier. Example: 1 m/s * 5 = 5 damage")]
        private float velocityDamageMultiplier = 5;
        [SerializeField, Tooltip("Ragdoll layer mask")]
        private int layerMask = 10;
        [SerializeField, Tooltip("Minimum when character will stand up")]
        private float velocityMinimum = 0.1f;
        [SerializeField, Tooltip("Time before character will stand up")]
        private float standUpDelay = 1;
        [SerializeField, Tooltip("How fast bones will recover their animation position")]
        private float recoverySpeed = 1;
        [SerializeField, Tooltip("How long after death ragdoll becomes kinematic")]
        private float sleepTime = 10;

        private bool ragdollEnabled = false;
        public bool RagdollEnabled
        {
            get
            {
                return ragdollEnabled;
            }
            private set
            {
                ragdollEnabled = value;
                UpdateRagdoll();
            }
        }

        /// <summary>
        /// Hips of the ragdoll.
        /// </summary>
        public RagdollPart hips { get; private set; }

        /// <summary>
        /// Ground position. Raycast from the hips to the Vector3.down.
        /// </summary>
        public Vector3 groundPos { get; private set; }

        /// <summary>
        /// Player attached to the ragdoll.
        /// </summary>
        public Player player { get; private set; }

        /// <summary>
        /// If set to false the enabling or disabling ragdoll will be ignored.
        /// </summary>
        public bool Active
        {
            get => active;
            set => active = value;
        }

        private ICharacter characterComponent;
        private IDamageable damageableComponent;
        private IActor playerComponent;
        private float delay;
        private Vector3 inputVelocity;
        private Vector3 oldVelocity;
        private Vector3 localPos;
        private Transform parent;
        private RagdollPart[] ragdollParts;
        private List<BonePair> bonePairs;
        private float copyRatio = 1;
        private bool stay;
        private float sleepDelay;

        public delegate void OnRagdollEnabled(bool ragdollEnabled);
        public OnRagdollEnabled onRagdollEnabled;

        private void Start()
        {
            if (character)
            {
                damageableComponent = character.GetComponent<IDamageable>();
                characterComponent = character.GetComponent<ICharacter>();
                playerComponent = character.GetComponent<IActor>();
                player = character.GetComponent<Player>();
            }

            localPos = transform.localPosition;
            parent = transform.parent;

            ragdollParts = GetComponentsInChildren<RagdollPart>();
            hips = ragdollParts[0];
            foreach (RagdollPart ragdollPart in ragdollParts)
            {
                ragdollPart.gameObject.layer = layerMask;
                ragdollPart.root = this;

                if (characterComponent != null)
                    Physics.IgnoreCollision(characterComponent.Collider, ragdollPart.coll);
            }

            bonePairs = new List<BonePair>();
            List<Transform> animatorBones, ragdollBones;
            animatorBones = bonesRoot.GetComponentsInChildren<Transform>().ToList();
            ragdollBones = ragdollRoot.GetComponentsInChildren<Transform>().ToList();
            for (int i = 0; i < animatorBones.Count; i++)
            {
                Transform copy = ragdollBones.SingleOrDefault(x => x.name == animatorBones[i].name);
                if (copy)
                {
                    bonePairs.Add(new BonePair(animatorBones[i], copy));
                }
            }

            UpdateRagdoll(true);
        }

        private void FixedUpdate()
        {
            if (RagdollEnabled)
            {
                bool characterDied = damageableComponent != null && damageableComponent.HPRatio <= 0;
                float diff = oldVelocity.magnitude - hips.rigidbody.velocity.magnitude;
                if (!stay && !characterDied)
                {
                    if (diff > velocityDiffThreshold)
                    {
                        if (player == null || !DevTools.Instance().NoFallDamage)
                            damageableComponent?.Damage(new DamageInfo(diff * velocityDamageMultiplier, transform.position, transform.position));
                    }

                    if (hips.rigidbody.velocity.magnitude <= velocityMinimum)
                    {
                        delay -= Time.deltaTime;
                        if (delay <= 0)
                            DisableRagdoll();
                    }
                    else delay = standUpDelay;
                }

                if (characterDied && diff <= velocityDiffThreshold && !ragdollParts[0].rigidbody.isKinematic)
                {
                    if (diff <= velocityDiffThreshold)
                    {
                        sleepDelay += Time.deltaTime;
                    }
                    else sleepDelay = 0;

                    if (sleepDelay >= sleepTime)
                    {
                        SetKinematic(true);
                    }
                }

                groundPos = hips.transform.position + Vector3.down;
                if (Physics.Raycast(hips.transform.position, Vector3.down, out RaycastHit hit, 1, characterComponent.GroundMask))
                {
                    groundPos = hit.point;
                }

                oldVelocity = hips.rigidbody.velocity;
            }
        }

        private void LateUpdate()
        {
            if (!RagdollEnabled)
            {
                if (copyRatio < 1)
                    copyRatio += Time.deltaTime * recoverySpeed;

                foreach (BonePair bonePair in bonePairs)
                {
                    if (copyRatio < 1)
                    {
                        bonePair.copy.position = Vector3.Lerp(bonePair.oldPos, bonePair.original.position, copyRatio);
                        bonePair.copy.rotation = Quaternion.Lerp(bonePair.oldRot, bonePair.original.rotation, copyRatio);
                    }
                    else
                    {
                        bonePair.copy.localPosition = bonePair.original.localPosition;
                        bonePair.copy.localRotation = bonePair.original.localRotation;
                    }
                }
            }
        }

        private void SetKinematic(bool kinematic)
        {
            sleepDelay = 0;

            foreach (RagdollPart part in ragdollParts)
                part.SetKinematic(kinematic);
        }

        private void UpdateRagdoll(bool init = false)
        {
            foreach(RagdollPart ragdollPart in ragdollParts)
            {
                ragdollPart.rigidbody.isKinematic = !ragdollEnabled;
                ragdollPart.rigidbody.useGravity = ragdollEnabled;
                ragdollPart.coll.enabled = ragdollEnabled;
                ragdollPart.rigidbody.velocity = inputVelocity;
            }

            groundPos = hips.transform.position + Vector3.down;
            transform.SetParent(RagdollEnabled ? null : parent);
            if (!RagdollEnabled && !init)
            {
                transform.localPosition = localPos;
                transform.localEulerAngles = Vector3.zero;
                playerComponent?.GetUp(Vector3.Angle(hips.transform.forward, Vector3.up) < 90);
            }
            delay = standUpDelay;

            onRagdollEnabled?.Invoke(ragdollEnabled);
        }

        private void SaveBonesTransform()
        {
            copyRatio = 0;
            foreach (BonePair bonePair in bonePairs)
                bonePair.SaveBone();
        }

        /// <summary>
        /// Enable ragdoll using current character velocity.
        /// </summary>
        /// <param name="stay">Stay as ragdoll for ever?</param>
        public void EnableRagdoll(Vector3 inputVelocity, bool stay = false)
        {
            if (!Active)
                return;

            if (ragdollEnabled)
            {
                if (inputVelocity != Vector3.zero)
                {
                    foreach (RagdollPart ragdollPart in ragdollParts)
                    {
                        ragdollPart.rigidbody.velocity = inputVelocity;
                    }
                }

                return;
            }

            if (ragdollParts[0].rigidbody.isKinematic)
                SetKinematic(false);

            this.stay = stay;
            this.inputVelocity = inputVelocity;
            RagdollEnabled = true;
        }

        /// <summary>
        /// Disable ragdoll.
        /// </summary>
        public void DisableRagdoll(bool fast = false)
        {
            if (!Active)
                return;

            if (!ragdollEnabled)
                return;

            SaveBonesTransform();
            if (fast)
                copyRatio = 0.99f;
            RagdollEnabled = false;
        }

        /// <summary>
        /// Redirect damage from the ragdoll to the character.
        /// </summary>
        /// <param name="damageInfo"></param>
        public void Damage(DamageInfo damageInfo)
        {
            if (!character)
                return;

            damageableComponent.Damage(damageInfo);
        }

        public void Kill()
        {
            if (!character)
                return;

            damageableComponent.Kill();
        }
    }
}