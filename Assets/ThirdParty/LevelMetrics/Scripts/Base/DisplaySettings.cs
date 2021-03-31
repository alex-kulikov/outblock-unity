using UnityEngine;

namespace Denver.Metrics
{
    [System.Serializable]
    public class DisplaySettings
    {

        public enum Modes { Simple, ViewCheck };

        [SerializeField, Tooltip("Display text in the scene view")]
        private bool showText = true;
        public bool ShowText => showText;
        [SerializeField, Tooltip("Simple mode just draws all gizmos in the scene view, good for the small scenes and low gizmo quantity. View Check checks whether is gizmo visible to the camera before drawing it, it works slightly slower than a Simple mode but can increase performance with big scenes and high gizmo quantity.")]
        private Modes mode = Modes.ViewCheck;
        public Modes Mode => mode;

    }

}
