using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    public interface IController : ICharacter
    {

        CapsuleCollider CapsuleCollider { get; }
        RagdollRoot RagdollRoot { get; }
        bool running { get; }
        bool Crouching { get; set; }
        Vector3 velocity { get; }
        Vector3 horizontalVelocity { get; }
        float maxSpeed { get; }
        bool climbing { get; }
        bool steppingUp { get; }
        Vector3 climbPoint { get; }
        bool isGrounded { get; }
        Vector3 MoveDirection { get; }
        bool climbInput { get; set; }
        bool Active { get; set; }
        bool Kinematic { get; set; }

        Character.OnLanded onLanded { get; set; }
        Character.OnJump onJump { get; set; }
        Character.OnCollider onTriggerEnter { get; set; }
        Character.OnCollider onTriggerExit { get; set; }
        Character.OnCollision onCollisionEnter { get; set; }
        Character.OnCollision onCollisionExit { get; set; }

        /// <summary>
        /// Move to the direction.
        /// </summary>
        /// <param name="dir">World direction</param>
        /// <param name="running">Running?</param>
        void Move(Vector3 dir, bool running);

        /// <summary>
        /// Set crouch.
        /// </summary>
        void Crouch(bool crouching);

        /// <summary>
        /// Initiate jump.
        /// </summary>
        void Jump();

        /// <summary>
        /// Set position.
        /// </summary>
        void SetPosition(Vector3 position);

        /// <summary>
        /// Enable ragdoll.
        /// </summary>
        /// <param name="stay">Stay in the ragdoll state for ever.</param>
        void EnableRagdoll(bool stay = false);

        /// <summary>
        /// Enable ragdoll with applied force.
        /// </summary>
        /// <param name="stay">Stay in the ragdoll state for ever.</param>
        void EnableRagdoll(Vector3 force, bool stay = false);

        /// <summary>
        /// Apply force to the character.
        /// </summary>
        void Force(Vector3 direction, float force, ForceMode forceMode);

    }
}