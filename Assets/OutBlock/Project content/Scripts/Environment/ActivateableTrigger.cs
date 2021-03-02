using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Trigger which is activate the Activateable through collision.
    /// Collision is valid if the object is implemented ISignalSource.
    /// </summary>
    public class ActivateableTrigger : MonoBehaviour
    {
        [SerializeField, Tooltip("Link to connected activateables")]
        private Activateable[] activateables = new Activateable[0];

        private List<ISignalSource> signals = new List<ISignalSource>();

        private void OnTriggerEnter(Collider other)
        {
            ISignalSource source = other.GetComponent<ISignalSource>();
            if (source != null && !signals.Contains(source))
            {
                signals.Add(source);
                foreach (Activateable activateable in activateables)
                    activateable.Activate(source);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ISignalSource source = other.GetComponent<ISignalSource>();
            if (source != null)
            {
                signals.Remove(source);
                foreach (Activateable activateable in activateables)
                    activateable.Deactivate(source);
            }
        }
    }
}