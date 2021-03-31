using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Denver.Metrics
{

    /// <summary>
    /// Class for the signals.
    /// </summary>
    public class Signal
    {

        [System.Serializable]
        public class Config
        {

            [SerializeField]
            private string name = "Signal";
            public string Name
            {
                get => name;
                set => name = value;
            }
            [SerializeField, Tooltip("From which object the signal should get position.")]
            private Transform lookUpObject = null;
            public Transform LookUpObject
            {
                get => lookUpObject;
                set
                {
                    lookUpObject = value;
                    ObjectName = lookUpObject != null ? lookUpObject.name : "";
                }
            }
            [SerializeField]
            private bool record = true;
            public bool Record
            {
                get => record;
                set => record = value;
            }
            [SerializeField, Tooltip("How many times a signal should be called before it going to be considered?")]
            private int countThreshold = 2;
            public int CountThreshold => countThreshold;
            [SerializeField, Tooltip("Minimal distance between same signals.")]
            private int distanceThreshold = 3;
            public int DistanceThreshold => distanceThreshold;
            [SerializeField, Header("Input"), Tooltip("Send signal on the key press?")]
            private bool activateOnKey = false;
            public bool ActivateOnKey => activateOnKey;
            [SerializeField, Tooltip("Button name from the Input Settings.")]
            private string button = "";
            public string Button => button;
            [SerializeField, Tooltip("KeyCode of the key.")]
            private KeyCode key = KeyCode.None;
            public KeyCode Key => key;
            [SerializeField, Header("Display"), Tooltip("Color of the gizmo of the signal.")]
            private Color gizmoColor = Color.green;
            public Color GizmoColor => gizmoColor;
            [SerializeField, Tooltip("Size of the gizmo of the signal.")]
            private float gizmoSize = 2;
            public float GizmoSize => gizmoSize;
            [SerializeField, Tooltip("Type of the gizmo of the signal.")]
            private GizmoTypes gizmoType = GizmoTypes.WireSphere;
            public GizmoTypes GizmoType => gizmoType;

            public string ObjectName { get; private set; } = "";

            public Config()
            {

            }

            public Config(string name, Transform lookUpObject, int countThreshold, int distanceThreshold, Color gizmoColor, float gizmoSize, GizmoTypes gizmoType)
            {
                this.name = name;
                this.lookUpObject = lookUpObject;
                this.countThreshold = countThreshold;
                this.distanceThreshold = distanceThreshold;
                this.gizmoColor = gizmoColor;
                this.gizmoSize = gizmoSize;
                this.gizmoType = gizmoType;
            }

            public void Init()
            {
                ObjectName = lookUpObject != null ? lookUpObject.name : "";
            }

        }

        public class Runtime
        {

            public string name { get; set; }
            public string objectName { get; set; }
            public int countThreshold { get; set; }
            public Color gizmoColor { get; set; }
            public float gizmoSize { get; set; }
            public GizmoTypes gizmoType { get; set; }
#if UNITY_EDITOR
            public LevelMetricsTreeElement treeElement { get; set; }
#endif

            public ReadOnlyCollection<Data> Data
            {
                get
                {
                    return new ReadOnlyCollection<Data>(data);
                }
            }

            private List<Data> data = new List<Data>();

            public Runtime(string name, string objectName, int countThreshold, Color gizmoColor, float gizmoSize, GizmoTypes gizmoType)
            {
                this.name = name;
                this.objectName = objectName;
                this.countThreshold = countThreshold;
                this.gizmoColor = gizmoColor;
                this.gizmoSize = gizmoSize;
                this.gizmoType = gizmoType;

                data = new List<Data>();
#if UNITY_EDITOR
                treeElement = new LevelMetricsTreeElement();
#endif
            }

            public void Add(Data data)
            {
                this.data.Add(data);
#if UNITY_EDITOR
                if (this.data.Count == 1)
                    treeElement.SetPos(data.pos);
#endif
            }

        }

        public class Data
        {

            public Vector3 pos { get; set; }
            public int count { get; set; }
            public float time { get; set; }
#if UNITY_EDITOR
            public LevelMetricsTreeElement treeElement { get; set; }
#endif

            public Data(Vector3 pos, int count, float time)
            {
                this.pos = pos;
                this.count = count;
                this.time = time;

#if UNITY_EDITOR
                treeElement = new LevelMetricsTreeElement();
                treeElement.SetPos(pos);
#endif
            }

        }

    }
}