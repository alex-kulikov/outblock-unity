using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Same as <see cref="InteractionTrigger"/> but activates only if player have an item.
    /// </summary>
    public class ItemInteractionTrigger : InteractionTrigger
    {

        [SerializeField, Header("Item trigger"), Tooltip("Target item in the player inventory.")]
        private string item = "";
        [SerializeField]
        private int requiredItemCount = 1;

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            if (Player.Instance.Inventory.HasItem(item, requiredItemCount))
            {
                ConsumeItem();
                base.TriggerAction(other);
                onInteract?.Invoke();
            }
            else
            {
                ShowMessage();
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
        /// Show consumption item.
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