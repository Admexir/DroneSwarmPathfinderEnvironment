using UnityEngine;

namespace DroneSwarmPathfinder.Unity.Controls
{
    /// <summary>
    /// Simple fly movement script for the camera
    /// </summary>
    public class CameraMovement : MonoBehaviour
    {
        [Header("Movement Config")]
        public float moveSpeed = 30f;
        public float sprintMultiplier = 3f;

        [Header("Other Config")]
        public float lookSensitivity = 2f;

        private float rotX = 0f;
        private float rotY = 0f;

        private void Start()
        {
            rotX = transform.localRotation.eulerAngles.y;
            rotY = transform.localRotation.eulerAngles.x;
        }

        private void Update()
        {
            HandleRotation();
            HandleMovement();
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButton(1))
            {
                rotX += Input.GetAxis("Mouse X") * lookSensitivity;
                rotY -= Input.GetAxis("Mouse Y") * lookSensitivity;

                // limit rotation to prevent flipping (looking up or down too much)
                rotY = Mathf.Clamp(rotY, -90f, 90f);

                transform.localRotation = Quaternion.Euler(rotY, rotX, 0f);
            }
        }

        private void HandleMovement()
        {
            // Fast movement
            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed *= sprintMultiplier;
            }

            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");
            float upDownInput = 0f;
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) upDownInput += 1f;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl)) upDownInput -= 1f;

            Vector3 moveDirection = (transform.forward * vInput) + (transform.right * hInput) + (Vector3.up * upDownInput);
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }
}
