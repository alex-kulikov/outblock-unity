using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class just destroys the object.
    /// </summary>
    public class Destroyer : MonoBehaviour
    {

        [SerializeField, Tooltip("Initiate destroy procedure on awake")]
        private bool onAwake = false;
        [SerializeField, Tooltip("Random time range before the object will be destroyed")]
        private Vector2 delayRange = Vector2.zero;

        public delegate void OnDestroyed();
        /// <summary>
        /// This event will be called on object's destroy.
        /// </summary>
        public OnDestroyed onDestroy;

        private void Awake()
        {
            if (onAwake)
                Destroy();
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }

        /// <summary>
        /// Destroy according to this object delay.
        /// </summary>
        public void Destroy()
        {
            DestroyTime(Random.Range(delayRange.x, delayRange.y));
        }

        /// <summary>
        /// Destroy with timer.
        /// </summary>
        public void DestroyTime(float time)
        {
            Destroy(gameObject, time);
        }

        /// <summary>
        /// Destroy this object now.s
        /// </summary>
        public void DestroyNow()
        {
            Destroy(gameObject);
        }

    }
}