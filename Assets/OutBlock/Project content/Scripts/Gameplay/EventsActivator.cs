using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// This class allows you to link MonoBehaviour events to the UnityEvents.
    /// </summary>
    public class EventsActivator : MonoBehaviour
    {

        [SerializeField]
        private UnityEvent onAwake = null;
        [SerializeField]
        private UnityEvent onEnable = null;
        [SerializeField]
        private UnityEvent onStart = null;
        [SerializeField]
        private UnityEvent onDisable = null;
        [SerializeField]
        private UnityEvent onDestroy = null;

        private void Awake()
        {
            onAwake?.Invoke();
        }

        private void Start()
        {
            onStart?.Invoke();
        }

        private void OnEnable()
        {
            onEnable?.Invoke();
        }

        private void OnDisable()
        {
            onDisable?.Invoke();
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }
    }
}