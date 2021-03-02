using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// Fancy spawner.
    /// </summary>
    public class Instantiator : MonoBehaviour
    {

        [SerializeField]
        private Transform prefab = null;
        [SerializeField]
        private UnityEvent onSpawnEvent = default;

        public delegate void OnSpawn(Transform prefab);
        /// <summary>
        /// This event will be called when the spawner spawned some object.
        /// </summary>
        public OnSpawn onSpaw;

        /// <summary>
        /// Main instantiation.
        /// </summary>
        /// <returns>Instantiated object.</returns>
        protected virtual Transform Create(Transform prefab, Vector3 pos, Quaternion rot)
        {
            if (!enabled || !gameObject.activeSelf)
                return null;

            if (!prefab)
                return null;

            Transform clone = Instantiate(prefab, pos, rot);
            onSpawnEvent?.Invoke();
            onSpaw?.Invoke(clone);
            
            if (SaveCollector.Collectors[0] != null)
            {
                SaveCollector.Collectors[0].AddToCollector(clone);
            }

            return clone;
        }

        private void Spawn(Transform prefab)
        {
            Create(prefab, transform.position, transform.rotation);
        }

        private void Spawn(Transform prefab, Vector3 pos, Quaternion rot)
        {
            Create(prefab, pos, rot);
        }

        /// <summary>
        /// Spawn prefab.
        /// </summary>
        public void Spawn()
        {
            Create(prefab, transform.position, transform.rotation);
        }

        /// <summary>
        /// Spawn prefab with specified position and rotation.
        /// </summary>
        public void Spawn(Vector3 pos, Quaternion rot)
        {
            Create(prefab, pos, rot);
        }

    }
}