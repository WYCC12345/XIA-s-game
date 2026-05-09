using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float zoomSpeed = 5f;
    public float rotateSpeed = 100f;

    public float defaultZoom = 10f;
    public float defaultRotationX = 0f;
    public float defaultRotationY = 20f;
    public Vector3 defaultTarget = Vector3.zero;

    private float currentZoom;
    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentTarget;

    void Start()
    {
        currentZoom = defaultZoom;
        currentRotationX = defaultRotationX;
        currentRotationY = defaultRotationY;
        currentTarget = defaultTarget;
        UpdateCameraTransform();
    }

    void Update()
    {
        // Use right mouse drag for camera rotation so left click can select planets cleanly.
        if (Input.GetMouseButton(1))
        {
            currentRotationX += Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            currentRotationY -= Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            currentRotationY = Mathf.Clamp(currentRotationY, 5f, 85f);
            UpdateCameraTransform();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            currentZoom -= scroll * zoomSpeed * 10f;
            currentZoom = Mathf.Clamp(currentZoom, 3f, 30f);
            UpdateCameraTransform();
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            Vector3 pan = (-right * Input.GetAxis("Mouse X") + -up * Input.GetAxis("Mouse Y")) * panSpeed * Time.deltaTime;
            currentTarget += pan;
            UpdateCameraTransform();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetView();
            PlanetInfoUI.Instance?.ResetInfo();
        }
    }

    public void FocusOn(Transform target, float zoom, float pitch)
    {
        if (target == null)
        {
            return;
        }

        currentTarget = target.position;
        currentZoom = Mathf.Clamp(zoom, 3f, 30f);
        currentRotationY = Mathf.Clamp(pitch, 5f, 85f);
        currentRotationX = target.eulerAngles.y;
        UpdateCameraTransform();
    }

    public void ResetView()
    {
        currentTarget = defaultTarget;
        currentZoom = defaultZoom;
        currentRotationX = defaultRotationX;
        currentRotationY = defaultRotationY;
        UpdateCameraTransform();
    }

    void UpdateCameraTransform()
    {
        float radX = currentRotationX * Mathf.Deg2Rad;
        float radY = currentRotationY * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(
            Mathf.Sin(radX) * Mathf.Cos(radY),
            Mathf.Sin(radY),
            Mathf.Cos(radX) * Mathf.Cos(radY)
        );
        transform.position = currentTarget + direction * currentZoom;
        transform.LookAt(currentTarget);
    }
}
