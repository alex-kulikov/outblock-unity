using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OutBlock
{

    /// <summary>
    /// Display <see cref="InputAction"/> name on the UI.
    /// </summary>
    public class ActionDisplay : MonoBehaviour
    {

        [SerializeField]
        private InputActionReference action = null;
        [SerializeField]
        private Text text = null;
        [SerializeField]
        private string overrideName = "";

        private void Start()
        {
            if (action && text)
            {
                text.text = !string.IsNullOrEmpty(overrideName) ? overrideName : action.action.name;
            }
        }

    }
}