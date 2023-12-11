using System.Text;
using UnityEngine;

namespace Code.User
{
    public class CameraController : MonoBehaviour
    {
        public float panSpeed = 20.0f;
        public float accelerationTime = 0.3f;
        public float mouseSensitivity = 3.0f;
        public float moveSpeedSensitivity = 0.2f;
        public float baseFov = 70.0f;
        public float zoomSensitivity = 0.2f;
        public float zoomSmoothing = 1.0f;

        private Vector3 velocity;
        private Vector3 force;
        private Vector2 rotation;

        private Vector3 moveInput;
        private float moveSpeedModifier = 1.0f;
        private float baseMoveSpeed = 0.0f;

        private float zoom;
        private float smoothedZoom;

        private new Camera camera;

        private void Awake() { camera = Camera.main; }

        private void OnEnable()
        {
            transform.position = new Vector3(-50.0f, 50.0f, -50.0f);
            transform.LookAt(Vector3.zero, Vector3.up);

            rotation = new Vector2(transform.eulerAngles.y, -transform.eulerAngles.x);
            zoom = 2.0f;
        }

        private void FixedUpdate()
        {
            Move();
            Iterate();
        }

        private void Move()
        {
            var panSpeed = this.panSpeed * moveSpeedModifier * Mathf.Pow(2.0f, baseMoveSpeed);
            force = (moveInput * panSpeed - velocity) * 2.0f / accelerationTime;
        }

        private void Update()
        {
            GetInputs();
            UpdateZoom();

            UpdateRotation();
            ConstrainCamera();
        }

        private void UpdateZoom()
        {
            smoothedZoom = Mathf.Lerp(zoom, smoothedZoom, Time.deltaTime / Mathf.Max(zoomSmoothing, Time.unscaledDeltaTime));
        }

        private void GetInputs()
        {
            moveInput = Vector3.zero;
            moveSpeedModifier = 1.0f;

            if (Input.GetMouseButton(1))
            {
                Cursor.lockState = CursorLockMode.Locked;

                if (Input.GetKey(KeyCode.W)) moveInput += transform.forward;
                if (Input.GetKey(KeyCode.S)) moveInput += -transform.forward;
                if (Input.GetKey(KeyCode.A)) moveInput += -transform.right;
                if (Input.GetKey(KeyCode.D)) moveInput += transform.right;
                if (Input.GetKey(KeyCode.Q)) moveInput += -Vector3.up;
                if (Input.GetKey(KeyCode.E)) moveInput += Vector3.up;

                if (Input.GetKey(KeyCode.LeftShift)) moveSpeedModifier *= 2.0f;
                if (Input.GetKey(KeyCode.LeftControl)) moveSpeedModifier *= 5.0f;
                if (Input.GetKey(KeyCode.LeftAlt)) moveSpeedModifier *= 0.5f;

                moveInput.Normalize();

                var mouseSensitivity = this.mouseSensitivity / Mathf.Pow(2.0f, zoom);
                rotation.x += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
                rotation.y += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

                baseMoveSpeed += Input.mouseScrollDelta.y * moveSpeedSensitivity;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                zoom += Input.mouseScrollDelta.y * zoomSensitivity;
            }
        }
    
        private void UpdateRotation()
        {
            rotation.x %= 360.0f;
            rotation.y = Mathf.Clamp(rotation.y, -90.0f, 90.0f);

            transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0.0f);
        }

        private void ConstrainCamera()
        {
            var position = transform.position + velocity * (Time.time - Time.fixedTime);
            
            camera.transform.position = position;
            camera.transform.rotation = transform.rotation;

            var planeSize = Mathf.Tan(baseFov * 0.5f * Mathf.Deg2Rad);
            var fieldOfView = 2.0f * Mathf.Atan(planeSize / Mathf.Pow(2.0f, zoom)) * Mathf.Rad2Deg;
            camera.fieldOfView = fieldOfView;
        }

        private void Iterate()
        {
            transform.position += velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;

            force = Vector3.zero;
        }

        private void OnGUI()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Camera Speed {panSpeed * Mathf.Pow(2.0f, baseMoveSpeed):N2} x{moveSpeedModifier}");
            sb.AppendLine($"Camera Zoom {Mathf.Pow(2.0f, zoom):N1}x");

            var padding = 10.0f;
            GUI.Label(new Rect(padding, padding, Screen.width - padding, Screen.height - padding), sb.ToString());
        }
    }
}