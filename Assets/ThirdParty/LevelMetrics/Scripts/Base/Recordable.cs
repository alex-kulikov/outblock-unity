using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Denver.Metrics
{

    public class Recordable
    {

        [System.Serializable]
        public class Config
        {
            [SerializeField]
            private string name = "Path";
            public string Name
            {
                get => name;
                set => name = value;
            }
            [SerializeField]
            private Transform objectToRecord = null;
            public Transform ObjectToRecord
            {
                get => objectToRecord;
                set
                {
                    objectToRecord = value;
                    ObjectName = objectToRecord != null ? objectToRecord.name : "";
                }
            }
            [SerializeField]
            private bool record = true;
            public bool Record
            {
                get => record;
                set => record = value;
            }
            [SerializeField]
            private float delay = 1;
            public float Delay => delay;
            [SerializeField, Header("Display")]
            private Color color = Color.green;
            public Color Color => color;

            public string ObjectName { get; private set; } = "";

            private float timer;

            public Config()
            {
            }

            public Config(string name, Transform objectToRecord, float delay, Color color)
            {
                this.name = name;
                this.objectToRecord = objectToRecord;
                this.delay = delay;
                this.color = color;
                UpdateObjectName();
            }

            private void UpdateObjectName()
            {
                ObjectName = objectToRecord != null ? objectToRecord.name : "";
            }

            public void Init()
            {
                timer = delay;
                UpdateObjectName();
            }

            public bool UpdateTimer()
            {
                timer += Time.deltaTime;
                if (timer > delay)
                {
                    timer = 0;
                    return true;
                }
                return false;
            }
        }

        public class Runtime
        {
            public string name { get; set; }
            public string objectName { get; set; }
            public Color gizmoColor { get; set; }
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

            public Runtime(string name, string objectName, Color gizmoColor)
            {
                this.name = name;
                this.objectName = objectName;
                this.gizmoColor = gizmoColor;

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

            public float time { get; set; }
            public Vector3 pos { get; set; }
            public bool isBreak { get; set; }

            public Data(float time, Vector3 pos, bool isBreak)
            {
                this.time = time;
                this.pos = pos;
                this.isBreak = isBreak;
            }

            public static List<Vector3> ToVector3(IList<Data> points)
            {
                List<Vector3> result = new List<Vector3>(points.Count);
                foreach (Data data in points)
                    result.Add(data.pos);
                return result;
            }

        }

    }
}