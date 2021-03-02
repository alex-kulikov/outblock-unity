using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Physically-based character controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class Character : MonoBehaviour, IController
    {

        [SerializeField, Tooltip("Draw debug gizmos")]
        private bool debugMode = false;
        [SerializeField, Header("Components")]
        private Rigidbody rigid = null;
        [SerializeField]
        private CapsuleCollider coll = null;
        public Collider Collider => coll;
        public CapsuleCollider CapsuleCollider => coll;
        [SerializeField]
        private RagdollRoot ragdollRoot = null;
        public RagdollRoot RagdollRoot => ragdollRoot;
        [SerializeField, Header("Movement"), Range(0, 90), Tooltip("Max slope angle that character can walk")]
        private float maxSlope = 40;
        [SerializeField]
        private float walkSpeed = 3f;
        [SerializeField]
        private float runSpeed = 6f;
        [SerializeField]
        private float crouchSpeed = 1.5f;
        [SerializeField, Tooltip("Movement smoothing. The larger the value the less the movement is smoothed")]
        private float movementFriction = 3;
        [SerializeField, Header("Climbing")]
        private float maxClimbHeight = 1.2f;
        [SerializeField]
        private float maxSteppingHeight = 0.7f;
        [SerializeField]
        private float climbCheckArea = 0.4f;
        [SerializeField, Tooltip("Climbing speed")]
        private float climbSpeed = 1.5f;
        [SerializeField, Header("Crouching"), Tooltip("Capsule collider height multiplier when crouching")]
        private float crouchHeightMultiplier = 0.5f;
        [SerializeField, Header("Jumping")]
        private float jumpForce = 5;
        [SerializeField, Tooltip("How long jump force will be applied to the character after a jump")]
        private float jumpTime = 0.05f;
        [SerializeField, Tooltip("Cooldown before the character can jump after a landing")]
        private float jumpCooldown = 0.2f;
        [SerializeField, Tooltip("Can the character move while in the air")]
        private bool moveInAir = true;
        [SerializeField, Tooltip("Air movement smoothing. The larger the value the less the movement is smoothed")]
        private float airMovementFriction = 0.4f;
        [SerializeField, Header("Physics"), Range(0, 1), Tooltip("Force to push rigidbodies. Percent of the body mass.")]
        private float pushForce = 0.75f;
        [SerializeField, Tooltip("Minimum impulse acquired to enable ragdoll")]
        private float minImpulse = 20;
        public float MinImpulse => minImpulse;
        [SerializeField]
        private LayerMask groundMask = default;
        public LayerMask GroundMask => groundMask;
        [SerializeField, Tooltip("Ground check sphere cast radius")]
        private float groundCheckRadius = 0.2f;
        [SerializeField, Tooltip("Ground check sphere cast distance")]
        private float groundCheckDistance = 1;
        [SerializeField, Tooltip("The character and ground min Y pos difference while the ground checking")]
        private float groundCheckThreshold = 0.1f;

        public bool running { get; private set; }
        private bool crouchInput;
        private bool crouching;
        public bool Crouching
        {
            get
            {
                return crouching;
            }
            set
            {
                if (crouching == value)
                    return;

                crouching = value;
                coll.height = startHeight * (crouching ? crouchHeightMultiplier : 1);
                coll.center = Vector3.up * coll.height * 0.5f;
            }
        }
        public Vector3 velocity { get; private set; }
        public Vector3 horizontalVelocity => new Vector3(velocity.x, 0, velocity.z);
        public float maxSpeed => Mathf.Max(crouchSpeed, walkSpeed, runSpeed);
        public bool climbing { get; private set; }
        public bool steppingUp => canStep && rigid.velocity.y > 0 && !climbing;
        public Vector3 climbBoxCheckPoint => climbPoint + Vector3.up * startHeight * 0.42f + transform.forward * climbCheckArea * 0.5f;
        public Vector3 climbBoxCheckSize => new Vector3(climbCheckArea, startHeight * 0.8f, climbCheckArea);
        public Vector3 climbPoint { get; private set; }
        public bool isGrounded { get; private set; } = true;
        public Vector3 MoveDirection { get; private set; }
        public bool climbInput { get; set; } = false;

        private bool active = true;
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                if (active == value)
                    return;

                active = value;
                rigid.velocity = Vector3.zero;
                rigid.isKinematic = !active;
                coll.enabled = active;
                isGrounded = true;
            }
        }

        private bool kinematic = false;
        public bool Kinematic
        {
            get
            {
                return kinematic;
            }
            set
            {
                if (kinematic == value)
                    return;

                kinematic = value;
                rigid.velocity = Vector3.zero;
                rigid.isKinematic = value;
                isGrounded = true;
            }
        }

        private Vector3 newMove;
        private Vector3 oldPos;
        private RaycastHit groundHit;
        private float groundAngle;
        private float fallYPos;
        private bool standCheck;
        private bool jumpCheck;
        private PhysicMaterial physicMaterial;
        private float startHeight;
        private float jumpTimeCount;
        private float jumpCooldownCount;
        private bool canStep;
        private bool canClimb;
        private bool climbSpaceAvailable;

        private const float groundedFriction = 2;
        private const float onSlopeFriction = 0.05f;

        public delegate void OnLanded(float fallHeight);
        public delegate void OnJump();
        public delegate void OnCollider(Collider coll);
        public delegate void OnCollision(Collision coll);

        public OnLanded onLanded { get; set; }
        public OnJump onJump { get; set; }
        public OnCollider onTriggerEnter { get; set; }
        public OnCollider onTriggerExit { get; set; }
        public OnCollision onCollisionEnter { get; set; }
        public OnCollision onCollisionExit { get; set; }

        #region MonoBehaviour methods
        private void Start()
        {
            startHeight = coll.height;
            oldPos = rigid.position;

            if (coll.material == null)
            {
                physicMaterial = new PhysicMaterial("Entity");
                coll.material = physicMaterial;
                coll.material.staticFriction = groundedFriction;
                coll.material.dynamicFriction = groundedFriction;
            }
        }

        private void OnEnable()
        {
            if (ragdollRoot)
            {
                ragdollRoot.onRagdollEnabled += RagdollEnabled;
            }
        }

        private void OnDisable()
        {
            if (ragdollRoot)
            {
                ragdollRoot.onRagdollEnabled -= RagdollEnabled;
            }
        }

        private void FixedUpdate()
        {
            if (rigid.isKinematic)
                return;
            
            CalculateVelocity();
            GroundCheck();
            SlopeCheck();
            HeadCheck();
            CrouchCheck();
            StepCheck();
            UpdateClimbing();
            ApplyMovement();
            ApplyJump();
            ApplyClimb();
        }

        private void Update()
        {
            if (rigid.isKinematic && ragdollRoot && ragdollRoot.RagdollEnabled)
                transform.position = ragdollRoot.groundPos;

            if (rigid.isKinematic)
                return;

            transform.position = Vector3.Lerp(rigid.position, transform.position, 0.5f);

            if (jumpTimeCount > 0)
                jumpTimeCount -= Time.deltaTime;

            if (jumpCooldownCount > 0 && isGrounded)
                jumpCooldownCount -= Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            if (!debugMode)
                return;

            Gizmos.color = isGrounded ? (groundAngle > maxSlope ? Color.yellow : Color.green) : Color.red;
            Utils.GizmosDrawCircle(transform.position, coll.radius * 1.2f);

            Gizmos.color = standCheck ? Color.red : Color.green;
            Utils.GizmosDrawCircle(transform.position + Vector3.up * coll.height, coll.radius * 1.2f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + MoveDirection * 10);

            Gizmos.color = jumpCheck && isGrounded && jumpCooldownCount <= 0 ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, coll.radius * 0.5f);

            Gizmos.color = canStep ? (steppingUp ? Color.yellow : Color.green) : Color.red;
            Gizmos.DrawRay(transform.position + MoveDirection.normalized * (coll.radius + 0.1f), Vector3.up * maxClimbHeight);

            Gizmos.color = climbSpaceAvailable ? Color.green : Color.red;
            Gizmos.DrawWireCube(climbBoxCheckPoint, climbBoxCheckSize);

            if (climbing)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(climbPoint, 0.1f);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter?.Invoke(collision);

            if (ragdollRoot)
            {
                Rigidbody collRigidbody = collision.collider.attachedRigidbody;
                if (collRigidbody && collRigidbody.velocity.magnitude >= 1)
                {
                    float impulse = collRigidbody.mass * collRigidbody.velocity.magnitude;
                    if (impulse >= minImpulse)
                        EnableRagdoll(collRigidbody.velocity, false);
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            onCollisionExit?.Invoke(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            onTriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            onTriggerExit?.Invoke(other);
        }
        #endregion

        #region Private methods
        private void RagdollEnabled(bool ragdollEnabled)
        {
            Active = !ragdollEnabled;
            canStep = false;
            fallYPos = rigid.position.y;
        }

        private void GroundCheck()
        {
            bool oldGrouned = isGrounded;
            if (Physics.SphereCast(transform.position + Vector3.up * groundCheckDistance * 0.5f, groundCheckRadius, Vector3.down, out groundHit, groundCheckDistance, groundMask))
            {
                isGrounded = transform.position.y - groundHit.point.y <= groundCheckThreshold;
                if (!oldGrouned && isGrounded)
                {
                    float fallDist = 0;
                    if (fallYPos > rigid.position.y)
                    {
                        fallDist = fallYPos - rigid.position.y;
                        if (fallDist > 1)
                            onLanded?.Invoke(fallDist);
                    }
                }
            }
            else
            {
                isGrounded = false;
            }

            if (isGrounded)
                fallYPos = rigid.position.y;
        }

        private void SlopeCheck()
        {
            groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);

            coll.material.dynamicFriction = groundAngle > maxSlope || !isGrounded ? onSlopeFriction : groundedFriction;
            coll.material.staticFriction = groundAngle > maxSlope || !isGrounded ? onSlopeFriction : groundedFriction;
        }

        private void HeadCheck()
        {
            if (crouching)
            {
                Ray ray = new Ray(transform.position, Vector3.up);
                standCheck = Physics.SphereCast(ray, 0.2f, startHeight - 0.2f, groundMask);
                jumpCheck = false;
            }
            else
            {
                standCheck = false;
                Ray ray = new Ray(transform.position + Vector3.up * (startHeight - 0.2f), Vector3.up);
                jumpCheck = !Physics.SphereCast(ray, 0.2f, 0.5f, groundMask);
            }
        }

        private void CrouchCheck()
        {
            if (crouchInput == Crouching)
                return;

            if (crouchInput)
            {
                Crouching = true;
            }
            else if (!standCheck)
            {
                Crouching = false;
            }
        }

        private void StepCheck()
        {
            canStep = canClimb = false;

            if (Crouching)
                return;

            if (MoveDirection.magnitude < 0.01f)
                return;

            if (horizontalVelocity.magnitude > Mathf.Min(crouchSpeed, walkSpeed, runSpeed) * 0.5f)
                return;

            Vector3 point = rigid.position + MoveDirection.normalized * (coll.radius + 0.1f);
            Ray ray = new Ray(point + Vector3.up * coll.height, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, coll.height * 2, groundMask))
            {
                float diff = hit.point.y - point.y;
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                float mass = rigid.mass + 1;
                if (hit.collider.attachedRigidbody && !hit.collider.attachedRigidbody.isKinematic)
                    mass = hit.collider.attachedRigidbody.mass;
                climbPoint = hit.point;
                climbSpaceAvailable = !Physics.CheckBox(climbBoxCheckPoint, climbBoxCheckSize / 2, transform.rotation, groundMask);
                bool mainCondition = !hit.collider.isTrigger && angle <= 10 && mass >= rigid.mass * 0.5f && climbSpaceAvailable;
                canStep = mainCondition && diff >= 0.1f && diff <= maxSteppingHeight;
                canClimb = mainCondition && diff > maxSteppingHeight && diff <= maxClimbHeight;
            }
            else
            {
                canStep = canClimb = false;
            }
        }

        private void UpdateClimbing()
        {
            if (!climbing)
            {
                if (canClimb && climbInput)
                {
                    climbing = true;
                    Vector3 vel = rigid.velocity;
                    vel.y = 0;
                    rigid.velocity = vel;
                }
            }
            else
            {
                if (!canClimb && !canStep)
                    climbing = false;
            }
        }

        private void ApplyMovement()
        {
            float speed = Crouching ? crouchSpeed : (running ? runSpeed : walkSpeed);
            newMove = Vector3.ClampMagnitude(newMove.normalized, 1) * speed * Time.fixedDeltaTime;
            newMove = Vector3.ProjectOnPlane(newMove, groundHit.normal);
            MoveDirection = Vector3.Lerp(MoveDirection, newMove, Time.fixedDeltaTime * (isGrounded ? movementFriction : airMovementFriction));

            if (groundAngle > maxSlope)
                MoveDirection = Vector3.Lerp(MoveDirection, Vector3.zero, Time.fixedDeltaTime * 5);

            oldPos = rigid.position;
            Vector3 newPos = rigid.position + MoveDirection;

            Collider[] colls = Physics.OverlapCapsule(newPos + Vector3.up * coll.radius, newPos + Vector3.up * (coll.height - coll.radius), coll.radius);
            
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i] == coll)
                    continue;

                if (colls[i].isTrigger)
                    continue;

                if (colls[i].attachedRigidbody != null && colls[i].attachedRigidbody.mass <= rigid.mass * pushForce)
                    continue;

                if (Physics.ComputePenetration(coll, newPos, Quaternion.identity, colls[i], colls[i].transform.position, colls[i].transform.rotation, out Vector3 dir, out float dist))
                {
                    if (dist > MoveDirection.magnitude * 0.9f)
                    {
                        newPos = rigid.position;
                        return;
                    }
                }
            }

            rigid.MovePosition(newPos);
        }

        private void ApplyJump()
        {
            if (jumpTimeCount <= 0)
                return;

            Vector3 vel = rigid.velocity;
            vel.y = jumpForce;
            rigid.velocity = vel;

            if (isGrounded && jumpTimeCount > 0 && jumpTimeCount <= jumpTime * 0.8f)
            {
                jumpTimeCount = 0;
            }
        }

        private void ApplyClimb()
        {
            if (!canStep && !climbing)
                return;

            Vector3 vel = rigid.velocity;
            vel.y = steppingUp ? walkSpeed * 0.5f : climbSpeed;
            rigid.velocity = vel;
        }

        private void CalculateVelocity()
        {
            Vector3 newVel = (rigid.position - oldPos) / Time.fixedDeltaTime;
            newVel.y = rigid.velocity.y;
            velocity = newVel;
        }
        #endregion

        #region Public methods
        ///<inheritdoc/>
        public void Move(Vector3 dir, bool running = false)
        {
            if (!isGrounded && !moveInAir)
                return;

            this.running = running;
            newMove = dir;
        }

        ///<inheritdoc/>
        public void Crouch(bool crouching)
        {
            crouchInput = crouching;
        }

        ///<inheritdoc/>
        public void Jump()
        {
            if (!isGrounded || jumpCooldownCount > 0 || !jumpCheck)
                return;

            isGrounded = false;
            jumpTimeCount = jumpTime;
            jumpCooldownCount = jumpCooldown;
            onJump?.Invoke();
        }

        ///<inheritdoc/>
        public void SetPosition(Vector3 position)
        {
            if (ragdollRoot && ragdollRoot.RagdollEnabled)
            {
                ragdollRoot.DisableRagdoll(true);
            }
            rigid.position = position;
            transform.position = position;
            fallYPos = rigid.position.y;
        }

        ///<inheritdoc/>
        public void EnableRagdoll(bool stay = false)
        {
            ragdollRoot.EnableRagdoll(velocity, stay);
        }

        ///<inheritdoc/>
        public void EnableRagdoll(Vector3 force, bool stay = false)
        {
            ragdollRoot.EnableRagdoll(velocity + force, stay);
        }

        ///<inheritdoc/>
        public void Force(Vector3 direction, float force, ForceMode forceMode)
        {
            rigid.AddForce(direction * force, forceMode);
        }
        #endregion
    }
}