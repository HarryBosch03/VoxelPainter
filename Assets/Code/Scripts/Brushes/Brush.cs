using System;
using System.IO;
using Code.User;
using Code.Voxels;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace Code.Brushes
{
    [Serializable]
    public abstract class Brush
    {
        public Texture2D Icon { get; private set; }
        public abstract KeyCode KeyBinding { get; }

        protected Brush()
        {
            var spriteFilename = Path.Combine(Application.dataPath, "BrushIcons", $"{GetType().Name}.png");

            Icon = new Texture2D(2, 2);
            if (File.Exists(spriteFilename))
            {
                var bytes = File.ReadAllBytes(spriteFilename);
                Icon.LoadImage(bytes);
                Icon.filterMode = FilterMode.Point;
            }
        }

        ~Brush()
        {
            UnityEngine.Object.Destroy(Icon);
        }

        public virtual void PaintStart(CameraInteractionManager caller, Color? brush, Vector3Int key, VoxelData target) { }
        public virtual void PaintLoop(CameraInteractionManager caller, Color? brush, Vector3Int key, VoxelData target) { }
        public virtual void PaintEnd(CameraInteractionManager caller, Color? brush, Vector3Int key, VoxelData target) { }
    }
}