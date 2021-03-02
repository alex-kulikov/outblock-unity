using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Interface for the characters.
    /// </summary>
    public interface ICharacter
    {

        /// <summary>
        /// Collider of the character.
        /// </summary>
        Collider Collider { get; }

        /// <summary>
        /// Layer mask of the ground.
        /// </summary>
        LayerMask GroundMask { get; }

        /// <summary>
        /// Minimal impulse required to enable the ragdoll
        /// </summary>
        float MinImpulse { get; }

    }
}