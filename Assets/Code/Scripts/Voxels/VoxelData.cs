using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Voxels
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VoxelData : MonoBehaviour, IEnumerable<KeyValuePair<Vector3Int, Color>>
    {
        public Color fillVoxel = Color.white;
    
        private Dictionary<Vector3Int, Color> data = new();
        private Vector2Int min, max;

        public event Action<Vector3Int> DataChangedEvent;
    
        public Vector3Int ToKey(Vector3 worldPosition)
        {
            var position = transform.InverseTransformPoint(worldPosition);
            return Vector3Int.RoundToInt(position);
        }
    
        public Vector3 FromKey(Vector3Int key) => transform.TransformPoint(key);
    
        public Color? this[Vector3Int key]
        {
            get => data.ContainsKey(key) ? data[key] : null;
            set
            {
                var oldValue = this[key];
                if (oldValue == value) return;
            
                if (data.ContainsKey(key))
                {
                    if (value.HasValue) data[key] = value.Value;
                    else data.Remove(key);
                }
                else if (value.HasValue)
                {
                    data.Add(key, value.Value);
                }

                if (data.Count == 0)
                {
                    this[Vector3Int.zero] = fillVoxel;
                }
                
                DataChangedEvent?.Invoke(key);
            }
        }

        public IEnumerator<KeyValuePair<Vector3Int, Color>> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        private void OnEnable()
        {
            const int radius = 5;
            const int size = 2 * radius + 1;
        
            for (var i = 0; i < size * size * size; i++)
            {
                var x = (i % size) - radius;
                var y = (i / size) % size - radius;
                var z = (i / size) / size - radius;

                var key = new Vector3Int(x, y, z);
                if (key.magnitude > radius) continue;

                this[key] = fillVoxel;
            }
        }
    }
}
