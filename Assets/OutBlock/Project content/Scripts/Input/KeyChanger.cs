using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace OutBlock
{

    /// <summary>
    /// UI to change <see cref="InputAction"/> key in realtime.
    /// </summary>
    public class KeyChanger : MonoBehaviour
    {

        [SerializeField]
        private string nameOverride = "";
        [SerializeField]
        private InputActionReference actionReference = null;
        [SerializeField]
        private int bindingId = 0;
        [SerializeField]
        private InputBinding.DisplayStringOptions displayOptions = 0;
        [SerializeField]
        private Text bindingName = null;
        [SerializeField]
        private Text keyName = null;
        [SerializeField]
        private Image imageHolder = null;

        private InputAction action;
        private Color startColor;
        private bool rebinding;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;

        private static List<KeyChanger> changers = new List<KeyChanger>();

        private void Start()
        {
            startColor = imageHolder.color;

            if (actionReference != null)
            {
                action = InputManager.Instance().controls.asset.FindAction(actionReference.name);
                bindingName.text = string.IsNullOrEmpty(nameOverride) ? action.name : nameOverride;
                UpdateKeyName();
            }
        }

        private void OnEnable()
        {
            changers.Add(this);
        }

        private void OnDisable()
        {
            changers.Remove(this);
        }

        private void Update()
        {
            if (rebinding)
            {
                Color newColor = startColor;
                newColor.a = Mathf.PingPong(Time.unscaledTime * 2, 1);
                imageHolder.color = newColor;
            }
        }

        private void UpdateKeyName()
        {
            keyName.text = action.GetBindingDisplayString(bindingId, displayOptions);
        }

        private void Cancel()
        {
            if (rebinding)
                rebindOperation.Cancel();
        }

        private void ToDefault()
        {
            CancelAll();
            action.RemoveBindingOverride(bindingId);
            UpdateKeyName();
        }

        private void RebingOperationDispose(InputActionRebindingExtensions.RebindingOperation obj)
        {
            if (!rebinding)
                return;
            rebinding = false;

            obj?.Dispose();
            obj = null;

            imageHolder.color = startColor;

            action.Enable();

            UpdateKeyName();
        }

        /// <summary>
        /// Start rebing and wait for key.
        /// </summary>
        public void ChangeAction()
        {
            CancelAll();

            action.Disable();

            rebinding = true;

            rebindOperation = action.PerformInteractiveRebinding(bindingId)
                .WithControlsExcluding("<Gamepad>")
                .OnMatchWaitForAnother(0.1f)
                .OnCancel(RebingOperationDispose)
                .OnComplete(RebingOperationDispose)
                .Start();
        }

        /// <summary>
        /// Cancel all rebindings.
        /// </summary>
        public static void CancelAll()
        {
            foreach (KeyChanger changer in changers)
                changer.Cancel();
        }

        /// <summary>
        /// Reset binding to default.
        /// </summary>
        public static void ResetToDefault()
        {
            foreach (KeyChanger changer in changers)
                changer.ToDefault();
        }

    }
}