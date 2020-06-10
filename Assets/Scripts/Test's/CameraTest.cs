using UnityEngine;

namespace Assets.Scripts
{
    [AddComponentMenu("Mythirial Test/Camera Test")]
    [RequireComponent(typeof(Camera))]
    public class CameraTest : MonoBehaviour
    {
        [SerializeField]
        private const float Y_ANGLE_MIN = 0.0f;
        [SerializeField]
        private const float Y_ANGLE_MAX = 50.0f;

        Camera cam;

        public bool lockCursor;
        public Transform lookAt;
        public float dstFromTarget = 5f;
        public float dstFactor = .8f;

        public float rotationSmoothTime = .12f;
		public float depthSmoothTime = 1;
        Vector3 rotationSmoothVelocity;
        Vector3 currentRotation;

        public float dstMax = 5f;
        Vector3 dollyDir;

        private float currentX = 0.0f;
        private float currentY = 45.0f;
        private float sensitivityX = 4.0f;
        private float sensitivityY = 1.0f;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            dollyDir = transform.localPosition.normalized;
            dstFromTarget = transform.localPosition.magnitude;

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void LateUpdate()
        {
            CamPan();
        }

        /// <summary>
        /// Panning the camera
        /// </summary>
        void CamPan()
        {
            CollisionDetect();
            currentX += Input.GetAxis("Look Horizontal") * sensitivityX ;
            currentY += Input.GetAxis("Look Vertical") * sensitivityY;
            currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
       
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(currentY, currentX), ref rotationSmoothVelocity, rotationSmoothTime);
            transform.eulerAngles = currentRotation;

            transform.position = lookAt.position - transform.forward * Mathf.Clamp((dstFromTarget * dstFactor), 0, dstMax);
            //transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * dstFromTarget, Time.deltaTime * rotationSmoothTime);
        }

        /// <summary>
        /// Detecting collision for the camera to protect it from clipping through walls
        /// </summary>
        void CollisionDetect()
        {
            Vector3[] camCorners = new[] {new Vector3(0, 0, cam.nearClipPlane), new Vector3(0, 1, cam.nearClipPlane), new Vector3(1, 0, cam.nearClipPlane), new Vector3(1, 1, cam.nearClipPlane)};
            float h_Distance = dstMax;
            RaycastHit hitLine;
            for (int i = 0; i < camCorners.Length; i++) {
                var boxPoint = -(lookAt.position - cam.ViewportToWorldPoint(camCorners[i])) * h_Distance;

                Debug.DrawRay(cam.ViewportToWorldPoint(camCorners[i]), Vector3.up);
                Debug.DrawRay(lookAt.position, boxPoint, Color.green);

                if (Physics.Raycast(lookAt.position, boxPoint, out hitLine)) {
                    h_Distance = (hitLine.distance < h_Distance) ? hitLine.distance : h_Distance;
                    Debug.DrawRay(hitLine.point, Vector3.up);
                }
            }

            if (h_Distance < dstMax)
                dstFromTarget = h_Distance;
            else
                dstFromTarget = Mathf.Lerp(dstFromTarget, dstMax, Time.deltaTime * depthSmoothTime);
        }
    }
}