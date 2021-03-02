using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This class displays the current Activateable state.
    /// </summary>
    public class ActivateableDisplay : MonoBehaviour
    {

        [SerializeField, Tooltip("Target activateable")]
        private Activateable activateable = null;
        [SerializeField, Tooltip("Target renderer with material")]
        new private Renderer renderer = null;
        [SerializeField, Tooltip("Material when active")]
        private Material activeMaterial = null;
        [SerializeField, Tooltip("Material when not active")]
        private Material disabledMaterial = null;

        private bool active;

        private void Start()
        {
            UpdateMaterial();
        }

        private void Update()
        {
            if (active != activateable.Active)
                UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            active = activateable.Active;

            if (!activateable || !renderer)
                return;

            renderer.material = active ? activeMaterial : disabledMaterial;
        }
    }
}