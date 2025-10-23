using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float edgePanSpeed = 15f;
    [SerializeField] private float edgePanBorder = 10f; // Pixels from screen edge
    [SerializeField] private bool enableEdgePanning = true;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float scrollSensitivity = 2f;

    [Header("Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minZ = -50f;
    [SerializeField] private float maxZ = 50f;

    [Header("Mouse Drag")]
    [SerializeField] private bool enableMouseDrag = true;
    [SerializeField] private MouseButton dragButton = MouseButton.Middle;

    private Camera cam;
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private float targetZoom;

    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }

    void Start()
    {
        cam = GetComponent<Camera>();

        // Initialize zoom based on current camera setup
        if (cam.orthographic)
        {
            targetZoom = cam.orthographicSize;
        }
        else
        {
            targetZoom = transform.position.y;
        }
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleEdgePanning();
        HandleMouseDrag();
        HandleZoom();
    }

    void HandleKeyboardMovement()
    {
        Vector3 move = Vector3.zero;

        // WASD or Arrow Keys
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            move += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            move += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            move += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            move += Vector3.right;

        // Apply movement
        if (move != Vector3.zero)
        {
            move = move.normalized * panSpeed * Time.deltaTime;
            MoveCamera(move);
        }
    }

    void HandleEdgePanning()
    {
        if (!enableEdgePanning) return;

        Vector3 move = Vector3.zero;
        Vector2 mousePos = Input.mousePosition;

        // Check screen edges
        if (mousePos.x >= Screen.width - edgePanBorder)
            move += Vector3.right;
        if (mousePos.x <= edgePanBorder)
            move += Vector3.left;
        if (mousePos.y >= Screen.height - edgePanBorder)
            move += Vector3.forward;
        if (mousePos.y <= edgePanBorder)
            move += Vector3.back;

        // Apply edge panning
        if (move != Vector3.zero)
        {
            move = move.normalized * edgePanSpeed * Time.deltaTime;
            MoveCamera(move);
        }
    }

    void HandleMouseDrag()
    {
        if (!enableMouseDrag) return;

        int button = (int)dragButton;

        // Start dragging
        if (Input.GetMouseButtonDown(button))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        // Stop dragging
        if (Input.GetMouseButtonUp(button))
        {
            isDragging = false;
        }

        // Perform drag
        if (isDragging && Input.GetMouseButton(button))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Convert screen space to world space movement
            Vector3 move = new Vector3(-delta.x, 0, -delta.y);
            move = move.normalized * (delta.magnitude * 0.01f) * panSpeed * Time.deltaTime;

            MoveCamera(move);

            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            targetZoom -= scroll * scrollSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly interpolate zoom
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        }
        else
        {
            // For perspective camera, adjust Y position
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, targetZoom, Time.deltaTime * zoomSpeed);
            transform.position = pos;
        }
    }

    void MoveCamera(Vector3 move)
    {
        Vector3 newPos = transform.position + move;

        // Apply bounds if enabled
        if (useBounds)
        {
            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        }

        transform.position = newPos;
    }

    // Public methods for external control
    public void SetZoom(float zoom)
    {
        targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void FocusOnPosition(Vector3 worldPosition, float duration = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(FocusCoroutine(worldPosition, duration));
    }

    private System.Collections.IEnumerator FocusCoroutine(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(target.x, transform.position.y, target.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
    }
}