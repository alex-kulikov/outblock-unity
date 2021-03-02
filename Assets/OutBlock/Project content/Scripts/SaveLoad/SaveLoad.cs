using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Manages save and loading on the scene.
    /// </summary>
    public class SaveLoad : MonoBehaviour
    {

        private List<ISaveable> saveables = new List<ISaveable>();
        private List<SaveData> initSaveData;
        private List<SaveData> saveData;
        private List<ITrash> trash = new List<ITrash>();
        private bool processing;
        /// <summary>
        /// Is it in the process of loading or saving?
        /// </summary>
        public bool Processing
        {
            get
            {
                return processing;
            }
            private set
            {
                if (processing == value)
                    return;

                processing = value;
                onProcessingUpdate?.Invoke(processing);
            }
        }

        private static SaveLoad instance;
        /// <summary>
        /// SaveLoad instance. Only one on the scene.
        /// </summary>
        /// <param name="onDestroy">Set true when calling on OnDisable/OnDestroy to prevent error while unloading scene or game</param>
        /// <returns></returns>
        public static SaveLoad Instance(bool onExit = false)
        {
            if (instance == null)
            {
                if (onExit)
                    return null;

                SaveLoad inScene = FindObjectOfType<SaveLoad>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    GameObject go = new GameObject("SaveLoad");
                    SaveLoad saveLoad = go.AddComponent<SaveLoad>();
                    instance = saveLoad;
                }
            }
            return instance;
        }

        public delegate void OnSave();
        /// <summary>
        /// Called when saving is done.
        /// </summary>
        public OnSave onSave;

        public delegate void OnLoad();
        /// <summary>
        /// Called when loading is done.
        /// </summary>
        public OnLoad onLoad;

        public delegate void ProcessingUpdate(bool processing);
        /// <summary>
        /// Called when <see cref="Processing"/> is changed.
        /// </summary>
        public ProcessingUpdate onProcessingUpdate;

        /// <summary>
        /// Loading delay. For the game pause and UI fade.
        /// </summary>
        public const float loadDelay = 1;

        private int lastSaveableID = 0;

        private void Start()
        {
            Invoke("InitSave", 0.1f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                Save();
            if (Input.GetKeyDown(KeyCode.F6))
                Load();

            float alpha = GameUI.Instance().BlackAlpha;
            alpha += Processing ? Time.unscaledDeltaTime * 2 : -Time.unscaledDeltaTime * 4;
            alpha = Mathf.Clamp01(alpha);
            GameUI.Instance().BlackAlpha = alpha;
        }

        private void InitSave()
        {
            SaveData(ref initSaveData);
        }

        private void SaveData(ref List<SaveData> saveData)
        {
            if (Processing)
                return;

            Processing = true;
            saveData = new List<SaveData>();

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (ISaveable saveable in saveables)
            {
                SaveData newData = saveable.Save();
                saveData.Add(newData);
            }
            Processing = false;

            GameUI.Instance().SaveLoadIndicator(true);
            onSave?.Invoke();
        }

        private bool CanLoad(List<SaveData> saveData)
        {
            return saveData != null && saveData.Count > 0 && !Processing;
        }

        private bool LoadData(ref List<SaveData> saveData)
        {
            if (!CanLoad(saveData))
                return false;

            StartCoroutine(LoadSequence(saveData));
            GameUI.Instance().SaveLoadIndicator(false);
            return true;
        }

        private IEnumerator LoadSequence(List<SaveData> saveData)
        {
            Processing = true;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForSecondsRealtime(loadDelay);
            for (int i = 0; i < saveData.Count; i++)
            {
                SaveData data = saveData[i];
                ISaveable saveable = saveables.SingleOrDefault(x => x.Id == data.id);
                if (saveable != null && saveable.ToString() != "null")
                {
                    saveable.Load(data);
                }
            }

            Processing = false;

            ClearTrash();
            List<ISaveable> redundant = saveables.Where(x => !saveData.Any(y => y.id == x.Id)).ToList();
            for (int i = redundant.Count - 1; i >= 0; i--)
            {
                Destroy(redundant[i].GO);
            }

            DevTools.Instance().DisableAll();
            yield return new WaitForEndOfFrame();
            onLoad?.Invoke();
        }

        private int AddSaveable(ISaveable saveable)
        {
            if (Processing)
                return -1;

            saveables.Add(saveable);
            return lastSaveableID++;
        }

        private void RemoveSaveable(ISaveable saveable)
        {
            if (Processing)
                return;

            saveables.Remove(saveable);
        }

        private void ClearTrash()
        {
            for (int i = trash.Count - 1; i >= 0; i--)
            {
                trash[i].Dispose();
            }
        }

        /// <summary>
        /// Save.
        /// </summary>
        public void Save()
        {
            SaveData(ref saveData);
        }

        /// <summary>
        /// Load.
        /// </summary>
        public void Load()
        {
            if (!LoadData(ref saveData))
                LoadInit();
        }

        /// <summary>
        /// Load initial save.
        /// </summary>
        public void LoadInit()
        {
            LoadData(ref initSaveData);
        }

        /// <summary>
        /// Register saveable.
        /// </summary>
        public static void Add(ISaveable saveable)
        {
            if (Instance() != null)
            {
                if (Instance().saveables.Contains(saveable))
                    return;

                int newId = Instance().AddSaveable(saveable);
                if (saveable.Id == -1)
                    saveable.Id = newId;
            }
        }

        /// <summary>
        /// Unregister saveable.
        /// </summary>
        public static void Remove(ISaveable saveable)
        {
            Instance(true)?.RemoveSaveable(saveable);
        }

        /// <summary>
        /// Register trash.
        /// </summary>
        public static void AddTrash(ITrash trash)
        {
            Instance()?.trash.Add(trash);
        }

        /// <summary>
        /// Unregister trash.
        /// </summary>
        public static void RemoveTrash(ITrash trash)
        {
            Instance(true)?.trash.Remove(trash);
        }

    }
}