using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// This trigger can activate, deactivate or switch the objects.
    /// </summary>
    public class ActivationTrigger : Trigger
    {

        /// <summary>
        /// SaveData for this type of triggers.
        /// </summary>
        public class ActivationTriggerSaveData : TriggerSaveData
        {

            /// <summary>
            /// States of the objects.
            /// </summary>
            public bool[] objectsState { get; private set; }

            public ActivationTriggerSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, bool done, bool reloading, bool[] objectsState) : base(id, pos, rot, active, enabled, done, reloading)
            {
                this.objectsState = new bool[objectsState.Length];
                for (int i = 0; i < objectsState.Length; i++)
                    this.objectsState = objectsState;
            }
        }

        /// <summary>
        /// Types of the actions this trigger can perform.
        /// </summary>
        public enum Actions { Activate, Deactivate, Switch };

        [SerializeField, Header("Activation trigger")]
        private GameObject[] objects = new GameObject[0];
        public GameObject[] Objects => objects;
        [SerializeField]
        private Actions action = Actions.Activate;

        private void Start()
        {
            Reload();
        }

        ///<inheritdoc/>
        protected override void TriggerAction(Transform other)
        {
            base.TriggerAction(other);
            foreach (GameObject go in objects)
            {
                if (action != Actions.Switch)
                    go.SetActive(action == Actions.Activate);
                else go.SetActive(!go.activeSelf);
            }
        }

        ///<inheritdoc/>
        public override void Reload()
        {
            base.Reload();

            if (done || action == Actions.Switch)
                return;

            foreach (GameObject go in objects)
            {
                go.SetActive(action == Actions.Deactivate);
            }
        }

        ///<inheritdoc/>
        public override SaveData Save()
        {
            bool[] objectsState = new bool[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i])
                    objectsState[i] = objects[i].activeSelf;
            }
            return new ActivationTriggerSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, done, reloading, objectsState);
        }

        ///<inheritdoc/>
        public override void Load(SaveData data)
        {
            base.Load(data);
            ActivationTriggerSaveData saveData = (ActivationTriggerSaveData)data;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i])
                    objects[i].SetActive(saveData.objectsState[i]);
            }
        }
    }
}