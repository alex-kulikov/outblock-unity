using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace OutBlock
{

    /// <summary>
    /// Class for the player.
    /// </summary>
    public class Player : MonoBehaviour, ISignalSource, IDamageable, IActor, ISaveable
    {

        /// <summary>
        /// SaveData of the player.
        /// </summary>
        public class PlayerSaveData : DamageableSaveData
        {

            /// <summary>
            /// Weapon index.
            /// </summary>
            public int weaponIndex { get; private set; }

            /// <summary>
            /// Items in the inventory.
            /// </summary>
            public List<Item> items { get; private set; }

            /// <summary>
            /// Weapons ammo.
            /// </summary>
            public int[] ammos { get; private set; }

            /// <summary>
            /// Weapon mags.
            /// </summary>
            public int[] mags { get; private set; }

            public PlayerSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, float hp, bool dead, int weaponIndex, List<Item> items, int[] ammos, int[] mags) : base(id, pos, rot, active, enabled, hp, dead)
            {
                this.weaponIndex = weaponIndex;
                this.items = items;
                this.ammos = ammos;
                this.mags = mags;
            }
        }

        [System.Serializable]
        public class Events
        {

            [SerializeField]
            private UnityEvent onJump = default;
            public UnityEvent OnJump => onJump;
            [SerializeField]
            private UnityEvent onCrouch = default;
            public UnityEvent OnCrouch => onCrouch;
            [SerializeField]
            private UnityEvent onRagdoll = default;
            public UnityEvent OnRagdoll => onRagdoll;
            [SerializeField]
            private UnityEvent onAttack = default;
            public UnityEvent OnAttack => onAttack;
            [SerializeField]
            private UnityEvent onPickUp = default;
            public UnityEvent OnPickUp => onPickUp;
            [SerializeField]
            private UnityEvent onAction = default;
            public UnityEvent OnAction => onAction;
            [SerializeField]
            private UnityEvent onTrigger = default;
            public UnityEvent OnTrigger => onTrigger;
            [SerializeField]
            private UnityEvent onDeath = default;
            public UnityEvent OnDeath => onDeath;

        }

        /// <inheritdoc/>
        public Transform AssignedTransform => transform;
        /// <inheritdoc/>
        public Teams Team => Teams.Player;
        /// <inheritdoc/>
        public bool Sleeping { get; private set; }

        [SerializeField, Header("Components")]
        private GameObject characterComponent = null;
        [SerializeField]
        private CharacterInput input = null;
        [SerializeField]
        private Camera cam = null;
        [SerializeField]
        private CharacterAnimator characterAnimator = null;
        [SerializeField]
        private AudioSource weaponSwitchSource = null;
        [SerializeField]
        private Inventory inventory = null;
        public Inventory Inventory => inventory;
        [SerializeField, Header("Display")]
        private SkinnedMeshRenderer skinnedMesh = null;
        [SerializeField]
        private Material normalMaterial = null;
        [SerializeField]
        private Material hidingMaterial = null;
        [SerializeField]
        private GameObject coverIndicator = null;
        [SerializeField, Header("Animation"), Tooltip("IK look offset when aiming")]
        public Vector3 lookOffset = Vector3.zero;
        [SerializeField, Tooltip("Enable ragdoll when dying?")]
        private bool ragdollOnDeath = false;
        [SerializeField, Header("Stats")]
        private bool canUseRope = true;
        [SerializeField]
        private float maxHp = 100;
        [SerializeField]
        private int hpSegments = 4;
        [SerializeField]
        private float hpRegenCooldown = 1;
        [SerializeField, Tooltip("hp per second")]
        private float hpRegen = 10;
        [SerializeField]
        private AnimationCurve fallDamage = new AnimationCurve();
        [SerializeField]
        private Vector2 sleepTime = new Vector2(1, 2);
        [SerializeField, Header("Takedown")]
        private bool canFight = true;
        [SerializeField]
        private Vector3 takedownCheckOffset = new Vector3(0, 0.9f, 0.5f);
        [SerializeField]
        private Vector3 takedownCheckSize = new Vector3(0.05f, 0.5f, 0.3f);
        [SerializeField]
        private DamageInfo takedownDamage = default;
        [SerializeField]
        private DamageInfo kickDamage = default;
        [SerializeField, Header("Audio")]
        private SoundContainer soundContainer = null;
        [SerializeField, Space]
        private Events events = null;
        public Events PlayerEvents => events;

        public StealthTrigger stealthTrigger { get; private set; }

        /// <summary>
        /// Is player hiding in the grass?
        /// </summary>
        public bool hiding { get; private set; }

        /// <summary>
        /// Is player staying in the grass?
        /// </summary>
        public bool inGrass { get; private set; }

        private int weaponIndex;      
        /// <summary>
        /// Current weapon index.
        /// </summary>
        public int WeaponIndex
        {
            get
            {
                return weaponIndex;
            }
            private set
            {
                weaponIndex = value;

                inventory.UpdateWeaponIndex(weaponIndex);
            }
        }

        public bool RagdollEnabled => Character.RagdollRoot.RagdollEnabled;

        private float hp;
        /// <summary>
        /// Health points.
        /// </summary>
        public float HP
        {
            get
            {
                return hp;
            }
            private set
            {
                if (hp == value)
                    return;

                if (value < hp)
                {
                    hpCooldown = hpRegenCooldown;
                    hpMaxRegen = Mathf.CeilToInt(value / hpSegment) * hpSegment;
                }

                hp = value;
                if (hp > maxHp)
                    hp = maxHp;
                if (hp <= 0)
                    Kill();
            }
        }
        /// <inheritdoc/>
        public float HPRatio => HP / maxHp;
        public event OnDeath onDeath;

        /// <inheritdoc/>
        public bool Dead { get; private set; }

        /// <summary>
        /// Is player is busy?
        /// </summary>
        public bool busy { get; set; }

        /// <inheritdoc/>
        public int Id { get; set; } = -1;
        /// <inheritdoc/>
        public GameObject GO => transform.parent.gameObject;

        private IController character;
        public IController Character
        {
            get
            {
                if (character == null)
                {
                    character = characterComponent.GetComponent<IController>();
                }

                return character;
            }
        }


        private float hpCooldown;
        private float hpSegment;
        private float hpMaxRegen;
        private int prevWeaponIndex;
        private float weaponSwitching;
        private float aimingWeight;
        private Vector3 lookPos;
        private IDamageable damageable;
        private float motionTime;
        private int useIndex;
        private bool canTakedown;
        private bool canKick;
        private IDamageable takedownTarget;
        private Vector3 ropeStart;
        private Vector3 ropeEnd;
        private float ropeClimb;
        private float ropeClimbSpeed;
        private bool ropeClimbing;
        private bool ropeClimbDown;
        private bool ropeTransitioning;
        private Transform ropeTransitionPoint;

        private int bonusCount;
        /// <summary>
        /// Current bonus count.
        /// </summary>
        public int BonusCount
        {
            get
            {
                return bonusCount;
            }
            set
            {
                if (bonusCount == value)
                    return;

                bonusCount = value;
                GameUI.Instance().SetBonusCount(bonusCount);
            }
        }

        /// <summary>
        /// List of interactive triggers player currently is entered.
        /// </summary>
        public static List<Trigger> interaction { get; set; } = new List<Trigger>();

        
        private static Player instance;
        /// <summary>
        /// Instance of the player. Returns null if player is dead, use <see cref="InstanceForced"/> if you dont need that behaviour.
        /// </summary>
        public static Player Instance
        {
            get
            {
                return instance && !instance.Dead ? instance : null;
            }
            private set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Instance of the player.
        /// </summary>
        public static Player InstanceForced
        {
            get => instance;
        }

        /// <summary>
        /// Instance of the corpse of the player.
        /// </summary>
        public static GameObject corpse;

        /// <summary>
        /// SaveData that is saved while dying.
        /// </summary>
        public static PlayerSaveData deathSaveData;

        #region MonoBehaviour
        private void Awake()
        {
            HP = maxHp;
            hpSegment = maxHp / hpSegments;
            hpMaxRegen = 100;

            inventory.Init(Teams.Player);
            WeaponIndex = 0;
            prevWeaponIndex = weaponIndex;

            characterAnimator.Animator.SetInteger("Weapon", (int)(GetCurrentWeapon().WeaponType));

            damageable = GetComponent<IDamageable>();

            if (corpse)
            {
                Destroy(corpse);
            }

            GameUI.Instance().SetDeathScreen(false);
        }

        private void Start()
        {
            GameUI.Instance().HPSegments.uvRect = new Rect(0, 0, hpSegments, 1);

            Instance = this;
        }

        private void OnEnable()
        {
            foreach (WeaponSlot weaponSlot in inventory.WeaponSlots)
            {
                weaponSlot.Weapon.onReload += Reload;
            }

            Character.onLanded += OnLand;
            Character.onJump += OnJump;
            Character.RagdollRoot.onRagdollEnabled += OnRagdollEnabled;
            InputManager.Instance().onAction += Action;
            InputManager.Instance().onReload += ReloadButton;
            InputManager.Instance().onKick += KickButton;
        }

        private void OnDisable()
        {
            foreach (WeaponSlot weaponSlot in inventory.WeaponSlots)
            {
                weaponSlot.Weapon.onReload -= Reload;
            }

            Character.onLanded -= OnLand;
            Character.onJump -= OnJump;
            Character.RagdollRoot.onRagdollEnabled -= OnRagdollEnabled;
            if (InputManager.Instance(true))
            {
                InputManager.Instance().onAction -= Action;
                InputManager.Instance().onReload -= ReloadButton;
                InputManager.Instance().onKick -= KickButton;
            }
        }

        private void OnDestroy()
        {
            Instance = null;

            Unregister();
        }

        private void FixedUpdate()
        {
            canTakedown = false;
            canKick = false;

            if (!canFight)
                return;

            if (!Character.isGrounded || ropeClimbing || Character.climbing || busy || GetCurrentWeapon().Reloading() || Character.RagdollRoot.RagdollEnabled)
                return;

            Vector3 pos = transform.position + transform.forward * takedownCheckOffset.z + transform.up * takedownCheckOffset.y + transform.right * takedownCheckOffset.x;
            Collider[] colls = Physics.OverlapBox(pos, takedownCheckSize, transform.rotation, LayerMask.GetMask("Entity"));
            foreach(Collider coll in colls)
            {
                if (coll == Character.Collider)
                    continue;

                if (Physics.Linecast(transform.position, coll.transform.position, LayerMask.GetMask("Default")))
                    continue;

                Vector3 dir = (coll.transform.position - transform.position).normalized;
                if (coll.CompareTag("Enemy"))
                {
                    takedownTarget = coll.gameObject.GetComponent<IDamageable>();
                    if (takedownTarget != null && !takedownTarget.RagdollEnabled)
                    {
                        canKick = true;
                        canTakedown = Vector3.Dot(dir, coll.transform.forward) > 0.5f;
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            if (Dead)
                return;

            #region Locomotion
            if (!busy && !ropeClimbing)
            {
                Vector3 targetDir = Character.MoveDirection.magnitude >= 0.001f ? (input.aiming ? new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z) : Character.MoveDirection.normalized) : (input.aiming ? new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z) : transform.forward);
                targetDir.y = 0;
                Vector3 dir = Vector3.Slerp(transform.forward, targetDir, Time.deltaTime * 10);
                transform.forward = dir;
            }

            Character.climbInput = input.jump;
            #endregion

            #region Weapons
            if (!Character.RagdollRoot.RagdollEnabled)
            {
                if (weaponSwitching > 0)
                    weaponSwitching -= Time.deltaTime;

                bool aiming = inventory.GetWeapon(WeaponIndex).CanFire() && aimingWeight >= 0.92f && !RagdollEnabled && !ropeClimbing && !busy && weaponSwitching <= 0;
                inventory.GetWeapon(WeaponIndex).Aim(cam.transform, aiming);
                if (aiming && input.fire && inventory.GetWeapon(WeaponIndex).Fire(cam.transform))
                {
                    events.OnAttack?.Invoke();
                    characterAnimator.Animator.SetTrigger("Fire");
                }

                //Weapon switching
                if (!inventory.GetWeapon(WeaponIndex).Reloading() && !input.fire && weaponSwitching <= 0)
                {
                    int wheelMove = (int)Mathf.Clamp(input.weaponSwitch * 10, -1, 1);
                    weaponIndex += wheelMove;

                    if (Keyboard.current.digit1Key.wasPressedThisFrame)
                        weaponIndex = 0;
                    if (Keyboard.current.digit2Key.wasPressedThisFrame)
                        weaponIndex = 1;
                    if (Keyboard.current.digit3Key.wasPressedThisFrame)
                        weaponIndex = 2;
                    if (Keyboard.current.digit4Key.wasPressedThisFrame)
                    {
                        if (weaponIndex < 3)
                            weaponIndex = 3;
                        else if (++weaponIndex >= inventory.WeaponSlots.Length)
                        {
                            weaponIndex = 3;
                        }
                    }

                    do
                    {
                        if (weaponIndex < 0)
                            weaponIndex = inventory.WeaponSlots.Length - 1;
                        if (weaponIndex > inventory.WeaponSlots.Length - 1)
                            weaponIndex = 0;

                        if (!DevTools.Instance().God && inventory.GetWeapon(WeaponIndex).GetAmmo() <= 0 && inventory.GetWeapon(WeaponIndex).reservedAmmo <= 0)
                        {
                            weaponIndex += wheelMove != 0 ? wheelMove : 1;
                        }
                        else break;
                    } while (true);
                }

                if (prevWeaponIndex != weaponIndex)
                {
                    weaponSwitchSource.Play();
                    weaponSwitching = 0.5f;
                    prevWeaponIndex = weaponIndex;
                    WeaponIndex = weaponIndex;
                }
            }
            #endregion

            #region Stats
            if (hpCooldown > 0)
                hpCooldown -= Time.deltaTime;
            else if (HP < hpMaxRegen)
            {
                HP += hpRegen * Time.deltaTime;
            }

            if (inGrass)
            {
                hiding = true;
                StealthTrigger.StealthSettings settings = stealthTrigger == null ? StealthTrigger.StealthSettings.Default : stealthTrigger.Settings;

                if (settings.CrouchOnly && !Character.Crouching)
                    hiding = false;
                else if (!settings.AllowMovement && Character.velocity.magnitude > 1.6f)
                    hiding = false;

                skinnedMesh.material = hiding && settings.CostumeEffect ? hidingMaterial : normalMaterial;
            }
            else
            {
                hiding = false;
                skinnedMesh.material = normalMaterial;
            }
            #endregion

            #region Action
            useIndex = -1;
            if (!ropeClimbing && !busy && !Character.RagdollRoot.RagdollEnabled)
            {
                for (int i = 0; i < interaction.Count; i++)
                {
                    if (interaction[i].CanActivate())
                    {
                        useIndex = i;
                        break;
                    }
                }
            }

            input.busy = busy;
            #endregion

            #region RopeClimb
            if (ropeClimbing)
            {
                Character.SetPosition(Vector3.Lerp(ropeStart, ropeEnd, ropeClimb));
                ropeClimb += Time.deltaTime / ropeClimbSpeed;

                if (ropeClimb >= 1)
                {
                    EndClimb(true);
                }

                if (!ropeClimbDown && !soundContainer.IsPlaying())
                {
                    soundContainer.PlaySound("effort");
                }
            }

            if (ropeTransitioning)
            {
                Character.SetPosition(Vector3.Lerp(transform.position, ropeTransitionPoint.position, Time.deltaTime * 5));
                if (Vector3.Distance(transform.position, ropeTransitionPoint.position) <= 0.15f)
                {
                    ropeTransitioning = false;
                    ropeTransitionPoint = null;
                    Character.Kinematic = false;
                }
            }
            #endregion

            #region Animation
            if (characterAnimator)
            {
                aimingWeight = weaponIndex > 0 && !ropeClimbing && !busy ? Mathf.Lerp(aimingWeight, input.aiming || inventory.GetWeapon(WeaponIndex).Reloading() ? 1 : 0, Time.deltaTime * (input.aiming ? 16 : 8)) : 0;

                motionTime += (Time.deltaTime * inventory.GetWeapon(WeaponIndex).Speed) / characterAnimator.Animator.GetCurrentAnimatorStateInfo(0).length;

                characterAnimator.Animator.SetLayerWeight(1, weaponIndex > 0 ? aimingWeight : 0);
                characterAnimator.Animator.SetInteger("Weapon", (int)(GetCurrentWeapon().WeaponType));
                characterAnimator.Animator.SetFloat("LocomotionTime", motionTime);
                characterAnimator.Animator.SetBool(ropeClimbDown ? "RopeClimbDown" : "RopeClimb", ropeClimbing);

                //LookAt
                if (!Sleeping && !GetCurrentWeapon().Reloading())
                {
                    if (aimingWeight > 0.7f)
                    {
                        lookPos = Vector3.Lerp(lookPos, transform.position + Vector3.up * lookOffset.y + transform.right * lookOffset.x + cam.transform.forward * lookOffset.z, Time.deltaTime * 10);
                        characterAnimator.SetLookAt(lookPos, aimingWeight);
                    }
                    else
                    {
                        LookPoint lookPoint = LookPoint.GetLookPoint(transform.position);
                        CharacterAnimator.LookAtWeight weight = new CharacterAnimator.LookAtWeight(1, 0, 0.6f, 1, 0.75f);
                        Vector3 newPos = Vector3.zero;

                        if (lookPoint == null)
                        {
                            newPos = cam.transform.position + cam.transform.forward * 10;
                        }
                        else
                        {
                            newPos = lookPoint.position;
                        }

                        if (Vector3.Angle(transform.forward, (newPos - transform.position).normalized) > 90)
                            newPos = GetHeadPos() + transform.forward * 10;

                        lookPos = Vector3.Lerp(lookPos, newPos, Time.deltaTime * 2.5f);

                        characterAnimator.SetLookAt(lookPos, weight);
                    }
                }
                else
                {
                    characterAnimator.SetLookAt(lookPos, 0);
                }
            }
            #endregion

            #region UI
            GameUI.Instance().HPBar.fillAmount = HPRatio;
            GameUI.Instance().SetWeapon(inventory.GetWeapon(WeaponIndex));
            GameUI.Instance().SetCrosshair(GetCurrentWeapon(), input.aiming);
            GameUI.Instance().SetAction(useIndex >= 0, canTakedown, canKick);

            AIManager.States currentState = AIManager.Instance().GetAIStatus(out float cooldown);
            coverIndicator.SetActive(currentState > 0 && hiding);
            if (coverIndicator.activeSelf)
                coverIndicator.transform.localScale = Vector3.one * 2 * ((currentState == AIManager.States.Attention ? AISensors.hidingDistAttention : AISensors.hidingDistAggression) - 1.8f);
            #endregion
        }

        private void OnTriggerEnter(Collider coll)
        {
            inGrass = coll.CompareTag("Grass");
            if (inGrass)
            {
                stealthTrigger = coll.GetComponent<StealthTrigger>();
            }
        }

        private void OnTriggerExit(Collider coll)
        {
            if (coll.CompareTag("Grass"))
            {
                inGrass = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(takedownCheckOffset, takedownCheckSize);
        }
        #endregion

        #region Private coroutines
        private IEnumerator KungFu(string animation, string sound, float time, float delay, DamageInfo damage)
        {
            bool ragdollActive = Character.RagdollRoot.Active;
            Character.RagdollRoot.Active = false;
            characterAnimator.Animator.SetTrigger(animation);
            soundContainer.PlaySound(sound);
            SetBusy(time);
            Vector3 pos = takedownTarget.GetTargetPos();
            pos.y = transform.position.y;
            pos -= (pos - transform.position).normalized;
            Character.SetPosition(pos);
            yield return new WaitForSeconds(0.6f);
            takedownTarget.Damage(damage);
            yield return new WaitForSeconds(0.2f);
            Character.RagdollRoot.Active = ragdollActive;
        }
        #endregion

        #region Private methods
        private void Action()
        {
            if (useIndex >= 0)
            {
                events.OnAction?.Invoke();
                interaction[useIndex].Activate(Character.Collider);
                SetBusy(0.8f);
            }
            else if (canTakedown)
            {
                DamageInfo damage = new DamageInfo(takedownDamage.damage, transform.position, transform.position + Vector3.up * 1.6f, Teams.Player, transform.TransformDirection(takedownDamage.force), takedownDamage.damageType);
                StartCoroutine(KungFu("Takedown", "takedown", 1.3f, 0.6f, damage));
            }
        }

        private void KickButton()
        {
            Kick();
        }

        private bool Kick()
        {
            if (canKick)
            {
                DamageInfo damage = new DamageInfo(kickDamage.damage, transform.position, transform.position + Vector3.up * 1.6f, Teams.Player, transform.TransformDirection(kickDamage.force), kickDamage.damageType);
                StartCoroutine(KungFu("Kick", "takedown", 1.3f, 0.6f, damage));
                return true;
            }

            return false;
        }

        private void ReloadButton()
        {
            if (!Kick())
            {
                GetCurrentWeapon().Reload();
            }
        }

        private void OnLand(float height)
        {
            if (DevTools.Instance().NoFallDamage)
                return;

            Damage(new DamageInfo(fallDamage.Evaluate(height) * maxHp, transform.position, transform.position));
        }

        private void OnJump()
        {
            characterAnimator.Animator.SetTrigger("Jump");
        }

        private void OnRagdollEnabled(bool enabled)
        {
            if (enabled)
                input.crouchSwitch = true;
            events.OnRagdoll?.Invoke();
            inGrass = false;
            interaction.Clear();
        }

        private void SetBusy(float freeTime)
        {
            busy = true;
            Invoke("Free", freeTime);
        }

        private void Free()
        {
            busy = false;
        }

        private void Reload()
        {
            GetCurrentWeapon().Reload();
            characterAnimator.Animator.SetTrigger("Reload");
        }

        private void Sleep()
        {
            characterAnimator.Animator.SetTrigger("SleepStart");
            characterAnimator.Animator.SetBool("Sleep", true);
            Sleeping = true;
            float time = Random.Range(sleepTime.x, sleepTime.y);
            busy = true;
            Invoke("WakeUp", time);
        }

        private void WakeUp()
        {
            Sleeping = false;
            busy = false;
            characterAnimator.Animator.SetBool("Sleep", false);
        }

        private void EndClimb(bool end)
        {
            ropeClimbing = false;

            if (end && ropeTransitionPoint)
            {
                ropeTransitioning = true;
            }
            else
            {
                Character.Kinematic = false;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Initiate jump.
        /// </summary>
        public void Jump()
        {
            if (ropeClimbing)
            {
                EndClimb(false);
            }
            else
            {
                Character.Jump();
            }

            events.OnJump?.Invoke();
        }

        /// <summary>
        /// Get up after being a ragdoll.
        /// </summary>
        public void GetUp(bool spine)
        {
            SetBusy(1.5f);
            characterAnimator.Animator.SetTrigger(spine ? "GettingUpSpine" : "GettingUpBelly");
        }

        /// <summary>
        /// Climb the rope.
        /// </summary>
        public void ClimbRope(Rope.ClimbData climbData)
        {
            if (!canUseRope)
                return;

            if (RagdollEnabled)
                return;

            if (GetCurrentWeapon().Reloading())
                return;

            Character.Crouch(false);
            Character.Kinematic = true;
            ropeClimbing = true;
            ropeClimb = climbData.t;
            ropeStart = climbData.start;
            ropeEnd = climbData.end;
            ropeTransitionPoint = climbData.transitionPoint;

            ropeClimbDown = climbData.down;
            ropeClimbSpeed = climbData.dist / climbData.speed;
        }

        /// </inheritdoc>/>
        public void Warp(Vector3 pos)
        {
            Character.SetPosition(pos);
        }

        /// <summary>
        /// Getting the damage.
        /// </summary>
        public void Damage(DamageInfo damageInfo)
        {
            if (DevTools.Instance().God)
                return;

            if (Dead)
                return;

            if (damageInfo.damageType == DamageInfo.DamageTypes.Force && !DevTools.Instance().DisableRagdoll)
            {
                if (!damageInfo.forceFromCenter)
                    Character.EnableRagdoll(damageInfo.force);
                else Character.EnableRagdoll((transform.position - damageInfo.source).normalized * damageInfo.force.magnitude);
            }
            else if (damageInfo.damageType == DamageInfo.DamageTypes.Sleep)
            {
                if (!Character.isGrounded && !DevTools.Instance().DisableRagdoll)
                    Character.EnableRagdoll();
                Sleep();
            }

            if (damageInfo.damage < 1)
                return;

            if (DevTools.Instance().NoDamage)
                return;

            HP -= damageInfo.damage;
            characterAnimator.Animator.SetTrigger("Hit");

            soundContainer.PlaySound("pain");
        }

        /// <summary>
        /// Heals one segment of the health points.
        /// </summary>
        public bool Heal()
        {
            if (HP >= maxHp)
                return false;

            hpMaxRegen += hpSegment;
            if (hpMaxRegen > maxHp)
                hpMaxRegen = maxHp;

            HP = hpMaxRegen;
            return true;
        }

        /// <inheritdoc/>
        public void Kill()
        {
            if (Dead)
                return;

            Dead = true;

            deathSaveData = new PlayerSaveData(0, transform.position, transform.eulerAngles, gameObject.activeSelf, enabled, maxHp / 2, Dead, weaponIndex, inventory.GetItems(), inventory.GetAmmo(), inventory.GetReservedAmmo());
            GameUI.Instance().HPBar.fillAmount = 0;

            Character.Active = false;
            //corpse = transform.parent.gameObject;
            if (!ragdollOnDeath && !RagdollEnabled)
            {
                characterAnimator.Animator.SetTrigger("Death");
                characterAnimator.Animator.SetLayerWeight(1, 0);
                characterAnimator.SetLookAt(Vector3.zero, 0);
                characterAnimator.Animator.SetBool("IsGrounded", true);
            }
            else
            {
                Character.EnableRagdoll(true);
            }

            inventory.GetWeapon(WeaponIndex).Aim(cam.transform, false);

            GameUI.Instance().SoundContainer.PlaySound("death");
            GameUI.Instance().SetDeathScreen(true);
            GameUI.Instance().HideCrosshair();

            onDeath?.Invoke(this);
            events.OnDeath?.Invoke();
            interaction.Clear();
            enabled = false;
        }

        ///<inheritdoc/>
        public void Revive()
        {
            if (!Dead)
                return;

            Dead = false;

            Character.Active = true;

            if (RagdollEnabled)
            {
                Character.RagdollRoot.DisableRagdoll(true);
            }

            characterAnimator.Animator.SetTrigger("Revive");

            busy = false;
            CancelInvoke("Free");

            GameUI.Instance().SetDeathScreen(false);

            enabled = true;
        }

        /// <summary>
        /// Add ammo to the weapons in the inventory.
        /// </summary>
        public void AddAmmo(WeaponAmmo[] ammo)
        {
            inventory.AddAmmo(ammo);
        }

        /// <summary>
        /// Get currently active weapon.
        /// </summary>
        public Weapon GetCurrentWeapon()
        {
            return inventory.GetWeapon(WeaponIndex);
        }

        /// <inheritdoc/>
        public IDamageable Damageable()
        {
            return damageable;
        }

        /// <inheritdoc/>
        public Vector3 GetTargetPos()
        {
            return transform.position + Vector3.up * Character.CapsuleCollider.height * 0.5f;
        }

        /// <summary>
        /// Get position of the feet.
        /// </summary>
        public Vector3 GetFeetPos()
        {
            return transform.position + Vector3.up * 0.2f;
        }

        /// <summary>
        /// Get position of the head.
        /// </summary>
        public Vector3 GetHeadPos()
        {
            return transform.position + Vector3.up * Character.CapsuleCollider.height * 0.9f;
        }
        #endregion

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

        /// <inheritdoc/>
        public SaveData Save()
        {
            float hp = this.hp;
            if (!Dead && hp == 0)
                hp = maxHp;
            return new PlayerSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, hp, Dead, weaponIndex, inventory.GetItems(), inventory.GetAmmo(), inventory.GetReservedAmmo());
        }

        /// <inheritdoc/>
        public void Load(SaveData data)
        {
            transform.localEulerAngles = data.rot;
            Character.SetPosition(data.pos);

            PlayerSaveData playerSaveData = (PlayerSaveData)data;
            if (playerSaveData != null)
            {
                HP = playerSaveData.hp;
                if (Dead != playerSaveData.dead)
                {
                    if (playerSaveData.dead)
                        Kill();
                    else Revive();

                    Dead = playerSaveData.dead;
                }

                WeaponIndex = playerSaveData.weaponIndex;
                inventory.SetAmmo(playerSaveData.ammos, playerSaveData.mags);
                inventory.SetItems(playerSaveData.items);
            }

            input.crouchSwitch = false;
        }
        #endregion

        #region Static methods
        /*/// <summary>
        /// Revive the player.
        /// </summary>
        public static void Revive()
        {
            if (player != null || deathSaveData == null)
                return;

            Transform clone = Instantiate(Resources.Load<Transform>("Gloria"), Vector3.zero, Quaternion.identity);
            Player plr = clone.GetComponentInChildren<Player>();
            plr.Load(deathSaveData);
        }*/
        #endregion
    }
}