using System.Collections.Generic;
using UnityEngine;

namespace Code.Voxels
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(VoxelData))]
    public class VoxelCollision : MonoBehaviour
    {
        private VoxelData data;
        private Transform colliderParent;

        private Dictionary<Vector3Int, BoxCollider> colliders = new();

        private void Awake()
        {
            data = GetComponent<VoxelData>();
            colliderParent = new GameObject("Colliders").transform;
            colliderParent.SetParent(transform);
            colliderParent.localPosition = Vector3.zero;
            colliderParent.localRotation = Quaternion.identity;
            colliderParent.localScale = Vector3.one;
        }

        private void OnEnable() { data.DataChangedEvent += OnDataChanged; }

        private void OnDisable() { data.DataChangedEvent -= OnDataChanged; }

        private void OnDataChanged(Vector3Int key)
        {
            var voxel = data[key];

            if (voxel.HasValue == colliders.ContainsKey(key)) return;

            if (voxel.HasValue)
            {
                colliders.Add(key, GetCollider(key));
            }
            else
            {
                var collider = colliders[key];
                colliders.Remove(key);
                Destroy(collider.gameObject);
            }
        }

        private BoxCollider GetCollider(Vector3Int key)
        {
            var collider = new GameObject().AddComponent<BoxCollider>();

            collider.transform.SetParent(colliderParent);
            collider.transform.position = data.FromKey(key);
            collider.transform.localRotation = Quaternion.identity;
            collider.gameObject.name = $"[{key.x}, {key.y}, {key.z}]VoxelCollider";

            return collider;
        }
    }
}