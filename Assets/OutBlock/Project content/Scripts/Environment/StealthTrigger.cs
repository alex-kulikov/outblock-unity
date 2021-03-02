using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    public class StealthTrigger : MonoBehaviour, ISaveable
    {

        public class StealthTriggerSaveData : SaveData
        {
            public StealthSettings Settings { get; set; }

            public StealthTriggerSaveData (int id, Vector3 pos, Vector3 rot, bool active, bool enabled, StealthSettings settings) : base(id, pos, rot, active, enabled)
            {
                Settings = settings;
            }
        }

        [System.Serializable]
        public class StealthSettings
        {

            [SerializeField]
            private bool allowMovement = true;
            public bool AllowMovement => allowMovement;
            [SerializeField]
            private bool crouchOnly = true;
            public bool CrouchOnly => crouchOnly;
            [SerializeField]
            private bool bushSound = true;
            public bool BushSound => bushSound;
            [SerializeField]
            private bool costumeEffect = true;
            public bool CostumeEffect => costumeEffect;

            private static StealthSettings defaultSettings;
            public static StealthSettings Default
            {
                get
                {
                    return defaultSettings ?? new StealthSettings()
                    {
                        allowMovement = true,
                        crouchOnly = true,
                        bushSound = true,
                        costumeEffect = true
                    };
                }
            }

            public StealthSettings()
            {
            }

            public StealthSettings (StealthSettings settings)
            {
                allowMovement = settings.allowMovement;
                crouchOnly = settings.crouchOnly;
                bushSound = settings.bushSound;
                costumeEffect = settings.costumeEffect;
            }

        }

        [SerializeField]
        private StealthSettings settings = new StealthSettings();
        public StealthSettings Settings => settings;

        public int Id { get; set; } = -1;
        public GameObject GO => gameObject;

        private bool oldCostumeEffect;

        private void OnEnable()
        {
            UpdateLayer();
        }

        private void OnValidate()
        {
            if (oldCostumeEffect != settings.CostumeEffect)
            {
                UpdateLayer();
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void UpdateLayer()
        {
            oldCostumeEffect = settings.CostumeEffect;
            gameObject.layer = settings.BushSound ? 9 : 11;
        }

        public void Register()
        {
            SaveLoad.Add(this);
        }

        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public SaveData Save()
        {
            return new StealthTriggerSaveData(Id, transform.position, transform.eulerAngles, gameObject.activeSelf, enabled, new StealthSettings(Settings));
        }

        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);
            if (data is StealthTriggerSaveData saveData)
            {
                settings = saveData.Settings;
                UpdateLayer();
            }
        }
    }
}