using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class simplifies work with the layers and IKs of the animator.
    /// Anchor part is stands for the catching events from animations.
    /// </summary>
    public class AnimationAnchor : MonoBehaviour
    {

        /// <summary>
        /// Weights for the IK look
        /// </summary>
        public struct LookAtWeight
        {

            /// <summary>
            /// (0-1) the global weight of the LookAt, multiplier for other parameters.
            /// </summary>
            public float weight;

            /// <summary>
            /// (0-1) determines how much the body is involved in the LookAt.
            /// </summary>
            public float bodyWeight;

            /// <summary>
            /// (0-1) determines how much the head is involved in the LookAt.
            /// </summary>
            public float headWeight;

            /// <summary>
            /// (0-1) determines how much the eyes are involved in the LookAt.
            /// </summary>
            public float eyesWeight;

            /// <summary>
            /// (0-1) 0.0 means the character is completely unrestrained in motion, 1.0 means he's completely clamped (look at becomes impossible), and 0.5 means he'll be able to move on half of the possible range (180 degrees).
            /// </summary>
            public float clampWeight;

            /// <summary>
            /// Create LookAtWeight object.
            /// </summary>
            /// <param name="weight">(0-1) the global weight of the LookAt, multiplier for other parameters.</param>
            /// <param name="bodyWeight">(0-1) determines how much the body is involved in the LookAt.</param>
            /// <param name="headWeight">(0-1) determines how much the head is involved in the LookAt.</param>
            /// <param name="eyesWeight">(0-1) determines how much the eyes are involved in the LookAt.</param>
            /// <param name="clampWeight">(0-1) 0.0 means the character is completely unrestrained in motion, 1.0 means he's completely clamped (look at becomes impossible), and 0.5 means he'll be able to move on half of the possible range (180 degrees).</param>
            public LookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight)
            {
                this.weight = weight;
                this.bodyWeight = bodyWeight;
                this.headWeight = headWeight;
                this.eyesWeight = eyesWeight;
                this.clampWeight = clampWeight;
            }
        }

        [SerializeField]
        protected Animator animator = null;
        public Animator Animator => animator;

        /// <summary>
        /// Look at target
        /// </summary>
        protected Vector3 target;

        /// <summary>
        /// Look at weights
        /// </summary>
        protected LookAtWeight weight;

        private void OnAnimatorIK(int layerIndex)
        {
            AnimatorIK(layerIndex);
        }

        protected virtual void AnimatorIK(int layerIndex)
        {
            animator.SetLookAtPosition(target);
            animator.SetLookAtWeight(weight.weight, weight.bodyWeight, weight.headWeight, weight.eyesWeight, weight.clampWeight);
        }

        /// <summary>
        /// Look at the target.
        /// </summary>
        /// <param name="weight">(0-1) the global weight of the LookAt, multiplier for other parameters.</param>
        public void SetLookAt(Vector3 target, float weight)
        {
            this.target = target;
            this.weight = new LookAtWeight(weight, 1, 1, 1, 0.5f);
        }

        /// <summary>
        /// Look at the target.
        /// </summary>
        public void SetLookAt(Vector3 target, LookAtWeight weight)
        {
            this.target = target;
            this.weight = weight;
        }

    }
}