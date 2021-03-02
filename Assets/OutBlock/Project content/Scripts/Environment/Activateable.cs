using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

namespace OutBlock
{

    /// <summary>
    /// Base class for the objects that can be activated or deactivated in realtime. Used as doors.
    /// Activation and deactivation stands for the internal state not for the GameObject.
    /// </summary>
    public abstract class Activateable : MonoBehaviour, IActivatable, IDeactivatable, ISaveable
    {

        /// <summary>
        /// Activateable SaveData.
        /// </summary>
        public class ActivateableSaveData : SaveData
        {

            /// <summary>
            /// Is it open?
            /// </summary>
            public bool open { get; private set; }

            /// <summary>
            /// Is it active?
            /// </summary>
            public bool isActive { get; private set; }

            /// <summary>
            /// Is it done interacting?
            /// </summary>
            public bool done { get; private set; }

            public ActivateableSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, bool open, bool isActive, bool done) : base(id, pos, rot, active, enabled)
            {
                this.open = open;
                this.isActive = active;
                this.done = done;
            }
        }

        ///<inheritdoc/>
        public abstract void Activate(ISignalSource source);

        ///<inheritdoc/>
        public abstract void Deactivate(ISignalSource source);

        [SerializeField, Header("Activateable")]
        private bool active = true;
        /// <summary>
        /// Is it active right now?
        /// </summary>
        public bool Active
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
                if (navObstacle)
                {
                    if (targetTeam == 0)
                        navObstacle.enabled = true;
                    else navObstacle.enabled = !active;
                }

                if (!active)
                {
                    Reload();
                    Open = false;
                }
            }
        }
        [SerializeField, Space]
        protected NavMeshObstacle navObstacle;
        [SerializeField]
        private Teams targetTeam = Teams.Player;
        public Teams TargetTeam => targetTeam;
        [SerializeField]
        private UnityEvent onOpened = default;
        [SerializeField]
        private UnityEvent onClosed = default;

        private bool open;
        /// <summary>
        /// Is it open?
        /// </summary>
        protected bool Open
        {
            get
            {
                return open;
            }

            set
            {
                if (open == value)
                    return;

                open = value;
                /*if (navObstacle && targetTeam == Teams.Player)
                {
                    navObstacle.enabled = open;
                }*/

                if (open)
                    onOpened?.Invoke();
                else onClosed?.Invoke();
            }
        }

        /// <summary>
        /// Is it done interacting?
        /// </summary>
        protected bool done;

        public int Id { get; set; } = -1;
        public GameObject GO => gameObject;

        /// <summary>
        /// Reload signals.
        /// </summary>
        protected virtual void Reload() { }

        private void OnEnable()
        {
            if (navObstacle)
            {
                navObstacle.enabled = !active;
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        #region SaveLoad
        /// <inheritdoc/>
        public void Register()
        {
            SaveLoad.Add(this);
        }

        /// <inheritdoc/>
        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public SaveData Save()
        {
            return new ActivateableSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, Open, Active, done);
        }

        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            ActivateableSaveData saveData = (ActivateableSaveData)data;
            Open = saveData.open;
            Active = saveData.isActive;
            done = saveData.done;
            Reload();
        }
        #endregion
    }
}