using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Brushes;
using Code.Voxels;
using UnityEngine;
using Directory = UnityEngine.Windows.Directory;

namespace Code.User
{
    [SelectionBase, DisallowMultipleComponent]
    public class CameraInteractionManager : MonoBehaviour
    {
        public float brushHue;
        public float brushSaturation;
        public float brushValue;
        public bool erase;

        public List<Brush> brushes;
        public int brushIndex;

        private Transform grid;

        private new Camera camera;
        private bool didMouseHit;
        private RaycastHit mouseHit;
        private Ray mouseRay;

        private Plane paintPlane;
        private bool painting;
        private VoxelData paintingTarget;
        private int paintingInputIndex;
        private Vector3 paintPosition;
        private float normalOffset;

        private Vector3Int paintStartKey;
        private Vector3Int paintEndKey;
        private Material guiMaterial;
        private Material hueSquareMaterial;
        private Material hueSliderMaterial;

        public Brush CurrentBrush => brushes[brushIndex];
        public Color? PaintColor => !erase ? Color.HSVToRGB(brushHue, brushSaturation, brushValue) : null;

        private void Awake()
        {
            camera = Camera.main;

            grid = transform.Find("Grid");

            LoadBrushes();
        }

        private void OnEnable()
        {
            guiMaterial = new Material(Shader.Find("Unlit/VertexColor"));
            hueSquareMaterial = new Material(Shader.Find("Unlit/HueSquare"));
            hueSliderMaterial = new Material(Shader.Find("Unlit/HueSlider"));
            
            guiMaterial.hideFlags = HideFlags.HideAndDontSave;
            hueSquareMaterial.hideFlags = HideFlags.HideAndDontSave;
            hueSliderMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        private void OnDisable()
        {
            Destroy(guiMaterial);
            Destroy(hueSquareMaterial);
            Destroy(hueSliderMaterial);
        }

        private void LoadBrushes()
        {
            brushes = new List<Brush>();
            var directory = Path.Combine(Application.dataPath, "Brushes");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var types = typeof(Brush).Assembly.GetTypes().Where(e => e.IsSubclassOf(typeof(Brush)) && !e.IsAbstract).ToArray();
            foreach (var type in types)
            {
                var filename = Path.Combine(directory, $"{type.Name}.json");
                if (File.Exists(filename))
                {
                    var json = File.ReadAllText(filename);
                    var file = (Brush)JsonUtility.FromJson(json, type);
                    brushes.Add(file);
                }
                else
                {
                    var instance = (Brush)Activator.CreateInstance(type);
                    brushes.Add(instance);
                }
            }

            SaveBrushes();
        }

        private void SaveBrushes()
        {
            var directory = Path.Combine(Application.dataPath, "Brushes");
            foreach (var brush in brushes)
            {
                var filename = Path.Combine(directory, $"{brush.GetType().Name}.json");
                var json = JsonUtility.ToJson(brush, true);
                File.WriteAllText(filename, json);
            }
        }

        private void FixedUpdate() { Collide(); }

        private void Update()
        {
            mouseRay = camera.ScreenPointToRay(Input.mousePosition);

            UpdateGrid();
            Paint();

            normalOffset = Input.GetKey(KeyCode.LeftShift) ? 0.0f : 1.0f;
        }

        private void Paint()
        {
            var wasPainting = painting;
            if (!painting)
            {
                if (didMouseHit && paintingTarget)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        paintPlane = new Plane(mouseHit.normal, mouseHit.collider.transform.position + mouseHit.normal * normalOffset);
                        painting = true;
                        paintingInputIndex = 0;
                    }
                }
            }

            var lastPaintPosition = paintPosition;
            if (painting)
            {
                if (paintPlane.Raycast(mouseRay, out var enter))
                {
                    paintPosition = mouseRay.GetPoint(enter);
                }

                if (Input.GetMouseButtonUp(paintingInputIndex))
                {
                    painting = false;
                }
            }

            if (!paintingTarget) return;

            var delta = paintPosition - lastPaintPosition;
            var max = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
            var vector = delta / max;

            var key = paintingTarget.ToKey(paintPosition);
            if (painting)
            {
                if (!wasPainting) CurrentBrush.PaintStart(this, PaintColor, key, paintingTarget);
                else
                {
                    for (var i = 0; i < max; i++)
                    {
                        var position = lastPaintPosition + vector * i;
                        CurrentBrush.PaintLoop(this, PaintColor, paintingTarget.ToKey(position), paintingTarget);
                    }
                }
            }
            else if (wasPainting)
            {
                CurrentBrush.PaintEnd(this, PaintColor, key, paintingTarget);
            }
        }

        private void UpdateGrid()
        {
            var show = false;
            var position = Vector3.zero;
            var normal = Vector3.zero;

            if (painting)
            {
                show = true;
                position = paintPosition;
                normal = paintPlane.normal;
            }
            else if (didMouseHit)
            {
                show = true;
                position = mouseHit.collider.transform.position;
                normal = mouseHit.normal;
            }

            grid.gameObject.SetActive(show);
            if (!show) return;
            grid.position = position + normal * normalOffset;
            grid.rotation = Quaternion.LookRotation(normal, Vector3.up);
        }

        private void Collide()
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                didMouseHit = false;
                return;
            }

            didMouseHit = Physics.Raycast(mouseRay, out mouseHit);
            if (didMouseHit)
            {
                paintingTarget = mouseHit.collider.GetComponentInParent<VoxelData>();
            }
        }

        private void OnGUI()
        {
            DrawBrushes();
            DrawColorPicker();
        }

        private void DrawColorPicker()
        {
            var r0 = new Rect(0.0f, Screen.height - 350.0f, 300.0f, 350.0f);
            
            drawRect(r0, new Color(0.0f, 0.0f, 0.0f, 0.2f), guiMaterial);

            var r1 = new Rect(r0.x + 20.0f, r0.y + 20.0f, r0.width - 40.0f, r0.width - 40.0f);
            drawRect(r1, Color.HSVToRGB(brushHue, 1.0f, 1.0f), hueSquareMaterial);
            var r2 = r1;
            r2.y = r1.y + r1.height + 20.0f;
            r2.height = r0.height - r1.height - 60.0f;
            drawRect(r2, Color.white, hueSliderMaterial);
            
            void drawRect(Rect rect, Color color, Material material)
            {
                material.SetPass(0);
            
                GL.Begin(GL.TRIANGLES);
                GL.PushMatrix();
                
                var uvs = new Vector2[]
                {
                    new(0.0f, 0.0f),
                    new(1.0f, 0.0f),
                    new(0.0f, 1.0f),
                    new(1.0f, 1.0f),
                };
                
                GL.Color(color);
                vertex(0);
                vertex(1);
                vertex(2);
                
                vertex(1);
                vertex(3);
                vertex(2);
                
                GL.PopMatrix();
                GL.End();

                void vertex(int i)
                {
                    var uv = uvs[i];
                    GL.TexCoord(uv);
                    GL.Vertex3(rect.x + rect.width * uv.x, rect.y + rect.height * uv.y, 0.0f);
                }
            }
        }

        private void DrawBrushes()
        {
            const float width = 48.0f;
            const float padding = 16.0f;

            var basis = new Rect(Screen.width - width - padding, padding, width, width);

            for (var i = 0; i < brushes.Count; i++)
            {
                var rect = basis;
                rect.y += rect.height * i;

                var brush = brushes[i];
                if (i == brushIndex)
                {
                    rect.x -= width / 4.0f;
                }

                if (GUI.Button(rect, brush.Icon))
                {
                    brushIndex = i;
                }
            }
        }
    }
}