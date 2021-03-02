using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OutBlock
{

    /// <summary>
    /// Contains all controls and gives you straight access to them. Also manages save and load.
    /// </summary>
    public class InputManager : MonoBehaviour
    {

        [System.Serializable]
        private class BindingWrapper
        {
            public List<Binding> overrides;

            public BindingWrapper(List<Binding> overrides)
            {
                this.overrides = overrides;
            }
        }

        [System.Serializable]
        private class Binding
        {

            public string id;
            public string path;

            public Binding(string id, string path)
            {
                this.id = id;
                this.path = path;
            }
        }

        /// <summary>
        /// Current <see cref="Controls"/>.
        /// </summary>
        public Controls controls { get; private set; }

        /// <summary>
        /// Look vector.
        /// </summary>
        public Vector2 Look { get; private set; }

        /// <summary>
        /// Movement vector.
        /// </summary>
        public Vector2 Move { get; private set; }

        /// <summary>
        /// Change weapon value.
        /// </summary>
        public float ChangeWeapon { get; private set; }

        /// <summary>
        /// Jump button is pressed now.
        /// </summary>
        public bool Jump { get; private set; }

        /// <summary>
        /// Crouch button is pressed now.
        /// </summary>
        public bool Crouch { get; private set; }

        /// <summary>
        /// Run button is pressed now.
        /// </summary>
        public bool Run { get; private set; }

        /// <summary>
        /// Aim button is pressed now.
        /// </summary>
        public bool Aim { get; private set; }

        /// <summary>
        /// Fire button is pressed now.
        /// </summary>
        public bool Fire { get; private set; }

        public delegate void OnCrouchSwitch();
        /// <summary>
        /// CrouchSwitch button is performed.
        /// </summary>
        public OnCrouchSwitch onCrouchSwitch;

        public delegate void OnAction();
        /// <summary>
        /// Action button key is performed.
        /// </summary>
        public OnAction onAction;

        public delegate void OnSwitchCamera();
        /// <summary>
        /// SwitchCamera button is performed.
        /// </summary>
        public OnSwitchCamera onSwitchCamera;

        public delegate void OnReload();
        /// <summary>
        /// Reload button is performed.
        /// </summary>
        public OnReload onReload;

        public delegate void OnCheats();
        /// <summary>
        /// Cheats button is performed.
        /// </summary>
        public OnCheats onCheats;

        public delegate void OnKick();
        /// <summary>
        /// Kick button is performed.
        /// </summary>
        public OnKick onKick;

        public delegate void OnPause();
        /// <summary>
        /// Pause button is performed.
        /// </summary>
        public OnPause onPause;

        public delegate void OnHide();
        /// <summary>
        /// Hide UI button is performed.
        /// </summary>
        public OnHide onHide;

        /// <summary>
        /// Current gamepad device name.
        /// </summary>
        public static string currentJoystick { get; private set; }

        /// <summary>
        /// Is player using gamepad now?
        /// </summary>
        public static bool joyNow { get; private set; }

        private static InputManager instance;
        /// <summary>
        /// InputManager instance. Singleton.
        /// </summary>
        /// <param name="onDestroy">Set true when calling on OnDisable/OnDestroy to prevent error while unloading scene or game</param>
        public static InputManager Instance(bool onExit = false)
        {
            if (instance == null)
            {
                if (onExit)
                    return null;

                InputManager inScene = FindObjectOfType<InputManager>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    GameObject go = new GameObject("InputManager");
                    InputManager im = go.AddComponent<InputManager>();
                    instance = im;
                }
            }
            DontDestroyOnLoad(instance.gameObject);
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Init();
            }
        }

        private void Update()
        {
            #region Last device
            InputDevice device = InputSystem.devices.OrderByDescending(x => x.lastUpdateTime).FirstOrDefault();
            if (device != null)
                joyNow = device == Gamepad.current;
            else joyNow = false;

            currentJoystick = device.displayName;
            #endregion

            Look = controls.Player.Look.ReadValue<Vector2>();
            Move = controls.Player.Move.ReadValue<Vector2>();
            ChangeWeapon = controls.Player.ChangeWeapon.ReadValue<float>();

            Jump = controls.Player.Jump.ReadValue<float>() > 0;
            Crouch = controls.Player.Crouch.ReadValue<float>() > 0;
            Run = controls.Player.Run.ReadValue<float>() > 0;
            Aim = controls.Player.Aim.ReadValue<float>() > 0;
            Fire = controls.Player.Fire.ReadValue<float>() > 0;
        }

        private void Init()
        {
            controls = new Controls();
            controls.Enable();

            controls.Player.CrouchSwitch.performed += ctx => onCrouchSwitch?.Invoke();
            controls.Player.Action.performed += ctx => onAction?.Invoke();
            controls.Player.SwitchCamera.performed += ctx => onSwitchCamera?.Invoke();
            controls.Player.Reload.performed += ctx => onReload?.Invoke();
            controls.Player.Kick.performed += ctx => onKick?.Invoke();
            controls.UI.Cheats.performed += ctx => onCheats?.Invoke();
            controls.UI.Pause.performed += ctx => onPause?.Invoke();
            controls.UI.Hide.performed += ctx => onHide?.Invoke();

            Load();
        }

        /// <summary>
        /// Save keybindings.
        /// </summary>
        public void Save()
        {
            List<Binding> overrides = new List<Binding>();
            foreach(InputActionMap map in controls.asset.actionMaps)
            {
                foreach(InputBinding binding in map.bindings)
                {
                    if (!string.IsNullOrEmpty(binding.overridePath))
                    {
                        overrides.Add(new Binding(binding.id.ToString(), binding.overridePath));
                    }
                }
            }

            BindingWrapper toSave = new BindingWrapper(overrides);
            PlayerPrefs.SetString("Controls", JsonUtility.ToJson(toSave));
        }

        /// <summary>
        /// Load keybindings.
        /// </summary>
        public void Load()
        {
            BindingWrapper load = JsonUtility.FromJson<BindingWrapper>(PlayerPrefs.GetString("Controls"));

            if (load == null || load.overrides.Count <= 0)
                return;

            foreach(InputActionMap map in controls.asset.actionMaps)
            {
                UnityEngine.InputSystem.Utilities.ReadOnlyArray<InputBinding> bindings = map.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    Binding overrideBinding = load.overrides.SingleOrDefault(x => x.id == bindings[i].id.ToString());
                    if (overrideBinding != null)
                    {
                        map.ApplyBindingOverride(i, new InputBinding { overridePath = overrideBinding.path });
                    }
                }
            }
        }

        /// <summary>
        /// Get <see cref="InputAction"/> key by name and binging index.
        /// </summary>
        public string GetActionKey(string name, int bindingIndex)
        {
            string result = "";
            InputAction action = controls.asset.FindAction(name);
            if (action != null)
            {
                result = action.GetBindingDisplayString(bindingIndex);
            }
            return result;
        }

    }
}