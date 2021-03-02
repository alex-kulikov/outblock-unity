using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OutBlock
{

    /// <summary>
    /// Display of the vision of the entity
    /// </summary>
    public class AIVisionCone : MonoBehaviour
    {

        [SerializeField]
        private AIEntity entity = null;
        [SerializeField]
        private AISensors sensor = null;
        [SerializeField]
        private int minResolution = 24;
        [SerializeField]
        private int maxResolution = 32;
        [SerializeField]
        private Material innerMaterial = null;
        [SerializeField]
        private Material outerMaterial = null;
        [SerializeField]
        private bool alwaysUpdate = false;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private int resolution;
        private Vector3 oldRot;
        private Vector3 oldPos;
        private Color innerColor;
        private Color outerColor;
        private float updateTime = 0;

        public const float maxDist = 20;
        private const float updateTimeOut = 1;

        private void Start()
        {
            mesh = new Mesh();

            MeshFilter mFilter = gameObject.GetComponent<MeshFilter>();
            if (!mFilter)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            else meshFilter = mFilter;
            meshFilter.mesh = mesh;

            MeshRenderer mRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!mRenderer)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            else meshRenderer = mRenderer;

            meshRenderer.materials = new Material[] { outerMaterial, innerMaterial };
            innerColor = innerMaterial.GetColor("_BaseColor");
            outerColor = outerMaterial.GetColor("_BaseColor");
            resolution = minResolution;
        }

        private void FixedUpdate()
        {
            if (!entity.enabled || !Player.Instance || AIManager.closestToPlayer != entity)
            {
                if (mesh.vertexCount > 0)
                    mesh.Clear();

                return;
            }

            float dist = Mathf.Clamp01(Vector3.Distance(Player.Instance.transform.position, transform.position) / maxDist);
            meshRenderer.materials[0].SetColor("_BaseColor", outerColor * new Vector4(1, 1, 1, 1 - dist));
            meshRenderer.materials[1].SetColor("_BaseColor", innerColor * new Vector4(1, 1, 1, 1 - dist));

            if (dist >= 1)
                return;

            if (oldRot != sensor.transform.eulerAngles || oldPos != sensor.transform.position)
                updateTime = updateTimeOut;
            else if (updateTime > 0)
                updateTime -= Time.deltaTime;

            if (alwaysUpdate || updateTime > 0)
            {
                resolution = (int)Mathf.Lerp(maxResolution, minResolution, dist);
                oldRot = sensor.transform.eulerAngles;
                oldPos = sensor.transform.position;

                mesh.Clear();
                List<Vector3> vertices = new List<Vector3>();
                List<int> verticesInner = new List<int>();
                List<int> trianglesOuter = new List<int>();
                List<int> trianglesInner = new List<int>();
                Vector3 originVertex = sensor.VisionPivot.position;
                originVertex.y = sensor.transform.position.y + 0.1f;
                originVertex = transform.InverseTransformPoint(originVertex);
                vertices.Add(originVertex);

                float step = sensor.VisionOuterConeAngle / (resolution - 1);
                for (int i = 0; i < resolution; i++)
                {
                    float angle = -sensor.VisionOuterConeAngle / 2 + (i * step);
                    Vector3 origin = sensor.VisionPivot.position;
                    Vector3 endPoint = origin + Quaternion.AngleAxis(angle, sensor.VisionPivot.up) * sensor.VisionPivot.forward * sensor.VisionDistance;
                    endPoint.y = sensor.transform.position.y + 0.1f;
                    if (Physics.Linecast(origin, endPoint, out RaycastHit hit, sensor.VisionLayerMask))
                    {
                        Vector3 point = hit.point;
                        point.y = sensor.transform.position.y + 0.1f;
                        vertices.Add(transform.InverseTransformPoint(point));
                    }
                    else
                    {
                        vertices.Add(transform.InverseTransformPoint(endPoint));
                    }

                    if (Mathf.Abs(angle) < sensor.VisionInnerConeAngle)
                        verticesInner.Add(i);
                }

                for (int i = 1; i < resolution - 1; i++)
                {
                    if (verticesInner.Contains(i))
                    {
                        trianglesInner.Add(0);
                        trianglesInner.Add(i);
                        trianglesInner.Add(i + 1);
                    }
                    else
                    {
                        trianglesOuter.Add(0);
                        trianglesOuter.Add(i);
                        trianglesOuter.Add(i + 1);
                    }
                }

                mesh.subMeshCount = 2;
                mesh.SetVertices(vertices);
                mesh.SetTriangles(trianglesOuter, 0);
                mesh.SetTriangles(trianglesInner, 1);
            }
            else return;
        }
    }
}