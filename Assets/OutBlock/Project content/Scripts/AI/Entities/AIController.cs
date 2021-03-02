using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OutBlock
{
    
    /// <summary>
    /// Entity for the controller i.e. AI character
    /// </summary>
    public class AIController : AIEntity, ICharacter, IActor
    {

        [SerializeField, Header("Components")]
        private NavMeshAgent navMeshAgent = null;
        public NavMeshAgent NavMeshAgent => navMeshAgent;
        [SerializeField]
        private AnimationAnchor animationAnchor = null;
        public AnimationAnchor AnimationAnchor => animationAnchor;
        [SerializeField]
        new private Collider collider = null;
        [SerializeField]
        private RagdollRoot ragdoll = null;
        [SerializeField, Tooltip("This attention point is will be enabled if the controller die or become ragdoll.")]
        private GameObject attentionPoint = null;
        [SerializeField, Header("Weapons")]
        private Weapon weapon = null;
        public Weapon Weapon => weapon;
        [SerializeField]
        private Weapon_Grenade grenade = null;
        public Weapon_Grenade Grenade => grenade;
        [SerializeField, Header("Close punch")]
        private bool punchWhenClose = false;
        public bool PunchWhenClose => punchWhenClose;
        [SerializeField]
        private float punchDistance = 1;
        public float PunchDistance => punchDistance;
        [SerializeField]
        private DamageInfo punchDamage = default;
        [SerializeField, Tooltip("Delay between punches")]
        private float punchDelay = 2;
        [SerializeField, Header("Stats")]
        private bool ignoreAttackOrder = false;
        [SerializeField]
        private float maxHP = 50;
        [SerializeField, Space]
        private float walkSpeed = 3.5f;
        [SerializeField]
        private float runSpeed = 6.5f;
        [SerializeField, Tooltip("The time before AIManager will update this controller running allowance")]
        private float runningTimeUpdate = 15;
        [SerializeField, Space]
        private float maxCoverDistance = 15;
        [SerializeField, Space, Tooltip("Max distance between the controllers")]
        private float minAgentDistance = 1.5f;
        [SerializeField, Space]
        private bool ragdollOnDeath = false;
        [SerializeField, Tooltip("Minimum impulse acquired to enable ragdoll")]
        private float minImpulse = 20;
        public float MinImpulse => minImpulse;
        [SerializeField, Range(0, 100), Space]
        private int coverOnAggressionChance = 50;
        public int CoverOnAggressionChance => coverOnAggressionChance;
        [SerializeField, Range(0, 100)]
        private int coverOnDamageChance = 70;
        [SerializeField, Space]
        private LayerMask groundMask = default;
        [SerializeField, Header("Sounds")]
        private SoundContainer soundContainer = null;

        ///<inheritdoc/>
        public override bool CanAttack => true;
        ///<inheritdoc/>
        public override bool IgnoreAttackOrder => ignoreAttackOrder;

        ///<inheritdoc/>
        public override int Id { get; set; } = -1;
        public LayerMask GroundMask => groundMask;
        public Collider Collider => collider;
        ///<inheritdoc/>
        public override GameObject GO => gameObject;

        //States
        public AIStateAttention attentionState { get; private set; }
        public AIStateIdle idleState { get; private set; }
        public AIStateAggresion aggresionState { get; private set; }
        public AIStateSleep sleepState { get; private set; }
        public AIStateCover coverState { get; private set; }
        public AIStateClimb climbState { get; private set; }

        //target locked direction
        private Vector3 targetDirection;

        //Auto rotation to movement direction
        private bool lookMovementAtDirection;
        private float moveDelay;
        private Vector3 prevMovePoint;

        private bool running = false;

        private float gettingUp;
        private float motionTime;
        private float punchTime;
        private float hp;
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

                hp = value;
                if (hp > maxHP)
                    hp = maxHP;
                if (hp <= 0)
                    Kill();
            }
        }

        ///<inheritdoc/>
        public override bool Dead { get; protected set; }

        ///<inheritdoc/>
        public override bool RagdollEnabled => ragdoll.RagdollEnabled;
        ///<inheritdoc/>
        public override Transform AssignedTransform => transform;
        ///<inheritdoc/>
        public override bool Sleeping => data.currentState == sleepState;
        ///<inheritdoc/>
        public override float HPRatio => HP / maxHP;
        ///<inheritdoc/>
        public override event OnDeath onDeath;

        private void Awake()
        {
            weapon.team = Teams.AI;

            //Remove auto rotation from navmesh agent, we upate it manualy
            navMeshAgent.updateRotation = false;
            hp = maxHP;

            //Create instances of states
            idleState = new AIStateIdle();
            attentionState = new AIStateAttention();
            aggresionState = new AIStateAggresion();
            sleepState = new AIStateSleep();
            coverState = new AIStateCover();
            climbState = new AIStateClimb();

            //Switch to default state
            SwitchState(idleState);
        }

        private void OnEnable()
        {
            //Setup components
            if (animationAnchor)
                animationAnchor.Animator.SetInteger("Weapon", (int)weapon.WeaponType);

            AIManager.Instance().RegisterAI(this);

            weapon.onReload += Reload;
            ragdoll.onRagdollEnabled += OnRagdollEnabled;
        }

        private void OnDisable()
        {
            AIManager.Instance(true)?.DisposeAI(this);

            weapon.onReload -= Reload;
            ragdoll.onRagdollEnabled -= OnRagdollEnabled;
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Start()
        {
            //Set default values
            data.spawnPoint = transform.position;

            InvokeRepeating("UpdateRunningState", 0, runningTimeUpdate);
        }

        private void Update()
        {
            if (Dead)
                return;

            if (navMeshAgent.isOnOffMeshLink && navMeshAgent.currentOffMeshLinkData.offMeshLink && !(GetState() is AIStateClimb))
            {
                Rope rope = navMeshAgent.currentOffMeshLinkData.offMeshLink.GetComponent<Rope>();
                if (rope)
                {
                    ClimbRope(rope.CalculateClimbing(transform));
                }
            }

            if (gettingUp > 0)
                gettingUp -= Time.deltaTime;

            attentionPoint?.SetActive(Sleeping || RagdollEnabled);

            if (ragdoll && (RagdollEnabled || gettingUp > 0))
            {
                navMeshAgent.Warp(RagdollEnabled ? ragdoll.groundPos : transform.position);
                return;
            }

            UpdateRotation();
            if (data.currentState != null)
            {
                data.currentState.Update();
            }

            if (!(GetState() is AIStateClimb))
                navMeshAgent.speed = ((GetState() is AIStateAggresion || GetState() is AIStateCover) && (AIManager.Instance().GetIndex(this) <= 1 || running)) ? runSpeed : walkSpeed;

            if (punchTime > 0)
                punchTime -= Time.deltaTime;

            if (animationAnchor)
            {
                bool firing = punchTime <= 0 && data.currentState == aggresionState || data.currentState == coverState;
                animationAnchor.Animator.SetFloat("Forward", navMeshAgent.velocity.magnitude / runSpeed);
                animationAnchor.Animator.SetBool("IsGrounded", true);
                motionTime += Time.deltaTime / animationAnchor.Animator.GetCurrentAnimatorStateInfo(0).length;
                animationAnchor.Animator.SetFloat("LocomotionTime", motionTime);

                animationAnchor.Animator.SetBool("Crouch", weapon.Reloading());

                if (Player.Instance && sensors.visionResult.playerVisible)
                {
                    Vector3 pos = Player.Instance.GetTargetPos();
                    pos -= Vector3.up * 3.8f;
                    pos += transform.right * 10;
                    pos += transform.forward * 5f;
                    animationAnchor.SetLookAt(pos, firing ? 1 : 0);
                    animationAnchor.Animator.SetLayerWeight(1, firing ? 1 : 0);
                }
                else
                {
                    animationAnchor.SetLookAt(Vector3.up, 0);
                    animationAnchor.Animator.SetLayerWeight(1, 0);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag("Player"))
            {
                Aggression();
                assignedZone.PropogatePlayerPosition(collision.transform.position);
            }

            if (ragdoll)
            {
                Rigidbody collRigidbody = collision.collider.attachedRigidbody;
                if (collRigidbody && collRigidbody.velocity.magnitude >= 1)
                {
                    float impulse = collRigidbody.mass * collRigidbody.velocity.magnitude;
                    if (impulse >= minImpulse)
                        ragdoll.EnableRagdoll(collRigidbody.velocity, false);
                }
            }
        }

        private void OnRagdollEnabled(bool enabled)
        {
            if (enabled && !Dead)
                events.OnRagdoll?.Invoke();
        }

        private void UpdateRunningState()
        {
            running = Utils.CalculateChance(30);
        }

        private void Reload()
        {
            animationAnchor.Animator?.SetTrigger("Reload");
        }

        //Smoothly rotate to target Direction
        private void UpdateRotation()
        {
            if (lookMovementAtDirection) targetDirection = navMeshAgent.velocity.normalized;
            Vector3 dir = targetDirection;
            dir.y = 0f;
            dir.Normalize();
            if (dir == Vector3.zero) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), navMeshAgent.angularSpeed * Time.deltaTime);
        }

        private IEnumerator PunchDamage(IDamageable damageable, float delay)
        {
            yield return new WaitForSeconds(delay);
            DamageInfo damage = new DamageInfo(punchDamage);
            damage.force = transform.TransformDirection(punchDamage.force);
            damageable.Damage(damage);
        }

        ///<inheritdoc/>
        public override void Kill()
        {
            if (Dead)
                return;

            Dead = true;

            if (GetState() is AIStateClimb)
                ragdoll.EnableRagdoll(Vector3.zero, false);

            Relax();
            data.currentState?.End();
            data.currentState = null;

            Stop();

            attentionPoint?.SetActive(true);

            if (!ragdollOnDeath && !RagdollEnabled)
            {
                animationAnchor.Animator.SetTrigger("Death");
                animationAnchor.Animator.SetLayerWeight(1, 0);
                animationAnchor.SetLookAt(Vector3.zero, 0);
            }
            else
            {
                ragdoll.EnableRagdoll(Vector3.zero, true);
            }
            events.OnDeath?.Invoke();
            onDeath?.Invoke(this);

            soundContainer.PlaySound("death");

            enabled = false;
            collider.enabled = false;
            navMeshAgent.enabled = false;
        }

        ///<inheritdoc/>
        public override void Revive()
        {
            if (!Dead)
                return;

            Dead = false;

            attentionPoint?.SetActive(false);

            if (RagdollEnabled)
                ragdoll.DisableRagdoll(true);

            enabled = true;
            navMeshAgent.enabled = true;
            navMeshAgent.ResetPath();
            collider.enabled = true;
            animationAnchor.Animator.SetTrigger("Revive");

            Relax();
        }

        /// <summary>
        /// Can this entity use its weapon?
        /// </summary>
        public bool CanFire()
        {
            return AIManager.Instance().CanFire(this);
        }

        /// <summary>
        /// Can this entity throw its grenade?
        /// </summary>
        public bool CanThrow()
        {
            return AIManager.Instance().CanThrow();
        }

        /// <summary>
        /// Add this entity to the throwind queue
        /// </summary>
        public void Throw()
        {
            AIManager.Instance().Throw();
        }

        /// <summary>
        /// Get to the nearest cover 
        /// </summary>
        public void GetCover()
        {
            if (!Player.Instance)
                return;

            if (Vector3.Distance(transform.position, Player.Instance.transform.position) <= 5)
                return;

            if (data.currentState == coverState)
                return;

            //find nearest cover(even tho we touched a cover it can be already taken)
            if (Cover.GetCover(transform.position, data.lastSeenPlayerPosition, maxCoverDistance, out Cover cover))
            {
                coverState.currentCover?.Free();
                coverState.currentCover = cover;
                events.OnCover?.Invoke();
                SwitchState(coverState);
            }
        }

        /// <summary>
        /// Switch state
        /// </summary>
        public void SwitchState(AIState state)
        {
            if (ragdoll && RagdollEnabled)
                return;

            if (data.currentState == state)
                return;

            if (data.currentState != null)
            {
                data.currentState.End();
            }

            switch (state)
            {
                case AIStateAggresion agr:
                    events.OnAgression?.Invoke();
                    break;

                case AIStateAttention att:
                    events.OnAttention?.Invoke();
                    break;

                case AIStateClimb clmb:
                    events.OnRopeClimb?.Invoke();
                    break;

                case AIStateSleep slp:
                    events.OnSleep?.Invoke();
                    break;
            }

            data.currentState = state;
            state.Init(this);
        }

        ///<inheritdoc/>
        public override AIState GetState()
        {
            return data.currentState;
        }

        /// <summary>
        /// Switch state to the Attention
        /// </summary>
        public override void Attention()
        {
            if (!(data.currentState is AIStateIdle))
                return;

            SwitchState(attentionState);
        }

        /// <summary>
        /// Switch state to the Idle
        /// </summary>
        public void Relax()
        {
            SwitchState(idleState);
        }

        /// <summary>
        /// Propagate player position and set all entities in the current AIZone to the aggression state
        /// </summary>
        public void SpotPlayer()
        {
            assignedZone.PropogatePlayerPosition(data.lastSeenPlayerPosition);
            assignedZone.Aggresion(transform.position);
        }

        /// <summary>
        /// Switch to the aggression state
        /// </summary>
        public override void Aggression()
        {
            if (data.currentState == sleepState)
                return;

            SwitchState(aggresionState);
        }

        /// <summary>
        /// Switch to the sleep state
        /// </summary>
        /// <param name="multiplier">Sleep time multiplier</param>
        public void Sleep(float multiplier = 1)
        {
            if (GetState() is AIStateClimb)
                ragdoll.EnableRagdoll(Vector3.zero, false);

            sleepState.Multiplier = multiplier;
            SwitchState(sleepState);
        }

        /// <summary>
        /// Start climbing the rope
        /// </summary>
        public void ClimbRope(Rope.ClimbData climbData)
        {
            climbState.prevState = GetState();
            climbState.climbData = climbData;
            SwitchState(climbState);
        }

        /// <summary>
        /// Move to the point and pay attention to the distance between controllers
        /// </summary>
        public void MoveTo(Vector3 point, bool lookAtDirection = true)
        {
            if (Time.unscaledTime > moveDelay)
            {
                moveDelay = Time.unscaledTime + Random.Range(0.2f, 0.3f);

                int maxTries = 10;
                float offsetDist = 0.2f;
                while(!assignedZone.ValidateMove(this, point, minAgentDistance))
                {
                    if (maxTries % 2 == 0)
                    {
                        point += transform.right * offsetDist;
                        offsetDist += Random.Range(0.2f, 0.3f);
                    }
                    point -= transform.right * offsetDist;

                    if (--maxTries <= 0)
                        break;
                }

                if (Vector3.Distance(point, prevMovePoint) > 0.18f)
                {
                    prevMovePoint = point;
                    assignedZone.AddMove(this, point);
                    navMeshAgent.destination = point;
                }
            }
            lookMovementAtDirection = lookAtDirection;
        }

        /// <summary>
        /// Move to the point and ignore the distance between controllers
        /// </summary>
        public void ForceMoveTo(Vector3 point, bool lookAtDirection = true)
        {
            assignedZone.AddMove(this, point);
            navMeshAgent.destination = point;
            lookMovementAtDirection = lookAtDirection;
        }

        /// </inheritdoc>/>
        public void Warp(Vector3 pos)
        {
            if (RagdollEnabled)
            {
                ragdoll.DisableRagdoll(true);
            }
            navMeshAgent.isStopped = true;
            navMeshAgent.Warp(pos);
            navMeshAgent.isStopped = false;
            assignedZone = AIZone.GetZoneByPos(pos);
        }

        /// <summary>
        /// Stop any movement
        /// </summary>
        public void Stop()
        {
            navMeshAgent.ResetPath();
        }

        /// <summary>
        /// Look to the direction
        /// </summary>
        public void RotateTo(Vector3 direction)
        {
            if (direction == Vector3.zero)
                return;
            targetDirection = direction;
            lookMovementAtDirection = false;
        }

        /// <inheritdoc/>
        public override void Damage(DamageInfo damageInfo)
        {
            if (damageInfo.team == Teams.AI && !friendlyFire)
                return;

            bool headshot = Vector3.Distance(animationAnchor.Animator.GetBoneTransform(HumanBodyBones.Head).position, damageInfo.hitPoint) <= 0.3f;

            if (damageInfo.damageType == DamageInfo.DamageTypes.Sleep)
            {
                if (data.currentState != sleepState)
                    Sleep(headshot ? 2 : 1);
            }
            else if (damageInfo.damageType == DamageInfo.DamageTypes.Force)
            {
                if (!damageInfo.forceFromCenter)
                    ragdoll.EnableRagdoll(damageInfo.force, false);
                else ragdoll.EnableRagdoll((transform.position - damageInfo.source).normalized * damageInfo.force.magnitude, false);

                HP -= damageInfo.damage * (headshot ? 2 : 1);

                if (!Dead && data.currentState != sleepState)
                {
                    assignedZone.Aggresion(transform.position);
                    assignedZone.PropogatePlayerPosition(damageInfo.source);
                }
            }
            else
            {
                HP -= damageInfo.damage * (headshot ? 2 : 1);

                if (!Dead)
                {
                    if (data.currentState != sleepState)
                    {
                        Attention();
                        Aggression();
                        data.lastSeenPlayerPosition = transform.position + (damageInfo.source - transform.position).normalized * Random.Range(3, 6);
                    }
                    animationAnchor.Animator.SetTrigger("Hit");

                    if (Utils.CalculateChance(coverOnDamageChance) && GetState() is AIStateAggresion && !navMeshAgent.isOnOffMeshLink)
                        GetCover();
                }
            }

            if (Random.Range(0, 3) == 0)
                soundContainer.PlaySound("pain");

            if (damageInfo.team == Teams.Player)
                GameUI.Instance().EnemyDamage(this);
        }

        /// <inheritdoc/>
        public override Vector3 GetTargetPos()
        {
            return transform.position + Vector3.up * 1.2f;
        }

        /// <inheritdoc/>
        public override IDamageable Damageable()
        {
            return GetComponent<IDamageable>();
        }

        /// <summary>
        /// Get up after being a ragdoll
        /// </summary>
        /// <param name="spine">Lying on spine?</param>
        public void GetUp(bool spine)
        {
            animationAnchor.Animator.SetTrigger(spine ? "GettingUpSpine" : "GettingUpBelly");
            gettingUp = 1.5f;
        }

        /// <summary>
        /// Can perform a punch?
        /// </summary>
        public bool CanPunch()
        {
            return punchTime <= 0;
        }

        /// <summary>
        /// Start punch
        /// </summary>
        /// <param name="damageable">Target</param>
        public void Punch(IDamageable damageable)
        {
            animationAnchor.Animator.SetTrigger("RiflePunch");
            soundContainer.PlaySound("riflePunch");
            punchTime = punchDelay;
            StartCoroutine(PunchDamage(damageable, 0.5f));
        }

        /// <summary>
        /// Set on path
        /// </summary>
        public void SetPath(AIPath path)
        {
            data.Path = path;
            idleState.ResetPath();
        }

        #region Saveable
        /// <inheritdoc/>
        public override void Register()
        {
            SaveLoad.Add(this);
        }

        /// <inheritdoc/>
        public override void Unregister()
        {
            SaveLoad.Remove(this);
        }

        /// <inheritdoc/>
        public override SaveData Save()
        {
            float hp = this.hp;
            if (!Dead && hp == 0)
                hp = maxHP;
            return new EntitySaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, hp, Dead, data.currentState ?? new AIStateIdle());
        }

        /// <inheritdoc/>
        public override void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);

            navMeshAgent.Warp(data.pos);
            sensors.ResetVision();

            EntitySaveData entitySaveData = (EntitySaveData)data;
            if (entitySaveData != null)
            {
                hp = entitySaveData.hp;
                if (Dead != entitySaveData.dead)
                {
                    if (entitySaveData.dead)
                        Kill();
                    else Revive();

                    Dead = entitySaveData.dead;
                }

                switch (entitySaveData.state)
                {
                    case AIStateAggresion agr:
                        SwitchState(aggresionState);
                        break;

                    case AIStateAttention att:
                        SwitchState(attentionState);
                        break;

                    case AIStateCover cvr:
                        SwitchState(coverState);
                        break;

                    case AIStateIdle idl:
                        SwitchState(idleState);
                        break;

                    case AIStateSleep slp:
                        SwitchState(sleepState);
                        break;
                }
            }
        }
        #endregion
    }
}