using System.Collections.Generic;
using UnityEngine;

namespace Code.Voxels
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(VoxelData))]
    public class VoxelRenderer : MonoBehaviour
    {
        public Mesh cube;

        private VoxelData data;
        private MeshFilter filter;
        private Mesh mesh;
        private bool dirty;

        private void OnEnable()
        {
            mesh = new Mesh();
            mesh.name = "VoxelRenderer.Generated";
            mesh.hideFlags = HideFlags.HideAndDontSave;

            filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            data = GetComponent<VoxelData>();

            var collider = GetComponent<MeshCollider>();
            if (collider)
            {
                collider.sharedMesh = mesh;
            }

            data.DataChangedEvent += OnDataChanged;
            RegenerateMesh();
        }

        private void OnDisable()
        {
            data.DataChangedEvent -= OnDataChanged;

            Destroy(mesh);
        }

        private void Update()
        {
            if (dirty)
            {
                dirty = false;
                RegenerateMesh();
            }
        }

        private void OnDataChanged(Vector3Int key) { dirty = true; }

        private void RegenerateMesh()
        {
            var visible = new List<Vector3Int>();
            var vertexCount = 0;
            var triCount = 0;

            foreach (var e in data)
            {
                if (!IsVisible(e.Key)) continue;

                visible.Add(e.Key);
                vertexCount += cube.vertexCount;
                triCount += (int)cube.GetIndexCount(0);
            }

            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var colors = new Color[vertexCount];

            var indices = new int[triCount];

            var vertexHead = 0;
            var indexHead = 0;

            foreach (var key in visible)
            {
                var color = data[key];

                for (var i0 = 0; i0 < cube.vertexCount; i0++)
                {
                    var i1 = i0 + vertexHead;
                    vertices[i1] = transformVertex(key, cube.vertices[i0]);
                    normals[i1] = transformNormal(cube.normals[i0]);
                    uvs[i1] = cube.uv[i0];

                    colors[i1] = color ?? Color.clear;
                }

                for (var i0 = 0; i0 < cube.GetIndexCount(0); i0++)
                {
                    var i1 = i0 + indexHead;
                    indices[i1] = vertexHead + cube.triangles[i0];
                }

                vertexHead += cube.vertexCount;
                indexHead += (int)cube.GetIndexCount(0);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);

            mesh.SetTriangles(indices, 0);

            Vector3 transformVertex(Vector3Int key, Vector3 vertex)
            {
                var worldVertex = data.transform.TransformVector(vertex);
                return data.FromKey(key) + worldVertex;
            }

            Vector3 transformNormal(Vector3 normal) { return data.transform.TransformVector(normal); }
        }

        private bool IsVisible(Vector3Int key)
        {
            if (!data[key + Vector3Int.up].HasValue) return true;
            if (!data[key + Vector3Int.down].HasValue) return true;
            if (!data[key + Vector3Int.left].HasValue) return true;
            if (!data[key + Vector3Int.right].HasValue) return true;
            if (!data[key + Vector3Int.forward].HasValue) return true;
            if (!data[key + Vector3Int.back].HasValue) return true;

            return false;
        }
    }
}