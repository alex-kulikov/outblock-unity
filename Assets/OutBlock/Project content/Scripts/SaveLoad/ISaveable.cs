using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Object that can be saved.
    /// </summary>
    public interface ISaveable
    {

        /// <summary>
        /// Save ID of the object.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Link to the GameObject,
        /// </summary>
        GameObject GO { get; }

        /// <summary>
        /// Register this object to the SaveLoad system. Use in the OnEnable().
        /// </summary>
        void Register();

        /// <summary>
        /// Unregister this object from the SaveLoad system. Use in the OnDisable().
        /// </summary>
        void Unregister();

        /// <summary>
        /// Save data of the object.
        /// </summary>
        SaveData Save();

        /// <summary>
        /// Load saved data.
        /// </summary>
        void Load(SaveData data);
    }

    /// <summary>
    /// Object that will be disposed in the loading sequence.
    /// </summary>
    public interface ITrash
    {
        /// <summary>
        /// Should this object be disposed?
        /// </summary>
        bool DisposeMe { get; }

        /// <summary>
        /// Dispose object.
        /// </summary>
        void Dispose();
    }
}