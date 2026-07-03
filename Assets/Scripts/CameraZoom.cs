using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance;

    [Header("Cinemachine Integration (Optional)")]
    [Tooltip("Drag your Cinemachine Virtual Camera here. If empty, it will search in the scene automatically.")]
    [SerializeField] private CinemachineCamera virtualCamera;

    [Header("Zoom Limits")]
    [Tooltip("Minimum orthographic size or Field of View")]
    [SerializeField] private float minZoom = 2f;
    [Tooltip("Maximum orthographic size or Field of View")]
    [SerializeField] private float maxZoom = 15f;

    [Header("Zoom Sensitivity")]
    [SerializeField] private float scrollSensitivity = 5f;
    [SerializeField] private float keyboardSensitivity = 10f;
    [SerializeField] private float smoothSpeed = 10f;

    private Camera cam;
    private float targetZoom;
    private bool isOrthographic;

    private float saveCooldown = 1f;
    private float lastScrollTime;
    private bool needsSaving = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        cam = GetComponent<Camera>();

        // Auto-find Cinemachine Virtual Camera if not explicitly assigned in Inspector
        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        // Determine if camera is orthographic or perspective from the Unity Main Camera
        if (cam != null)
        {
            isOrthographic = cam.orthographic;
        }
        else
        {
            isOrthographic = true; // Fallback for 2D
        }

        if (virtualCamera != null)
        {
            Debug.Log("[CameraZoom] Cinemachine Virtual Camera detected and linked.");
        }
        else if (cam != null)
        {
            Debug.Log("[CameraZoom] Standard Unity Camera linked.");
        }
        else
        {
            Debug.LogWarning("[CameraZoom] No Camera or Cinemachine camera found! Defaulting to Orthographic.");
        }

        // Validate limits for perspective mode
        if (!isOrthographic)
        {
            minZoom = Mathf.Max(10f, minZoom);
            maxZoom = Mathf.Min(120f, maxZoom);
        }
    }

    private void Start()
    {
        LoadZoomFromDatabase();
    }

    private void Update()
    {
        HandleScrollInput();
        HandleKeyboardInput();
        ApplySmoothing();
        HandleSaveCooldown();
    }

    /// <summary>
    /// Loads the camera zoom value from the SQLite settings database.
    /// </summary>
    public void LoadZoomFromDatabase()
    {
        if (SettingManager.Instance != null && SettingManager.Instance.CurrentSettings != null)
        {
            float savedZoom = SettingManager.Instance.CurrentSettings.CameraZoom;
            
            // Validate the saved zoom value fits inside our limits
            targetZoom = Mathf.Clamp(savedZoom, minZoom, maxZoom);
            
            ApplyZoomImmediate(targetZoom);
            Debug.Log($"[CameraZoom] Loaded zoom from DB: {targetZoom}");
        }
        else
        {
            // Default fallback based on current values
            if (virtualCamera != null)
            {
                targetZoom = isOrthographic ? virtualCamera.Lens.OrthographicSize : virtualCamera.Lens.FieldOfView;
            }
            else if (cam != null)
            {
                targetZoom = isOrthographic ? cam.orthographicSize : cam.fieldOfView;
            }
        }
    }

    private void ApplyZoomImmediate(float zoomValue)
    {
        if (virtualCamera != null)
        {
            if (isOrthographic)
            {
                virtualCamera.Lens.OrthographicSize = zoomValue;
            }
            else
            {
                virtualCamera.Lens.FieldOfView = zoomValue;
            }
        }
        else if (cam != null)
        {
            if (isOrthographic)
            {
                cam.orthographicSize = zoomValue;
            }
            else
            {
                cam.fieldOfView = zoomValue;
            }
        }
    }

    private void HandleScrollInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Scrolling up zooms in (reduces size/FOV), scrolling down zooms out (increases size/FOV)
            targetZoom -= scrollInput * scrollSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            
            lastScrollTime = Time.time;
            needsSaving = true;
        }
    }

    private void HandleKeyboardInput()
    {
        float zoomDelta = 0f;

        // Zoom In with + or PageUp
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.PageUp))
        {
            zoomDelta -= keyboardSensitivity * Time.deltaTime;
        }
        // Zoom Out with - or PageDown
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.PageDown))
        {
            zoomDelta += keyboardSensitivity * Time.deltaTime;
        }

        if (Mathf.Abs(zoomDelta) > 0.01f)
        {
            targetZoom += zoomDelta;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            
            lastScrollTime = Time.time;
            needsSaving = true;
        }
    }

    private void ApplySmoothing()
    {
        if (virtualCamera != null)
        {
            if (isOrthographic)
            {
                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, targetZoom, Time.deltaTime * smoothSpeed);
            }
            else
            {
                virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, targetZoom, Time.deltaTime * smoothSpeed);
            }
        }
        else if (cam != null)
        {
            if (isOrthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothSpeed);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * smoothSpeed);
            }
        }
    }

    /// <summary>
    /// Save settings only when zoom changes have stopped for saveCooldown seconds
    /// to prevent heavy disk/database writes on every frame of scroll.
    /// </summary>
    private void HandleSaveCooldown()
    {
        if (needsSaving && (Time.time - lastScrollTime > saveCooldown))
        {
            SaveZoomToDatabase();
        }
    }

    /// <summary>
    /// Saves the current target zoom value back into the SQLite database.
    /// </summary>
    public void SaveZoomToDatabase()
    {
        needsSaving = false;
        
        if (SettingManager.Instance != null)
        {
            var settings = SettingManager.Instance.CurrentSettings;
            if (settings != null)
            {
                settings.CameraZoom = targetZoom;
                DatabaseManager.GetOrCreateInstance().SaveSettings(settings);
                Debug.Log($"[CameraZoom] Automatically saved zoom {targetZoom:0.00} to SQLite Database.");
            }
        }
    }

    /// <summary>
    /// Allows setting the zoom level manually (e.g. from a UI Slider)
    /// </summary>
    public void SetZoom(float value)
    {
        targetZoom = Mathf.Clamp(value, minZoom, maxZoom);
        lastScrollTime = Time.time;
        needsSaving = true;
    }

    private void OnDisable()
    {
        if (needsSaving)
        {
            SaveZoomToDatabase();
        }
    }
}
