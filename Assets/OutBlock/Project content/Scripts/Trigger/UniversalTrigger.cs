using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{
    public class UniversalTrigger : Trigger
    {

        public enum SensorModes { Simple, Interactive };
        public enum ConditionModes { None, Item };
        public enum ActuationModes { Basic, Switch };
        public enum SwitchModes { Loop, PingPong, Once };

        [System.Serializable]
        public class Actuator
        {
            [SerializeField]
            private Trigger[] triggers = new Trigger[0];
            public Trigger[] Triggers => triggers;
            [SerializeField, Space]
            private UnityEvent onTrigger = default;
            public UnityEvent OnTrigger => onTrigger;
        }

        [SerializeField, Tooltip("Simple collision or interactive player input?")]
        private SensorModes sensorMode = SensorModes.Simple;
        [SerializeField]
        private GameObject handIK = null;

        [SerializeField, Tooltip("Condition to fire the trigger")]
        private ConditionModes conditionMode = ConditionModes.None;
        [SerializeField, Tooltip("Target item in the player inventory.")]
        private string item = "";
        [SerializeField]
        private int requiredItemCount = 1;

        [SerializeField, Tooltip("Basic actuation or actuate different states?")]
        private ActuationModes actuationMode = ActuationModes.Basic;
        public ActuationModes ActuationMode => actuationMode;
        [SerializeField]
        private SwitchModes switchMode = SwitchModes.Loop;
        [SerializeField]
        private int initState = 0;
        [SerializeField]
        private Actuator basicActuator = new Actuator();
        public Actuator BasicActuator => basicActuator;
        [SerializeField]
        private Actuator[] switchStates = new Actuator[2];
        public Actuator[] SwitchStates => switchStates;

        private int currentState;
        public int CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;

                if (currentState >= switchStates.Length)
                {
                    switch (switchMode)
                    {
                        case SwitchModes.Loop:
                            currentState = 0;
                            break;

                        case SwitchModes.PingPong:
                            switchCount *= -1;
                            currentState = switchStates.Length - 2;
                            break;

                        case SwitchModes.Once:
                            done = true;
                            currentState = switchStates.Length - 1;
                            break;
                    }
                }
                else if (currentState < 0 && switchMode == SwitchModes.PingPong)
                {
                    currentState = 1;
                    switchCount *= -1;
                }
            }
        }

        private int switchCount = 1;

        private void Awake()
        {
            if (handIK)
                handIK.SetActive(false);

            currentState = initState;
        }

        private void OnValidate()
        {
            if (initState >= switchStates.Length)
                initState = switchStates.Length - 1;
            else if (initState < 0)
                initState = 0;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
            {
                base.OnTriggerEnter(other);
            }
            else
            {
                if (tags.Contains(other.tag) && !Player.interaction.Contains(this))
                    Player.interaction.Add(this);
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
                base.OnTriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
            {
                base.OnTriggerEnter(other);
            }
            else
            {
                if (tags.Contains(other.tag))
                    Player.interaction.Remove(this);
            }
        }

        private void DisableHandIK()
        {
            if (handIK)
                handIK.SetActive(false);
        }

        private void Actuate(Transform other)
        {
            if (actuationMode == ActuationModes.Basic)
            {
                foreach (Trigger trigger in basicActuator.Triggers)
                    trigger?.Activate(other);

                basicActuator.OnTrigger?.Invoke();
            }
            else
            {
                foreach (Trigger trigger in switchStates[currentState].Triggers)
                    trigger?.Activate(other);

                switchStates[currentState].OnTrigger?.Invoke();

                CurrentState += switchCount;
            }

            if (sensorMode == SensorModes.Interactive && handIK)
            {
                handIK.SetActive(true);
                Invoke("DisableHandIK", 0.5f);
            }
        }

        protected override void TriggerAction(Transform other)
        {
            if (conditionMode == ConditionModes.Item)
            {
                if (Player.Instance.Inventory.HasItem(item, requiredItemCount))
                {
                    ConsumeItem();
                    base.TriggerAction(other);
                    Actuate(other);
                }
                else
                {
                    ShowMessage();
                }
            }
            else
            {
                Actuate(other);
            }
        }

        /// <summary>
        /// Use player's item.
        /// </summary>
        public void ConsumeItem()
        {
            Player.Instance?.Inventory.ConsumeItem(item, requiredItemCount);
        }

        /// <summary>
        /// Show consumption item.
        /// </summary>
        public void ShowMessage()
        {
            GameUI.Instance().ShowMessage(string.Format("You need {0}x of {1}", requiredItemCount, item));
            StopCoroutine("EndCollision");
            done = false;
            colliding = false;
            reloading = true;
            Invoke("Reload", Random.Range(reloadTime.x, reloadTime.y));
        }
    }
}