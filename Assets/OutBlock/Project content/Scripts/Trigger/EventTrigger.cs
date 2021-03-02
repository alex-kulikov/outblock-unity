using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Calls event on activation.
    /// </summary>
    public class EventTrigger : Trigger
    {

        [SerializeField, Header("Event trigger")]
        private UnityEvent onTrigger = default;
        public UnityEvent OnTrigger => onTrigger;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            onTrigger?.Invoke();
        }

    }
}