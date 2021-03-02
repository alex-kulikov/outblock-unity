using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OutBlock
{
    public class Trigger : MonoBehaviour, ISaveable
    {

        /// <summary>
        /// This class stores trigger settings. This can be passed to the triggers to change their settings. Used by grenades.
        /// </summary>
        [System.Serializable]
        public class TriggerSettings
        {

            [SerializeField]
            private bool onCollision = true;
            public bool OnCollision => onCollision;
            [SerializeField]
            private bool onEnter = false;
            public bool OnEnter => onEnter;
            [SerializeField]
            private List<string> tags = new List<string>() { "Player" };
            public List<string> Tags => tags;
            [SerializeField]
            private bool once = false;
            public bool Once => once;
            [SerializeField]
            private Vector2 reloadTime = Vector2.one;
            public Vector2 ReloadTime => reloadTime;
            [SerializeField]
            private float destroyTime = 0.1f;
            public float DestroyTime => destroyTime;

        }

        public class TriggerSaveData : Bonus.BonusSaveData
        {

            public bool reloading { get; private set; }

            public TriggerSaveData(int id, Vector3 pos, Vector3 rot, bool active,bool enabled, bool done, bool reloading) : base(id, pos, rot, active, enabled, done)
            {
                this.reloading = reloading;
            }
        }

        [SerializeField, Header("Trigger")]
        protected bool onCollision = true;
        [SerializeField]
        protected bool onEnter = false;
        [SerializeField]
        protected List<string> tags = new List<string>() { "Player" };
        [SerializeField]
        private bool once = false;
        [SerializeField]
        protected Vector2 reloadTime = Vector2.one;
        [SerializeField]
        private AudioClip sound = null;

        protected bool done;
        protected bool reloading;
        protected bool colliding;

        ///<inheritdoc/>
        public int Id { get; set; } = -1;
        ///<inheritdoc/>
        public GameObject GO => gameObject;

        public static bool ShowLines { get; set; } = true;

        private void OnDestroy()
        {
            Unregister();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!onCollision || !onEnter)
                return;

            if (tags.Contains(other.tag))
                Activate(other);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!onCollision || onEnter)
                return;

            if (tags.Contains(other.tag))
                Activate(other);
        }

        protected IEnumerator EndCollision()
        {
            yield return 0;

            if (once)
                done = true;
            else
            {
                reloading = true;
                Invoke("Reload", Random.Range(reloadTime.x, reloadTime.y));
            }

            colliding = false;
        }

        /// <summary>
        /// Action of the trigger.
        /// </summary>
        protected virtual void TriggerAction(Transform other)
        {
        }

        /// <summary>
        /// Set trigger settings.
        /// </summary>
        public void Set(TriggerSettings settings)
        {
            onCollision = settings.OnCollision;
            once = settings.Once;
            onEnter = settings.OnEnter;
            tags = settings.Tags;
            reloadTime = settings.ReloadTime;
        }

        /// <summary>
        /// Is trigger can be activated?
        /// </summary>
        public bool CanActivate()
        {
            return enabled && !reloading && !done;
        }

        /// <summary>
        /// Activate this trigger after the collision.
        /// </summary>
        /// <param name="other"></param>
        public void Activate(Collider other)
        {
            if (!CanActivate())
                return;

            if (sound && !colliding)
                Utils.PlaySound(transform.position, sound, 0.75f, 1);

            colliding = true;
            StopCoroutine("EndCollision");
            StartCoroutine("EndCollision");

            if (other.CompareTag("Player"))
                Player.Instance?.PlayerEvents.OnTrigger?.Invoke();

            TriggerAction(other.transform);
        }

        /// <summary>
        /// Activate this trigger by the transform.
        /// </summary>
        /// <param name="other"></param>
        public void Activate(Transform other)
        {
            if (!CanActivate())
                return;

            if (sound && !colliding)
                Utils.PlaySound(transform.position, sound, 0.75f, 1);

            colliding = true;
            StopCoroutine(EndCollision());
            StartCoroutine(EndCollision());

            if (other != null && other.tag == "Player")
                Player.Instance.PlayerEvents.OnTrigger?.Invoke();

            TriggerAction(other);
        }

        /// <summary>
        /// Reload trigger state.
        /// </summary>
        public virtual void Reload()
        {
            reloading = false;
        }

        #region SaveLoad
        /// <inheritdoc/>
        public virtual void Register()
        {
            SaveLoad.Add(this);
        }

        /// <inheritdoc/>
        public virtual void Unregister()
        {
            SaveLoad.Remove(this);
        }

        ///<inheritdoc/>
        public virtual SaveData Save()
        {
            return new TriggerSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, done, reloading);
        }

        ///<inheritdoc/>
        public virtual void Load(SaveData data)
        {
            CancelInvoke();

            SaveLoadUtils.BasicLoad(this, data);

            TriggerSaveData saveData = (TriggerSaveData)data;
            if (saveData != null)
            {
                done = saveData.done;
                reloading = saveData.reloading;
            }

            if (reloading)
            {
                Invoke("Reload", Random.Range(reloadTime.x, reloadTime.y));
            }
        }
        #endregion

#if UNITY_EDITOR
        #region Menu items
        [MenuItem("OutBlock/Gizmos/Hide \u2044 Show Trigger Gizmos")]
        public static void AddAIZone()
        {
            ShowLines = !ShowLines;
        }
        #endregion
#endif

    }
}