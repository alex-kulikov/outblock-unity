using UnityEngine;
using System.Collections;

namespace OutBlock
{

    /// <summary>
    /// Represention of the Min Max range.
    /// </summary>
    [System.Serializable]
    public struct MinMaxFloat
    {

        /// <summary>
        /// Minimum value.
        /// </summary>
        public float min;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public float max;

        /// <summary>
        /// Get a random value of the range.
        /// </summary>
        /// <returns></returns>
        public float GetRandom()
        {
            return Random.Range(min, max);
        }
    }
}