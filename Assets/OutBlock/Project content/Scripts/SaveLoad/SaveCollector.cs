using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OutBlock
{
    public class SaveCollector : MonoBehaviour
    {

        private List<ISaveable> saveables = new List<ISaveable>();

        public static List<SaveCollector> Collectors { get; private set; } = new List<SaveCollector>();

        private void OnEnable()
        {
            Collectors.Add(this);

            SaveLoad.Instance().onLoad += CleanSaveablesList;

            saveables.AddRange(GetComponentsInChildren<ISaveable>(true));
            foreach(ISaveable saveable in saveables)
            {
                saveable.Register();
            }

            List<Transform> simpleObjects = GetAllTransforms(transform);

            foreach (Transform tr in simpleObjects)
            {
                Saveable saveable = tr.gameObject.AddComponent<Saveable>();
                saveable.Register();
            }
        }

        private void OnDisable()
        {
            Collectors.Remove(this);

            if (SaveLoad.Instance(true))
                SaveLoad.Instance(true).onLoad -= CleanSaveablesList;
        }

        private List<Transform> GetAllTransforms(Transform parent)
        {
            List<Transform> transformList = new List<Transform>();
            BuildTransformList(transformList, parent);
            return transformList;
        }

        private void BuildTransformList(ICollection<Transform> transforms, Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            if (saveables.FirstOrDefault(x => x.GO == parent.gameObject) != null)
                return;

            if (parent != transform)
                transforms.Add(parent);

            foreach (Transform t in parent)
            {
                BuildTransformList(transforms, t);
            }
        }

        private void CleanSaveablesList()
        {
            for (int i = saveables.Count - 1; i >= 0; i--)
            {
                try
                {
                    bool exists = saveables[i].GO != null;
                }
                catch
                {
                    saveables.Remove(saveables[i]);
                }
            }
        }

        public void AddToCollector(Transform go)
        {
            ISaveable[] goSaveables = go.GetComponentsInChildren<ISaveable>();
            foreach (ISaveable saveable in goSaveables)
            {
                saveable.Register();
            }
            saveables.AddRange(goSaveables);

            List<Transform> trs = new List<Transform>() { go };
            trs.AddRange(GetAllTransforms(go));
            foreach(Transform tr in trs)
            {
                Saveable saveable = tr.gameObject.AddComponent<Saveable>();
                saveable.Register();
            }
        }

#if UNITY_EDITOR
        #region Menu items
        [MenuItem("OutBlock/Logic/Add Logic Collector")]
        public static SaveCollector AddLogicCollector()
        {
            GameObject go = new GameObject("LogicCollector");
            SaveCollector collector = go.AddComponent<SaveCollector>();
            Selection.activeGameObject = go;
            return collector;
        }

        [MenuItem("OutBlock/Logic/Collect Objects")]
        public static void CollectObjects()
        {
            SaveCollector collector = FindObjectOfType<SaveCollector>();
            if (collector == null)
            {
                collector = AddLogicCollector();
            }

            IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
            foreach(ISaveable saveable in saveables)
            {
                if (saveable.GO.transform.parent != null)
                    continue;

                Undo.RecordObject(saveable.GO.transform, "Set new parent");
                saveable.GO.transform.SetParent(collector.transform);
            }
        }
        #endregion
#endif

    }
}