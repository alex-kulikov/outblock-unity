using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class represents gun firing effect.
    /// </summary>
    public class GunEffect : MonoBehaviour
    {

        [SerializeField]
        private float destroyTime = 5;

        /// <summary>
        /// Muzzle position.
        /// </summary>
        protected Vector3 startPos;

        /// <summary>
        /// Position where weapon hit.
        /// </summary>
        protected Vector3 hitPos;

        /// <summary>
        /// Normal vector of the hit surface.
        /// </summary>
        protected Vector3 normal;

        /// <summary>
        /// Hit object.
        /// </summary>
        protected Transform hitObject;

        /// <summary>
        /// Set all parameters to the effect.
        /// </summary>
        public virtual void Set(Vector3 startPos, Vector3 hitPos, Vector3 normal, Transform hitObject)
        {
            this.startPos = startPos;
            this.hitPos = hitPos;
            this.normal = normal;
            this.hitObject = hitObject;

            Destroy(gameObject, destroyTime);
        }
    }
}