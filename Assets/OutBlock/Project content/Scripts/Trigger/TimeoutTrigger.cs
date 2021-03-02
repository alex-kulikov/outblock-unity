using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Triggers time countdown and performs action when time is out.
    /// </summary>
    public class TimeoutTrigger : Trigger
    {

        [SerializeField, Header("Timeout trigger")]
        private float timeOut = 1;
        [SerializeField]
        private UnityEvent timeOutEvent = default;
        public UnityEvent TimeOutEvent => timeOutEvent;

        private bool counting;
        private float time = 0;

        private void Update()
        {
            if (counting)
            {
                if (time > 0)
                    time -= Time.deltaTime;
                else TimeOut();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (counting && onCollision && tags.Contains(other.tag))
            {
                counting = false;
                time = 0;
            }
        }

        private void TimeOut()
        {
            counting = false;
            timeOutEvent?.Invoke();
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            counting = true;
            if (time <= 0)
                time = timeOut;
        }

        /// <summary>
        /// Stop time countdown.
        /// </summary>
        public void StopCount()
        {
            counting = false;
        }

    }
}