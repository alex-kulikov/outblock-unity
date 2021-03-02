using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Interface for the actors
    /// </summary>
    public interface IActor
    {

        /// <summary>
        /// Get up from the ground after being a ragdoll
        /// </summary>
        /// <param name="spine">Lying on the spine?</param>
        void GetUp(bool spine);

        /// <summary>
        /// Warps actor to the position.
        /// </summary>
        void Warp(Vector3 pos);

    }
}