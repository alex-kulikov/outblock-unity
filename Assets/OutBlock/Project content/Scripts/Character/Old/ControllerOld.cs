using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{
    public class ControllerOld : MonoBehaviour, IController
    {

        [SerializeField, Header("Main")]
        private CharacterController characterController = null;
        [SerializeField]
        private CapsuleCollider coll = null;
        [SerializeField]
        private RagdollRoot ragdollRoot = null;
        [SerializeField, Header("Stats")]
        private LayerMask groundMask = default;
        [SerializeField]
        private float runSpeed = 8;
        [SerializeField]
        private float walkSpeed = 4;
        [SerializeField]
        private float crouchSpeed = 1.5f;
        [SerializeField]
        private float pushPower = 2;
        [SerializeField]
        private float jumpCooldown;
        [SerializeField]
        private float jumpHeight;
        [SerializeField, Header("Ragdoll")]
        private float minImpulse = 2;

        public Collider Collider => coll;
        public LayerMask GroundMask => groundMask;
        public float MinImpulse => minImpulse;
        public CapsuleCollider CapsuleCollider => coll;
        public RagdollRoot RagdollRoot => ragdollRoot;

        public bool running { get; private set; }

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
                coll.height = startHeight * (crouching ? 0.65f : 1);
                coll.center = Vector3.up * coll.height * 0.5f;
                characterController.height = coll.height;
                characterController.center = coll.center;
            }
        }

        public Vector3 velocity => characterController.velocity;
        public Vector3 horizontalVelocity => new Vector3(velocity.x, 0, velocity.z);
        public float maxSpeed => Mathf.Max(crouchSpeed, walkSpeed, runSpeed);
        public bool climbing => false;
        public bool steppingUp => false;
        public Vector3 climbPoint => Vector3.zero;
        public bool isGrounded { get; private set; } = true;

        private Vector3 moveDirection;
        public Vector3 MoveDirection
        {
            get => moveDirection;
            private set => moveDirection = value;
        }

        public bool climbInput { get; set; } = false;

        private bool active;
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
                characterController.enabled = active;
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
                characterController.enabled = !kinematic;
                isGrounded = true;
            }
        }

        public Character.OnLanded onLanded { get; set; }
        public Character.OnJump onJump { get; set; }
        public Character.OnCollider onTriggerEnter { get; set; }
        public Character.OnCollider onTriggerExit { get; set; }
        public Character.OnCollision onCollisionEnter { get; set; }
        public Character.OnCollision onCollisionExit { get; set; }

        private Vector3 inputDirection;
        private bool inputRunning;
        private bool crouchInput;
        private bool standCheck;
        private float startHeight;
        private float jumpTime;
        private RaycastHit groundHit;
        private float gravity;
        private float speed;
        private float jumpY;
        private float fallDist;
        private float jumpReload;

        private void Start()
        {
            startHeight = coll.height;
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
            CrouchCheck();

            if (jumpTime >= 0.9f)
            {
                isGrounded = false;
                return;
            }

            if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.2f, Vector3.down, out groundHit, 1f, groundMask))
            {
                float angle = Vector3.Angle(Vector3.up, groundHit.normal);
                isGrounded = transform.position.y - groundHit.point.y <= 0.1f;
                if (angle > characterController.slopeLimit)
                    isGrounded = false;
            }
            else
            {
                isGrounded = false;
            }
        }

        private void Update()
        {
            if (ragdollRoot && ragdollRoot.RagdollEnabled)
                SetPosition(ragdollRoot.groundPos);

            if (ragdollRoot.RagdollEnabled)
                return;

            if (!Active)
                return;

            if (isGrounded)
            {
                if (gravity < 0)
                {
                    if (transform.position.y > jumpY)
                        fallDist = 0;
                    else fallDist = Mathf.Abs(transform.position.y - jumpY);
                    jumpReload = jumpCooldown;
                    onLanded?.Invoke(fallDist);
                }
                gravity = 0;

                if (transform.position.y < groundHit.point.y)
                    transform.position = groundHit.point;

                if (jumpTime != 0)
                    jumpTime = 0;
            }
            else
            {
                if (gravity == 0)
                    jumpY = transform.position.y;

                gravity -= 9.81f * Time.deltaTime;
            }

            running = !crouching && inputRunning && inputDirection.magnitude >= 0.1f;

            if (jumpTime > 0)
                jumpTime -= Time.deltaTime;

            if (jumpReload > 0)
                jumpReload -= Time.deltaTime;

            speed = walkSpeed;
            if (running)
                speed = runSpeed;
            else if (crouching)
                speed = crouchSpeed;

            moveDirection = new Vector3(inputDirection.x, 0, inputDirection.z);
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1);
            moveDirection *= speed;
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundHit.normal);

            if (!isGrounded)
            {
                moveDirection.x += (1f - groundHit.normal.y) * groundHit.normal.x * -gravity;
                moveDirection.z += (1f - groundHit.normal.y) * groundHit.normal.z * -gravity;
            }

            characterController.Move(moveDirection * Time.deltaTime);

            characterController.enabled = false;
            transform.position += Vector3.up * (jumpTime * jumpHeight * 7.5f + gravity) * Time.deltaTime;
            characterController.enabled = active;
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

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (!body || body.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3f)
                return;

            Vector3 dir = hit.moveDirection;
            dir.y = hit.transform.position.y;

            body.AddForce(dir * pushPower);
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

        private void RagdollEnabled(bool ragdollEnabled)
        {
            Active = !ragdollEnabled;
            jumpY = transform.position.y;
        }

        public void Crouch(bool crouching)
        {
            crouchInput = crouching;
        }

        public void EnableRagdoll(bool stay = false)
        {
            ragdollRoot.EnableRagdoll(velocity, stay);
        }

        public void EnableRagdoll(Vector3 force, bool stay = false)
        {
            ragdollRoot.EnableRagdoll(velocity + force, stay);
        }

        public void Force(Vector3 direction, float force, ForceMode forceMode)
        {
        }

        public void Jump()
        {
            if (jumpReload <= 0 && isGrounded && jumpTime <= 0)
            {
                jumpTime = 1;
                isGrounded = false;
                onJump?.Invoke();
            }
        }

        public void Move(Vector3 dir, bool running)
        {
            inputDirection = dir;
            inputRunning = running;
        }

        public void SetPosition(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = Active;
            jumpY = position.y;
        }
    }
}