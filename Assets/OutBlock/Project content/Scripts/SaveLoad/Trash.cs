using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Basic ITrash implemented class.
    /// </summary>
    public class Trash : MonoBehaviour, ITrash
    {

        [SerializeField]
        private bool disposeMe = true;
        ///<inheritdoc/>
        public bool DisposeMe => disposeMe;

        private void OnEnable()
        {
            SaveLoad.AddTrash(this);
        }

        private void OnDisable()
        {
            SaveLoad.RemoveTrash(this);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            SaveLoad.RemoveTrash(this);
            Destroy(gameObject);
        }

    }
}