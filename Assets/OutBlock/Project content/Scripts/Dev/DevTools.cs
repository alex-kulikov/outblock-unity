using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace OutBlock
{

    /// <summary>
    /// Displayable cheat menu.
    /// </summary>
    public class DevTools : MonoBehaviour
    {

        private static DevTools instance;

        /// <summary>
        /// Fly through everything.
        /// </summary>
        public bool Noclip { get; set; } = false;

        /// <summary>
        /// Invisible to the AI.
        /// </summary>
        public bool Invisible { get; set; } = false;

        /// <summary>
        /// Become invincible and have limitless ammo.
        /// </summary>
        public bool God { get; set; } = false;

        /// <summary>
        /// Receive zero damage.
        /// </summary>
        public bool NoDamage { get; set; } = false;

        /// <summary>
        /// Disable any damage from falling.
        /// </summary>
        public bool NoFallDamage { get; set; } = false;

        /// <summary>
        /// Disable ragdoll for the player.
        /// </summary>
        public bool DisableRagdoll { get; set; } = false;

        private bool menuActive;
        private Rect windowRect = new Rect(Screen.width - 134, 0, 134, 315);

        private InputAction moveAction;
        private InputAction submitAction;
        private int selectedIndex = 0;
        private int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                if (selectedIndex < 0)
                    selectedIndex = 8;
                else if (selectedIndex > 8)
                    selectedIndex = 0;

                moveDelay = Time.unscaledTime + 0.2f;

                GUI.FocusControl(selectedIndex.ToString());
            }
        }
        private float moveDelay;

        private EventSystem eventSystem;

        private GUIStyle selected, normal, selectedButton, normalButton;

        /// <summary>
        /// Return instance of this class. Its singleton.
        /// </summary>
        /// <returns></returns>
        public static DevTools Instance()
        {
            if (instance == null)
            {
                DevTools inScene = FindObjectOfType<DevTools>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    GameObject go = new GameObject("DevTools");
                    DevTools dt = go.AddComponent<DevTools>();
                    instance = dt;
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

        private void OnEnable()
        {
            InputManager.Instance().onCheats += OpenWindow;

            if (submitAction != null)
                submitAction.performed += ctx => Submit();
            if (moveAction != null)
                moveAction.performed += ctx => Move();

            eventSystem = EventSystem.current;
        }

        private void OnDisable()
        {
            if (InputManager.Instance(true))
                InputManager.Instance().onCheats -= OpenWindow;

            if (submitAction != null)
                submitAction.performed -= ctx => Submit();
            if (moveAction != null)
                moveAction.performed -= ctx => Move();
        }

        private void OnGUI()
        {
            if (!menuActive)
                return;

            windowRect = GUI.Window(0, windowRect, WindowContent, "DevTools");
        }

        private void Init()
        {
            moveAction = InputManager.Instance().controls?.asset?.FindActionMap("UI")?.FindAction("Navigate");
            submitAction = InputManager.Instance().controls?.asset?.FindActionMap("UI")?.FindAction("Submit");
        }

        private void Move()
        {
            if (!menuActive)
                return;

            if (Time.unscaledTime > moveDelay && Mathf.Abs(moveAction.ReadValue<Vector2>().y) > 0.7f)
            {
                SelectedIndex -= (int)Mathf.Clamp(moveAction.ReadValue<Vector2>().y * 10, -1, 1);
            }
        }

        private void Submit()
        {
            if (!menuActive)
                return;

            switch (SelectedIndex)
            {
                case 0:
                    Noclip = !Noclip;
                    break;

                case 1:
                    Invisible = !Invisible;
                    break;

                case 2:
                    God = !God;
                    break;

                case 3:
                    NoDamage = !NoDamage;
                    break;

                case 4:
                    NoFallDamage = !NoFallDamage;
                    break;

                case 5:
                    DisableRagdoll = !DisableRagdoll;
                    break;

                case 6:
                    ReviveButton();
                    break;

                case 7:
                    OpenWindow();
                    break;

                case 8:
                    GameUI.Instance().Hide();
                    break;
            }
        }

        private void ReviveButton()
        {
            Player.InstanceForced?.Revive();
        }

        private void WindowContent(int windowID)
        {
            if (normal == null)
            {
                normal = new GUIStyle(GUI.skin.toggle);
                selected = new GUIStyle(GUI.skin.toggle);
                normal.fontStyle = FontStyle.Normal;
                selected.fontStyle = FontStyle.Bold;

                normalButton = new GUIStyle(GUI.skin.button);
                selectedButton = new GUIStyle(GUI.skin.button);
                normalButton.fontStyle = FontStyle.Normal;
                selectedButton.fontStyle = FontStyle.Bold;
            }

            Noclip = GUI.Toggle(new Rect(4, 23, 127, 32), Noclip, " Noclip", selectedIndex == 0 ? selected : normal);
            Invisible = GUI.Toggle(new Rect(4, 58, 127, 32), Invisible, " Invisible", selectedIndex == 1 ? selected : normal);
            God = GUI.Toggle(new Rect(4, 93, 127, 32), God, " God", selectedIndex == 2 ? selected : normal);
            NoDamage = GUI.Toggle(new Rect(4, 128, 127, 32), NoDamage, " NoDamage", selectedIndex == 3 ? selected : normal);
            NoFallDamage = GUI.Toggle(new Rect(4, 163, 127, 32), NoFallDamage, " NoFallDamage", selectedIndex == 4 ? selected : normal);
            DisableRagdoll = GUI.Toggle(new Rect(4, 198, 127, 32), DisableRagdoll, " DisableRagdoll", selectedIndex == 5 ? selected : normal);

            if (GUI.Button(new Rect(4, 219, 127, 28), "Revive", selectedIndex == 6 ? selectedButton : normalButton))
            {
                ReviveButton();
            }

            if (GUI.Button(new Rect(4, 251, 127, 28), "Close", selectedIndex == 7 ? selectedButton : normalButton))
            {
                OpenWindow();
            }

            if (GUI.Button(new Rect(4, 283, 127, 28), "Hide UI", selectedIndex == 8 ? selectedButton : normalButton))
            {
                GameUI.Instance().Hide();
            }
        }

        /// <summary>
        /// Show cheat menu.
        /// </summary>
        public void OpenWindow()
        {
            menuActive = !menuActive;
            Cursor.visible = menuActive || GameUI.pause;
            Cursor.lockState = (menuActive || GameUI.pause) ? CursorLockMode.None : CursorLockMode.Locked;

            eventSystem.sendNavigationEvents = !menuActive;
            eventSystem.currentInputModule.enabled = !menuActive;
        }

        /// <summary>
        /// Disable all cheats.
        /// </summary>
        public void DisableAll()
        {
            Noclip = false;
            Invisible = false;
            God = false;
            NoDamage = false;
            NoFallDamage = false;
            DisableRagdoll = false;
        }

    }
}