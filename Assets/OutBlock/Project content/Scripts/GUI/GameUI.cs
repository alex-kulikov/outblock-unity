using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using System.Text;
using UnityEngine.SceneManagement;

namespace OutBlock
{

    /// <summary>
    /// Main game user interface.
    /// </summary>
    public class GameUI : MonoBehaviour
    {

        [SerializeField]
        private CanvasGroup canvasGroup = null;
        [SerializeField, Header("Pause menu")]
        private GameObject pauseWindow = null;
        [SerializeField]
        private string menuScene = "Menu";
        [SerializeField, Header("Death screen")]
        private GameObject deathScreen = null;
        [SerializeField, Header("Player UI")]
        private GameObject playerUI = null;
        [SerializeField]
        private Image crosshair = null;
        [SerializeField]
        private CanvasGroup bonusHolder = null;
        [SerializeField]
        private Text bonusCounter = null;
        [SerializeField]
        private RawImage hpSegments = null;
        public RawImage HPSegments => hpSegments;
        [SerializeField]
        private Image hpBar = null;
        public Image HPBar => hpBar;
        [SerializeField]
        private Text inventoryItems = null;
        public Text InventoryItems => inventoryItems;
        [SerializeField]
        private Text ammo = null;
        [SerializeField]
        private Text weaponName = null;
        [SerializeField]
        private Image stealth = null;
        [SerializeField]
        private GameObject cooldownHolder = null;
        [SerializeField]
        private Image cooldownFill = null;
        [SerializeField]
        private Text takedown = null;
        [SerializeField]
        private Text saveLoadIndicator = null;
        [SerializeField]
        private Text messageText = null;
        [SerializeField]
        private Image black = null;
        private float blackAlpha = 0;
        public float BlackAlpha
        {
            get
            {
                return blackAlpha;
            }
            set
            {
                if (blackAlpha == value)
                    return;

                blackAlpha = value;
                Color newColor = black.color;
                newColor.a = blackAlpha;
                black.color = newColor;
            }
        }
        [SerializeField, Header("Camera UI")]
        private GameObject cameraUI = null;
        [SerializeField]
        private Text cameraControls = null;
        [SerializeField, Header("Audio")]
        private AudioSource alertSound = null;
        [SerializeField]
        private AudioSource alarmSound = null;
        [SerializeField]
        private SoundContainer soundContainer = null;
        public SoundContainer SoundContainer => soundContainer;
        [SerializeField, Header("Enemy healthbar")]
        private GameObject enemyHPHolder = null;
        [SerializeField]
        private Image enemyHP = null;

        /// <summary>
        /// Is game paused?
        /// </summary>
        public static bool pause { get; private set; }

        /// <summary>
        /// Delayed pause. Switch its state within 2 frames to the <see cref="pause"/>
        /// </summary>
        public static bool pauseDelayed { get; private set; }

        /// <summary>
        /// Last enemy that the player hit.
        /// </summary>
        public AIEntity lastEnemy { get; private set; }

        private float messageTime;
        private bool pauseUpdating;
        private float bonusFadeOutTime;
        private float saveLoadIndicatorTime;
        private float lastEnemyTimeout;

        private const float messageFadeoutTime = 4;
        private const float lastEnemyCooldown = 3;

        private static GameUI instance;
        /// <summary>
        /// Instance of the GameUI. Only one instance in the scene.
        /// </summary>
        /// <returns></returns>
        public static GameUI Instance()
        {
            if (instance == null)
            {
                GameUI inScene = FindObjectOfType<GameUI>();
                if (inScene)
                {
                    instance = inScene;
                }
                else
                {
                    Transform clone = Instantiate(Resources.Load<Transform>("GUI"), Vector3.zero, Quaternion.identity);
                    GameUI gui = clone.GetComponent<GameUI>();
                    instance = gui;
                }
            }
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
            }
        }

        private void Start()
        {
            pauseDelayed = pause;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            messageText.canvasRenderer.SetAlpha(0);

            TimelineManager.Instance().onStart += PauseHidden;
            TimelineManager.Instance().onEnd += UnPauseHidden;
            InputManager.Instance().onPause += PauseButton;
            InputManager.Instance().onHide += Hide;
            SaveLoad.Instance().onProcessingUpdate += SaveLoadPause;
        }

        private void OnDisable()
        {
            if (InputManager.Instance(true))
            {
                InputManager.Instance().onPause -= PauseButton;
                InputManager.Instance().onHide -= Hide;
            }

            if (TimelineManager.Instance(true))
            {
                TimelineManager.Instance(true).onStart -= PauseHidden;
                TimelineManager.Instance(true).onEnd -= UnPauseHidden;
            }
            if (SaveLoad.Instance(true))
            {
                SaveLoad.Instance().onProcessingUpdate -= SaveLoadPause;
            }
        }

        private void Update()
        {
            if (pauseDelayed != pause && !pauseUpdating)
            {
                StartCoroutine(UpdatePause());
            }

            if (saveLoadIndicatorTime > 0)
            {
                saveLoadIndicatorTime -= Time.unscaledDeltaTime;
                Color color = saveLoadIndicator.color;
                color.a = saveLoadIndicatorTime;
                saveLoadIndicator.color = color;
            }

            if (lastEnemyTimeout > 0)
            {
                lastEnemyTimeout -= Time.deltaTime;
                enemyHP.fillAmount = lastEnemy ? lastEnemy.HPRatio : 0;

                if (lastEnemyTimeout <= 0)
                    lastEnemy = null;
            }
            enemyHPHolder.SetActive(lastEnemyTimeout > 0);

            if (bonusFadeOutTime > 0)
                bonusFadeOutTime -= Time.deltaTime;
            bonusHolder.alpha = bonusFadeOutTime;

            hpSegments.gameObject.SetActive(!(pause && !pauseWindow.activeSelf));

            if (messageTime > 0)
            {
                messageTime -= Time.deltaTime;
                float alpha = Mathf.Clamp01(messageTime);
                messageText.canvasRenderer.SetAlpha(alpha);
            }

            if (pauseWindow.activeSelf)
                bonusHolder.alpha = 1;

            AIManager.States state = AIManager.Instance().GetAIStatus(out float cooldown);
            switch (state)
            {
                case AIManager.States.Idle:
                    stealth.color = Color.clear;
                    alertSound.volume = 0;
                    alarmSound.volume = 0;
                    break;

                case AIManager.States.Attention:
                    if (stealth.color == Color.clear)
                        soundContainer.PlaySound("alert");
                    stealth.color = new Color(1, 1, 1, Mathf.PingPong(Time.time * 2, 1));
                    alertSound.volume = 0.06f;
                    alarmSound.volume = 0;
                    break;

                case AIManager.States.Aggresion:
                    stealth.color = Color.red;
                    alertSound.volume = 0;
                    alarmSound.volume = 0.06f;
                    break;
            }

            cooldownHolder.SetActive(cooldown > 0 && cooldown < 1);
            cooldownFill.fillAmount = cooldown;
        }

        private void PauseButton()
        {
            if (TimelineManager.playing)
            {
                TimelineManager.Instance().SkipScene();
            }
            else Pause();
        }

        private void SaveLoadPause(bool processing)
        {
            if (processing)
                PauseHidden();
            else UnPauseHidden();
        }

        private IEnumerator UpdatePause()
        {
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime * 2);
            pauseDelayed = pause;
            pauseUpdating = false;
        }

        private void PauseHidden()
        {
            pause = true;
            Time.timeScale = pause ? 0 : 1;
        }

        private void UnPauseHidden()
        {
            if (TimelineManager.Instance() && TimelineManager.playing)
                return;

            pause = false;
            Time.timeScale = pause ? 0 : 1;
        }

        private IEnumerator SelectObjectSequence(GameObject go)
        {
            yield return 0;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }

        /// <summary>
        /// Select element on the UI.
        /// </summary>
        public void SelectObject(GameObject go)
        {
            StartCoroutine(SelectObjectSequence(go));
        }

        /// <summary>
        /// Set crosshair state.
        /// </summary>
        public void SetCrosshair(Weapon weapon, bool aiming)
        {
            bool showCrosshair = !(weapon is Weapon_Empty) && aiming;
            crosshair.color = Color.Lerp(crosshair.color, showCrosshair ? Color.white : Color.clear, Time.deltaTime * 10);
            crosshair.transform.localScale = Vector3.Lerp(crosshair.transform.localScale, Vector3.one * (aiming ? 1 : 1.5f), Time.deltaTime * 10);
        }

        /// <summary>
        /// Set current weapon.
        /// </summary>
        public void SetWeapon(Weapon weapon)
        {
            weaponName.text = weapon.Name;
            ammo.text = weapon.GetAmmoUI();
        }

        /// <summary>
        /// Hide crosshair.
        /// </summary>
        public void HideCrosshair()
        {
            crosshair.color = Color.clear;
            crosshair.transform.localScale = Vector3.one * 1.5f;
        }

        /// <summary>
        /// Set active action.
        /// </summary>
        /// <param name="action">Is player can perform any action?</param>
        /// <param name="takedown">Is player can takedown any AI?</param>
        /// <param name="kick">Is player can kick any AI?</param>
        public void SetAction(bool action, bool takedown, bool kick)
        {
            bool show = action || takedown || kick;
            this.takedown.gameObject.SetActive(show);

            if (!show)
                return;

            string actionKey, kickKey;
            actionKey = string.Format("[{0}]", InputManager.Instance().GetActionKey("Action", 0));
            kickKey = string.Format("[{0}]", InputManager.Instance().GetActionKey("Reload", 0));
            string joyTakedown = InputManager.currentJoystick == "Xbox Controller" ? "(X)" : "(□)";

            if (takedown || kick)
            {
                this.takedown.text = "";
                if (takedown)
                    this.takedown.text += (InputManager.joyNow ? joyTakedown : actionKey) + " Takedown";
                if (kick)
                    this.takedown.text += '\n' + (InputManager.joyNow ? "(RB)" : kickKey) + " Kick";
            }
            else
            {
                this.takedown.text = (InputManager.joyNow ? joyTakedown : actionKey) + " Use";
            }
        }

        /// <summary>
        /// Set count of the picked up bonuses.
        /// </summary>
        public void SetBonusCount(int bonusCount)
        {
            bonusFadeOutTime = 2.5f;
            bonusCounter.text = bonusCount.ToString();
        }

        /// <summary>
        /// Set death screen state.
        /// </summary>
        public void SetDeathScreen(bool active)
        {
            deathScreen.SetActive(active);
            if (!active)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Set interactive camera UI state.
        /// </summary>
        public void SetCameraUI(bool active)
        {
            playerUI.SetActive(!active);
            cameraUI.SetActive(active);

            string actionKey = string.Format("[{0}]", InputManager.Instance().GetActionKey("Action", 0));
            string joyActionKey = InputManager.currentJoystick == "Xbox Controller" ? "(X)" : "(□)";
            string zoomKey = string.Format("[{0}]", InputManager.Instance().GetActionKey("Aim", 0));
            string joyZoomKey = "LT";

            cameraControls.text = (InputManager.joyNow ? joyActionKey : actionKey) + " Exit camera     " + (InputManager.joyNow ? joyZoomKey : zoomKey) + " Zoom";
        }

        /// <summary>
        /// Set last hitted enemy.
        /// </summary>
        public void EnemyDamage(AIEntity entity)
        {
            lastEnemy = entity;
            lastEnemyTimeout = lastEnemyCooldown;
        }

        /// <summary>
        /// Switch pause.
        /// </summary>
        public void Pause()
        {
            if (deathScreen.activeSelf)
                return;

            pause = !pause;
            pauseWindow.SetActive(pause);

            Time.timeScale = pause ? 0 : 1;

            if (!pause)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                InputManager.Instance().controls.Player.Enable();
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                InputManager.Instance().controls.Player.Disable();
            }
        }

        /// <summary>
        /// Show text message.
        /// </summary>
        public void ShowMessage(string message)
        {
            messageText.text = message;
            messageTime = messageFadeoutTime;
        }

        /// <summary>
        /// Show save/load indicator.
        /// </summary>
        /// <param name="save">Is it saving now?</param>
        public void SaveLoadIndicator(bool save)
        {
            saveLoadIndicatorTime = 1;
            saveLoadIndicator.text = save ? "Saving..." : "Loading...";
        }

        /// <summary>
        /// Hide UI.
        /// </summary>
        public void Hide()
        {
            canvasGroup.alpha = canvasGroup.alpha == 1 ? 0 : 1;
        }

        /// <summary>
        /// Open cheat menu.
        /// </summary>
        public void OpenDevTools()
        {
            DevTools.Instance().OpenWindow();
        }

        /// <summary>
        /// Load last saved game.
        /// </summary>
        public void Load()
        {
            SaveLoad.Instance().Load();
        }

        /// <summary>
        /// Restart game.
        /// </summary>
        public void Restart()
        {
            SaveLoad.Instance().LoadInit();
        }

        /// <summary>
        /// Quit to the main menu.
        /// </summary>
        public void Quit()
        {
            SceneManager.LoadSceneAsync(menuScene);
        }

    }
}