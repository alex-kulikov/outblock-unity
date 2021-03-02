using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// More complex AnimationAnchor with every limb IK.
    /// </summary>
    public class CharacterAnimator : AnimationAnchor
    {

        /// <summary>
        /// Type of limb.
        /// </summary>
        public enum Limbs { LeftHand, RightHand, LeftFoot, RightFoot};

        /// <summary>
        /// Weight of the IK.
        /// </summary>
        public struct IKWeight
        {
            /// <summary>
            /// Target position.
            /// </summary>
            public Vector3 position;

            /// <summary>
            /// Target rotation.
            /// </summary>
            public Quaternion rotation;

            /// <summary>
            /// Weight of the IK.
            /// </summary>
            public float weight;

            public IKWeight(Vector3 position, Quaternion rotation, float weight)
            {
                this.position = position;
                this.rotation = rotation;
                this.weight = weight;
            }
        }

        [SerializeField, Header("Components")]
        private Player player = null;
        [SerializeField, Header("Parameters")]
        private string speedParameter = "";
        [SerializeField]
        private float speedSmoothing = 6;
        [SerializeField]
        private string crouchParameter = "";
        [SerializeField]
        private string isGroundedParameter = "";
        [SerializeField]
        private string jumpParameter = "";
        [SerializeField]
        private string climbParameter = "";
        [SerializeField, Header("IK")]
        private float footGroundOffset = 0.1f;

        private const float climbingDelay = 0.75f;

        private float isGroundedValue;
        private bool isGrounded;
        private bool oldClimbing;
        private float climbingTime;
        private float velocity;
        private IKWeight leftHandIK;
        private float leftHandWeight;
        private IKWeight rightHandIK;
        private float rightHandWeight;
        private IKWeight leftFootIK;
        private IKWeight rightFootIK;
        private Vector3 startLeftHandPos;
        private Vector3 startRightHandPos;
        private Transform touching;

        private void OnEnable()
        {
            if (player.Character == null)
                return;

            player.Character.onJump += Jump;
            player.Character.onTriggerEnter += CharOnTriggerEnter;
            player.Character.onTriggerExit += CharOnTriggerExit;
            player.Character.RagdollRoot.onRagdollEnabled += RagdollEnabled;
        }

        private void OnDisable()
        {
            if (player.Character == null)
                return;

            player.Character.onJump -= Jump;
            player.Character.onTriggerEnter -= CharOnTriggerEnter;
            player.Character.onTriggerExit -= CharOnTriggerExit;
            player.Character.RagdollRoot.onRagdollEnabled += RagdollEnabled;
        }

        private void Start()
        {
            startLeftHandPos = transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftHand).position);
            startRightHandPos = transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightHand).position);
        }

        private void Update()
        {
            if (player.Character == null || !animator)
                return;

            if (climbingTime > 0)
                climbingTime -= Time.deltaTime;

            isGroundedValue = Mathf.Lerp(isGroundedValue, player.Character.isGrounded ? 1 : 0, Time.deltaTime * 4);
            isGrounded = isGroundedValue > 0.5f || player.Character.isGrounded;

            float targetVelocity = player.Character.steppingUp ? 0.5f : player.Character.horizontalVelocity.magnitude / player.Character.maxSpeed;
            if (player.Character.MoveDirection.magnitude <= 0.01f)
                targetVelocity = 0;
            velocity = Mathf.Lerp(velocity, targetVelocity, Time.deltaTime * speedSmoothing);

            if (!string.IsNullOrEmpty(speedParameter))
            {
                animator.SetFloat(speedParameter, velocity);
            }

            if (oldClimbing != player.Character.climbing)
            {
                oldClimbing = player.Character.climbing;
                if (player.Character.climbing && climbingTime <= 0)
                {
                    climbingTime = climbingDelay;
                    animator.SetTrigger(climbParameter);
                }
            }

            if (!string.IsNullOrEmpty(crouchParameter))
                animator.SetBool(crouchParameter, player.Character.Crouching);

            if (!string.IsNullOrEmpty(isGroundedParameter))
                animator.SetBool(isGroundedParameter, isGrounded);

            //hands
            leftHandIK = new IKWeight(leftHandIK.position, leftHandIK.rotation, 0);
            rightHandIK = new IKWeight(rightHandIK.position, rightHandIK.rotation, 0);

            Transform chestTransform = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (touching && !touching.gameObject.activeSelf)
                touching = null;

            if (player.GetCurrentWeapon().WeaponType == Weapon.WeaponTypes.Empty && touching && Vector3.Angle(transform.forward, (touching.position - transform.position).normalized) <= 90)
            {
                float leftHandDist = Vector3.Distance(transform.TransformPoint(startLeftHandPos), touching.position);
                float rightHandDist = Vector3.Distance(transform.TransformPoint(startRightHandPos), touching.position);
                Quaternion rot = Quaternion.LookRotation(transform.forward, (chestTransform.position - touching.position).normalized);

                if (Mathf.Min(leftHandDist, rightHandDist) <= 0.7f)
                {
                    bool rightHand = rightHandDist < leftHandDist;
                    leftHandIK = new IKWeight(touching.position, rot, rightHand ? 0 : 1);
                    rightHandIK = new IKWeight(touching.position, rot, rightHand ? 1 : 0);
                }
            }

            if (player.Character.climbing)
            {
                leftHandIK = new IKWeight(player.Character.climbPoint - transform.right * 0.3f, transform.rotation, 1);
                rightHandIK = new IKWeight(player.Character.climbPoint + transform.right * 0.3f, transform.rotation, 1);
            }

            leftHandWeight = Mathf.Lerp(leftHandWeight, leftHandIK.weight, Time.deltaTime * 5);
            rightHandWeight = Mathf.Lerp(rightHandWeight, rightHandIK.weight, Time.deltaTime * 5);            
        }

        private void CharOnTriggerEnter(Collider coll)
        {
            if (coll.CompareTag("ENV"))
                touching = coll.transform;
        }

        private void CharOnTriggerExit(Collider coll)
        {
            if (coll.CompareTag("ENV"))
                touching = null;
        }

        private void RagdollEnabled(bool ragdollEnabled)
        {
            animator.enabled = !ragdollEnabled;
        }

        private void Jump()
        {
            if (!string.IsNullOrEmpty(jumpParameter))
                animator.SetTrigger(jumpParameter);
        }

        protected override void AnimatorIK(int layerIndex)
        {
            base.AnimatorIK(layerIndex);

            //hands
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIK.position);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIK.position);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);

            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIK.rotation);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIK.rotation);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);

            //feet
            Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            leftFootIK.weight = 0;
            rightFootIK.weight = 0;
            if (Physics.Raycast(leftFoot.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit leftFootHit, 1, player.Character.GroundMask))
            {
                leftFootIK.weight = animator.GetFloat("LeftFootIK");
                leftFootIK.position = leftFootHit.point + Vector3.up * footGroundOffset;
                Vector3 slopeCorrected = Vector3.Cross(leftFootHit.normal, leftFoot.right);
                Quaternion footRotation = Quaternion.LookRotation(slopeCorrected, leftFootHit.normal);
                leftFootIK.rotation = footRotation;
            }
            if (Physics.Raycast(rightFoot.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit rightFootHit, 1, player.Character.GroundMask))
            {
                rightFootIK.weight = animator.GetFloat("RightFootIK");
                rightFootIK.position = rightFootHit.point + Vector3.up * footGroundOffset;
                Vector3 slopeCorrected = Vector3.Cross(rightFootHit.normal, rightFoot.right);
                Quaternion footRotation = Quaternion.LookRotation(slopeCorrected, rightFootHit.normal);
                rightFootIK.rotation = footRotation;
            }

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIK.position);
            animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIK.position);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIK.weight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIK.weight);

            animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootIK.rotation);
            animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootIK.rotation);

            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootIK.weight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootIK.weight);
        }

        /// <summary>
        /// Apply IK to the limb.
        /// </summary>
        public void SetIK(Limbs limb, IKWeight ikData)
        {
            switch (limb)
            {
                case Limbs.LeftHand:
                    leftHandIK = ikData;
                    break;

                case Limbs.RightHand:
                    rightHandIK = ikData;
                    break;

                case Limbs.LeftFoot:
                    leftFootIK = ikData;
                    break;

                case Limbs.RightFoot:
                    rightFootIK = ikData;
                    break;
            }
        }

    }
}