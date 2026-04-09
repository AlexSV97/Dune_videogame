using UnityEngine;

namespace DuneArrakisDominion.Visuals.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float zoomSpeed = 10f;
        
        [Header("Limits")]
        [SerializeField] private float minHeight = 10f;
        [SerializeField] private float maxHeight = 100f;
        [SerializeField] private float minRotation = 30f;
        [SerializeField] private float maxRotation = 80f;
        
        [Header("Bounds")]
        [SerializeField] private Vector2 minBounds = new(-500, -500);
        [SerializeField] private Vector2 maxBounds = new(500, 500);

        [Header("Input")]
        [SerializeField] private KeyCode panKey = KeyCode.MouseMiddle;
        [SerializeField] private KeyCode rotateKey = KeyCode.MouseRight;
        
        private Vector3 _targetPosition;
        private float _targetRotation;
        private float _targetHeight;
        private bool _isFollowingTarget;
        private Transform _followTarget;

        private void Start()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.eulerAngles.x;
            _targetHeight = transform.position.y;
        }

        private void Update()
        {
            HandleInput();
            UpdateCamera();
        }

        private void HandleInput()
        {
            if (_isFollowingTarget && _followTarget != null)
            {
                _targetPosition = _followTarget.position;
                return;
            }

            if (Input.GetKey(panKey) || Input.GetKey(rotateKey))
            {
                HandlePan();
                HandleRotation();
                HandleZoom();
            }
            else
            {
                HandleEdgeScrolling();
                HandleKeyboardPan();
            }

            ClampTargetValues();
        }

        private void HandlePan()
        {
            float horizontal = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float vertical = Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            _targetPosition += right * horizontal + forward * vertical;
        }

        private void HandleRotation()
        {
            float rotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.RotateAround(_targetPosition, Vector3.up, rotation);
        }

        private void HandleZoom()
        {
            float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            _targetHeight = Mathf.Clamp(_targetHeight - zoom, minHeight, maxHeight);
        }

        private void HandleEdgeScrolling()
        {
            Vector3 mousePos = Input.mousePosition;
            float edgeSize = 20f;

            Vector3 moveDir = Vector3.zero;

            if (mousePos.x < edgeSize)
                moveDir -= transform.right;
            else if (mousePos.x > Screen.width - edgeSize)
                moveDir += transform.right;

            if (mousePos.y < edgeSize)
                moveDir -= transform.forward;
            else if (mousePos.y > Screen.height - edgeSize)
                moveDir += transform.forward;

            if (moveDir != Vector3.zero)
            {
                moveDir.y = 0;
                moveDir.Normalize();
                _targetPosition += moveDir * panSpeed * Time.deltaTime;
            }
        }

        private void HandleKeyboardPan()
        {
            Vector3 moveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveDir += transform.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveDir -= transform.forward;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveDir -= transform.right;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveDir += transform.right;

            if (Input.GetKey(KeyCode.Q))
                _targetRotation = Mathf.Clamp(_targetRotation - rotationSpeed * Time.deltaTime, minRotation, maxRotation);
            if (Input.GetKey(KeyCode.E))
                _targetRotation = Mathf.Clamp(_targetRotation + rotationSpeed * Time.deltaTime, minRotation, maxRotation);

            if (Input.GetKey(KeyCode.R))
                _targetHeight = Mathf.Clamp(_targetHeight + zoomSpeed * Time.deltaTime, minHeight, maxHeight);
            if (Input.GetKey(KeyCode.F))
                _targetHeight = Mathf.Clamp(_targetHeight - zoomSpeed * Time.deltaTime, minHeight, maxHeight);

            moveDir.y = 0;
            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();
                _targetPosition += moveDir * panSpeed * Time.deltaTime;
            }
        }

        private void ClampTargetValues()
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, minBounds.x, maxBounds.x);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, minBounds.y, maxBounds.y);
            _targetHeight = Mathf.Clamp(_targetHeight, minHeight, maxHeight);
        }

        private void UpdateCamera()
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 5f);
            _targetRotation = Mathf.Lerp(_targetRotation, _targetHeight, Time.deltaTime * 5f);
            
            Vector3 direction = Quaternion.Euler(_targetRotation, transform.eulerAngles.y, 0) * Vector3.forward;
            transform.rotation = Quaternion.Euler(_targetRotation, transform.eulerAngles.y, 0);
        }

        public void SetTarget(Transform target)
        {
            _followTarget = target;
            _isFollowingTarget = target != null;
        }

        public void ClearTarget()
        {
            _followTarget = null;
            _isFollowingTarget = false;
        }

        public void FocusOnPosition(Vector3 position)
        {
            _targetPosition = position;
            _isFollowingTarget = false;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        public void ResetCamera()
        {
            _targetPosition = Vector3.zero;
            _targetHeight = 50f;
            _targetRotation = 55f;
            _isFollowingTarget = false;
            _followTarget = null;
            transform.rotation = Quaternion.Euler(_targetRotation, 0, 0);
        }
    }
}
