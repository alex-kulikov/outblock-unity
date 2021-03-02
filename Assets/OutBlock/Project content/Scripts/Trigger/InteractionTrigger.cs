using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Interactive trigger. Used by player. Activates by collision and pressing Action key.
    /// </summary>
    public class InteractionTrigger : Trigger
    {

        [SerializeField, Header("Interaction trigger")]
        private Trigger[] triggers = new Trigger[0];
        public Trigger[] Triggers => triggers;
        [SerializeField, Space]
        protected UnityEvent onInteract = default;
        public UnityEvent OnInteract => onInteract;
        [SerializeField]
        private GameObject handIK = null;

        private void Awake()
        {
            if (handIK)
                handIK.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (tags.Contains(other.tag))
                Player.interaction.Remove(this);
        }

        private void DisableHandIK()
        {
            if (handIK)
                handIK.SetActive(false);
        }

        protected override void OnTriggerStay(Collider other)
        {
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (tags.Contains(other.tag) && !Player.interaction.Contains(this))
                Player.interaction.Add(this);
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);

            foreach (Trigger trigger in triggers)
                trigger?.Activate(other);

            onInteract?.Invoke();

            if (handIK)
            {
                handIK.SetActive(true);
                Invoke("DisableHandIK", 0.5f);
            }
        }

    }
}