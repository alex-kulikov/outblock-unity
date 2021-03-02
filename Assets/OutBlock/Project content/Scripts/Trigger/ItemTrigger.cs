using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Checks if player have an item and calls events corresponding to this.
    /// </summary>
    public class ItemTrigger : Trigger
    {

        [SerializeField, Header("Item trigger"), Tooltip("Target item in the player inventory.")]
        private string item = "";
        [SerializeField]
        private int requiredItemCount = 1;
        [SerializeField, Tooltip("Automatically consume item and show message on touch.")]
        private bool autoMode = true;
        [SerializeField, Tooltip("Activate these triggers on success.")]
        private Trigger[] triggers = new Trigger[0];
        public Trigger[] Triggers => triggers;
        [SerializeField, Tooltip("Fire this event when player have a target item.")]
        private UnityEvent onSuccess = default;
        public UnityEvent OnSuccess => onSuccess;
        [SerializeField, Tooltip("Fire this event when player doesnt have a target item.")]
        private UnityEvent onFail = default;
        public UnityEvent OnFail => onFail;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            if (Player.Instance.Inventory.HasItem(item, requiredItemCount))
            {
                if (autoMode)
                    ConsumeItem();

                foreach (Trigger trigger in triggers)
                    trigger.Activate(other);

                onSuccess?.Invoke();
            }
            else
            {
                if (autoMode)
                    ShowMessage();

                onFail?.Invoke();
            }
        }

        /// <summary>
        /// Use player's item.
        /// </summary>
        public void ConsumeItem()
        {
            Player.Instance?.Inventory.ConsumeItem(item, requiredItemCount);
        }

        /// <summary>
        /// Show consumption message.
        /// </summary>
        public void ShowMessage()
        {
            GameUI.Instance().ShowMessage(string.Format("You need {0}x of {1}", requiredItemCount, item));
            StopCoroutine("EndCollision");
            done = false;
            colliding = false;
            Reload();
        }

    }
}