using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// With this class you can move, rotate, teleport object. All of the functions are accessible via UnityEvents.
    /// </summary>
    public class Transportable : MonoBehaviour, ISaveable
    {

        public class TransportableSaveData : SaveData
        {

            public struct TransportableRuntimeData
            {
                public bool Pause { get; set; }
                public bool Moving { get; set; }
                public bool Rotating { get; set; }
                public Vector3 TargetPos { get; set; }
                public Quaternion TargetRot { get; set; }
                public float MoveTime { get; set; }
                public float RotTime { get; set; }
                public float T { get; set; }
                public float TRot { get; set; }
                public float RotSpeed { get; set; }
                public float MoveSpeed { get; set; }
            }

            public TransportableRuntimeData RuntimeData { get; private set; }

            public TransportableSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, TransportableRuntimeData runtimeData) : base(id, pos, rot, active, enabled)
            {
                RuntimeData = runtimeData;
            }
        }

        /// <summary>
        /// Positional constraints.
        /// </summary>
        [System.Serializable]
        public class Constraints
        {

            [SerializeField]
            private bool x = false;
            [SerializeField]
            private Vector2 xConstraints = new Vector2();
            [SerializeField]
            private bool y = false;
            [SerializeField]
            private Vector2 yConstraints = new Vector2();
            [SerializeField]
            private bool z = false;
            [SerializeField]
            private Vector2 zConstraints = new Vector2();

            public Vector3 ApplyConstraints(Vector3 target)
            {
                Vector3 result = target;

                if (x)
                    result.x = Mathf.Clamp(target.x, xConstraints.x, xConstraints.y);

                if (y)
                    result.y = Mathf.Clamp(target.y, yConstraints.x, yConstraints.y);

                if (z)
                    result.z = Mathf.Clamp(target.z, zConstraints.x, zConstraints.y);

                return result;
            }

        }

        [SerializeField]
        private bool moveByPhysics = false;
        [SerializeField]
        private Space relativeTo = Space.World;
        [SerializeField, Header("Rotation"), Tooltip("Degrees per second")]
        private float rotationSpeed = 0;
        [SerializeField, Header("Movement")]
        private float moveSpeed = 1;
        [SerializeField, Space]
        private bool useConstraints = false;
        [SerializeField]
        private Constraints constraints = new Constraints();

        public int Id { get; set; } = -1;
        public GameObject GO => gameObject;

        private bool paused;

        private Rigidbody rigid;

        private bool moving;
        private Vector3 startPos;
        private Vector3 targetPos;
        private float moveTime;
        private float t;

        private bool rotating;
        private Quaternion startRot;
        private Quaternion targetRot;
        private float rotTime;
        private float tRot;

        private void Awake()
        {
            if (moveByPhysics && !rigid)
            {
                FindRigidbody();
            }
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Update()
        {
            if (paused)
                return;

            if (!moveByPhysics)
            {
                if (moving)
                {
                    Vector3 newPos = transform.position;
                    if (t < 1)
                    {
                        t += Time.deltaTime / moveTime;

                        newPos = Vector3.Lerp(startPos, targetPos, t);
                    }
                    else
                    {
                        moving = false;
                    }

                    if (useConstraints)
                    {
                        newPos = constraints.ApplyConstraints(newPos);
                    }

                    transform.position = newPos;
                }

                if (rotating)
                {
                    Quaternion newRot = transform.rotation;
                    if (tRot < 1)
                    {
                        tRot += Time.deltaTime / rotTime;

                        newRot = Quaternion.Slerp(startRot, targetRot, tRot);
                    }
                    else
                    {
                        rotating = false;
                    }

                    transform.rotation = newRot;
                }
            }
        }

        private void FixedUpdate()
        {
            if (paused)
                return;

            if (moveByPhysics)
            {
                if (!rigid)
                {
                    FindRigidbody();
                }
                else
                {
                    if (moving)
                    {
                        Vector3 newPos = rigid.position;
                        if (t < 1)
                        {
                            t += Time.fixedDeltaTime / moveTime;

                            newPos = Vector3.Lerp(startPos, targetPos, t);
                        }
                        else
                        {
                            moving = false;
                        }

                        if (useConstraints)
                        {
                            newPos = constraints.ApplyConstraints(newPos);
                        }

                        rigid.position = newPos;
                    }

                    if (rotating)
                    {
                        Quaternion newRot = rigid.rotation;
                        if (tRot < 1)
                        {
                            tRot += Time.fixedDeltaTime / rotTime;

                            newRot = Quaternion.Slerp(startRot, targetRot, tRot);
                        }
                        else
                        {
                            rotating = false;
                        }

                        rigid.MoveRotation(newRot);
                    }
                }
            }
        }

        private void FindRigidbody()
        {
            rigid = GetComponent<Rigidbody>();
            if (!rigid)
            {
                rigid = gameObject.AddComponent<Rigidbody>();
                rigid.isKinematic = true;
                rigid.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        private void Move(Vector3 moveVector)
        {
            moving = true;
            t = 0;
            if (moveByPhysics)
            {
                startPos = rigid.position;
                targetPos = rigid.position + (relativeTo == Space.World ? moveVector : transform.InverseTransformVector(moveVector));
            }
            else
            {
                startPos = transform.position;
                targetPos = transform.position + (relativeTo == Space.World ? moveVector : transform.InverseTransformVector(moveVector));
            }
            float dist = Vector3.Distance(startPos, targetPos);
            moveTime = dist / moveSpeed;
        }

        private void MoveTo(Vector3 moveVector)
        {
            moving = true;
            t = 0;
            if (moveByPhysics)
            {
                startPos = rigid.position;
            }
            else
            {
                startPos = transform.position;
            }
            targetPos = (relativeTo == Space.World ? moveVector : transform.InverseTransformVector(moveVector));
            float dist = Vector3.Distance(startPos, targetPos);
            moveTime = dist / moveSpeed;
        }

        private void SetPos(Vector3 pos)
        {
            moving = false;

            if (useConstraints)
                pos = constraints.ApplyConstraints(pos);

            if (moveByPhysics)
            {
                if (relativeTo == Space.World)
                {
                    rigid.MovePosition(pos);
                }
                else
                {
                    rigid.MovePosition(transform.InverseTransformPoint(pos));
                }
            }
            else
            {
                if (relativeTo == Space.World)
                {
                    transform.position = pos;
                }
                else
                {
                    transform.localPosition = pos;
                }
            }
        }

        private void Rotate(Vector3 euler)
        {
            rotating = true;
            tRot = 0;

            if (moveByPhysics)
            {
                startRot = rigid.rotation;
                targetRot = startRot * Quaternion.Euler(euler);
            }
            else
            {
                startRot = transform.rotation;
                targetRot = startRot * Quaternion.Euler(euler);
            }

            rotTime = euler.magnitude / rotationSpeed;
        }

        private void RotateTo(Vector3 euler)
        {
            rotating = true;
            tRot = 0;

            startRot = relativeTo == Space.World ? transform.rotation : transform.localRotation;
            targetRot = (relativeTo == Space.World ? Quaternion.Euler(euler) : Quaternion.Euler(transform.InverseTransformVector(euler)));

            rotTime = (targetRot.eulerAngles - startRot.eulerAngles).magnitude / rotationSpeed;
        }

        private void SetRot(Quaternion rotation)
        {
            rotating = false;

            if (moveByPhysics)
            {
                if (relativeTo == Space.World)
                    rigid.rotation = rotation;
                else rigid.rotation = Quaternion.Euler(transform.InverseTransformVector(rotation.eulerAngles));
            }
            else
            {
                if (relativeTo == Space.World)
                    transform.rotation = rotation;
                else transform.localRotation = rotation;
            }
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> by value on the X axis.
        /// </summary>
        public void RotateX(float rotation)
        {
            Rotate(Vector3.right * rotation);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> by value on the Y axis.
        /// </summary>
        public void RotateY(float rotation)
        {
            Rotate(Vector3.up * rotation);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> by value on the Z axis.
        /// </summary>
        public void RotateZ(float rotation)
        {
            Rotate(Vector3.forward * rotation);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> by vector.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void Rotate(string vector)
        {
            Rotate(Utils.StringToVector3(vector));
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> to the value on the X axis.
        /// </summary>
        public void RotateToX(float rotation)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.x = rotation;
            RotateTo(newRot);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> to the value on the Y axis.
        /// </summary>
        public void RotateToY(float rotation)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.y = rotation;
            RotateTo(newRot);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> to the value on the Z axis.
        /// </summary>
        public void RotateToZ(float rotation)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.z = rotation;
            RotateTo(newRot);
        }

        /// <summary>
        /// Rotates object with <i>rotation speed</i> to the vector.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void RotateTo(string vector)
        {
            RotateTo(Utils.StringToVector3(vector));
        }

        /// <summary>
        /// Set rotation speed.
        /// </summary>
        public void SetRotationSpeed(float rotationSpeed)
        {
            this.rotationSpeed = rotationSpeed;
            rotTime = (targetRot.eulerAngles - startRot.eulerAngles).magnitude / this.rotationSpeed;
        }

        /// <summary>
        /// Set X angle of the object.
        /// </summary>
        public void SetRotationX(float rot)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.x = rot;
            SetRot(Quaternion.Euler(newRot));
        }

        /// <summary>
        /// Set Z angle of the object.
        /// </summary>
        public void SetRotationY(float rot)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.y = rot;
            SetRot(Quaternion.Euler(newRot));
        }

        /// <summary>
        /// Set Y angle of the object.
        /// </summary>
        public void SetRotationZ(float rot)
        {
            Vector3 newRot = relativeTo == Space.World ? transform.eulerAngles : transform.localEulerAngles;
            newRot.z = rot;
            SetRot(Quaternion.Euler(newRot));
        }

        /// <summary>
        /// Set rotation angle of the object.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void SetRotation(string vector)
        {
            Vector3 newRot = Utils.StringToVector3(vector);
            newRot = relativeTo == Space.World ? newRot : transform.InverseTransformVector(newRot);
            SetRot(Quaternion.Euler(newRot));
        }

        /// <summary>
        /// Moves object with <i>speed</i> by value on the X axis.
        /// </summary>
        public void MoveX(float move)
        {
            Move(Vector3.right * move);
        }

        /// <summary>
        /// Moves object with <i>speed</i> by value on the Y axis.
        /// </summary>
        public void MoveY(float move)
        {
            Move(Vector3.up * move);
        }

        /// <summary>
        /// Moves object with <i>speed</i> by value on the Z axis.
        /// </summary>
        public void MoveZ(float move)
        {
            Move(Vector3.forward * move);
        }

        /// <summary>
        /// Moves object with <i>speed</i> by vector.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void Move(string vector)
        {
            Move(Utils.StringToVector3(vector));
        }

        /// <summary>
        /// Moves object with <i>speed</i> to the value on the X axis.
        /// </summary>
        public void MoveToX(float move)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.x = move;
            MoveTo(newPos);
        }

        /// <summary>
        /// Moves object with <i>speed</i> to the value on the Y axis.
        /// </summary>
        public void MoveToY(float move)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.y = move;
            MoveTo(newPos);
        }

        /// <summary>
        /// Moves object with <i>speed</i> to the value on the Z axis.
        /// </summary>
        public void MoveToZ(float move)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.z = move;
            MoveTo(newPos);
        }

        /// <summary>
        /// Moves object with <i>speed</i> to the position of the vector.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void MoveTo(string vector)
        {
            MoveTo(Utils.StringToVector3(vector));
        }

        /// <summary>
        /// Set X position.
        /// </summary>
        public void SetX(float pos)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.x = pos;
            SetPos(newPos);
        }

        /// <summary>
        /// Set Y position.
        /// </summary>
        public void SetY(float pos)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.y = pos;
            SetPos(newPos);
        }

        /// <summary>
        /// Set Z position.
        /// </summary>
        public void SetZ(float pos)
        {
            Vector3 newPos = relativeTo == Space.World ? transform.position : transform.localPosition;
            newPos.z = pos;
            SetPos(newPos);
        }

        /// <summary>
        /// Set position.
        /// </summary>
        /// <param name="vector">Converts string to the Vector3. <see cref="Utils.StringToVector3(string)"/></param>
        public void SetPosition(string vector)
        {
            SetPos(Utils.StringToVector3(vector));
        }

        /// <summary>
        /// Set movement speed.
        /// </summary>
        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
            float dist = Vector3.Distance(startPos, targetPos);
            moveTime = dist / moveSpeed;
        }

        /// <summary>
        /// Pause/Unpause the movement.
        /// </summary>
        public void Pause()
        {
            paused = !paused;
        }

        #region SaveLoad
        public void Register()
        {
            SaveLoad.Add(this);
        }

        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public SaveData Save()
        {
            TransportableSaveData.TransportableRuntimeData runtimeData = new TransportableSaveData.TransportableRuntimeData()
            {
                MoveSpeed = moveSpeed,
                MoveTime = moveTime,
                Moving = moving,
                Pause = paused,
                RotSpeed = rotationSpeed,
                T = t,
                TargetPos = targetPos,
                Rotating = rotating,
                RotTime = rotTime,
                TargetRot = targetRot,
                TRot = tRot
            };
            return new TransportableSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, runtimeData);

        }

        public void Load(SaveData data)
        {
            SaveLoadUtils.BasicLoad(this, data);
            paused = true;
            if (moveByPhysics)
            {
                rigid.position = data.pos;
            }
            if (data is TransportableSaveData saveData)
            {
                moveSpeed = saveData.RuntimeData.MoveSpeed;
                moveTime = saveData.RuntimeData.MoveTime;
                moving = saveData.RuntimeData.Moving;
                paused = saveData.RuntimeData.Pause;
                rotationSpeed = saveData.RuntimeData.RotSpeed;
                t = saveData.RuntimeData.T;
                targetPos = saveData.RuntimeData.TargetPos;
                rotating = saveData.RuntimeData.Rotating;
                rotTime = saveData.RuntimeData.RotTime;
                targetRot = saveData.RuntimeData.TargetRot;
                tRot = saveData.RuntimeData.TRot;
            }
            paused = false;
        }
        #endregion
    }
}