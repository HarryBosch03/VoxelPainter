using Code.User;
using Code.Voxels;
using UnityEngine;

namespace Code.Brushes
{
    public class PixelBrush : Brush
    {
        public override KeyCode KeyBinding => KeyCode.E;

        public override void PaintLoop(CameraInteractionManager caller, Color? brush, Vector3Int key, VoxelData target)
        {
            target[key] = brush;
        }
    }
}