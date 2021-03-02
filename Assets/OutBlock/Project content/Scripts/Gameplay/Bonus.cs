using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Base class for the pickable bonus
    /// </summary>
    public class Bonus : MonoBehaviour, ISaveable
    {

        public class BonusSaveData : SaveData
        {

            public bool done { get; private set; }

            public BonusSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, bool done) : base(id, pos, rot, active, enabled)
            {
                this.done = done;
            }

        }

        [SerializeField, Header("Bonus")]
        private AudioClip sound = null;
        [SerializeField]
        private Vector2 lifeTime = Vector2.zero;
        [SerializeField]
        private bool count = false;
        [SerializeField]
        private float rotSpeed = 60;

        ///<inheritdoc/>
        public int Id { get; set; } = -1;
        ///<inheritdoc/>
        public GameObject GO => gameObject;

        private bool colliding;
        protected bool done;

        private void OnEnable()
        {
            if (lifeTime.x > 0 && lifeTime.y > 0)
            {
                Invoke("Disable", Random.Range(lifeTime.x, lifeTime.y));
            }
        }

        private void OnDisable()
        {
            CancelInvoke("Disable");
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Update()
        {
            transform.Rotate(transform.up, rotSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider coll)
        {
            if (colliding)
                return;

            colliding = true;
            if (coll.CompareTag("Player"))
            {
                PickUp(coll.gameObject);
            }
        }

        private void OnTriggerExit(Collider coll)
        {
            colliding = false;
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }

        private void PickedUp()
        {
            if (sound)
                Utils.PlaySound(transform.position, sound, 0.4f, 1);

            colliding = false;
        }

        /// <summary>
        /// Fires when the player picks up this object.
        /// </summary>
        protected void PickUp(GameObject go)
        {
            if (done)
                return;

            if (count && Player.Instance)
                Player.Instance.BonusCount++;

            Player.Instance?.PlayerEvents.OnPickUp?.Invoke();

            OnPickUp(go);
        }

        /// <summary>
        /// What to do after the player picked up this object.
        /// </summary>
        protected virtual void OnPickUp(GameObject go)
        {
            done = true;
            gameObject.SetActive(false);
            PickedUp();
        }

        #region Saveable
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

        ///<inheritdoc/>
        public SaveData Save()
        {
            return new BonusSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, done);
        }

        ///<inheritdoc/>
        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            BonusSaveData saveData = (BonusSaveData)data;
            if (saveData != null)
            {
                done = saveData.done;
            }
        }
        #endregion

    }
}