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

    [Header("Dynamic Airborne Zoom")]
    [SerializeField] private bool enableDynamicZoom = true;
    [SerializeField] private float velocityThreshold = 8f;
    [SerializeField] private float zoomPerVelocityUnit = 0.15f;
    [SerializeField] private float maxDynamicZoomOffset = 5f;

    private Camera cam;
    private float targetZoom;
    private bool isOrthographic;

    private float saveCooldown = 1f;
    private float lastScrollTime;
    private bool needsSaving = false;
    private Rigidbody2D playerRigidbody;

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
        // Safety check to prevent Unity Inspector serialization from overriding defaults to 0/false
        if (velocityThreshold <= 0.1f) velocityThreshold = 8f;
        if (zoomPerVelocityUnit <= 0.001f) zoomPerVelocityUnit = 0.15f;
        if (maxDynamicZoomOffset <= 0.1f) maxDynamicZoomOffset = 5f;
        enableDynamicZoom = true;

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
            CinemachineCamera currentCam = (CameraManager.ActiveCamera != null) ? CameraManager.ActiveCamera : virtualCamera;
            if (currentCam == null)
            {
                currentCam = FindFirstObjectByType<CinemachineCamera>();
                virtualCamera = currentCam;
            }

            if (currentCam != null)
            {
                targetZoom = isOrthographic ? currentCam.Lens.OrthographicSize : currentCam.Lens.FieldOfView;
            }
            else if (cam != null)
            {
                targetZoom = isOrthographic ? cam.orthographicSize : cam.fieldOfView;
            }
        }
    }

    private void ApplyZoomImmediate(float zoomValue)
    {
        CinemachineCamera currentCam = (CameraManager.ActiveCamera != null) ? CameraManager.ActiveCamera : virtualCamera;
        if (currentCam == null)
        {
            currentCam = FindFirstObjectByType<CinemachineCamera>();
            virtualCamera = currentCam;
        }

        if (currentCam != null)
        {
            if (isOrthographic)
            {
                currentCam.Lens.OrthographicSize = zoomValue;
            }
            else
            {
                currentCam.Lens.FieldOfView = zoomValue;
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
        float dynamicTargetZoom = targetZoom + GetDynamicZoomOffset();

        CinemachineCamera currentCam = (CameraManager.ActiveCamera != null) ? CameraManager.ActiveCamera : virtualCamera;
        if (currentCam == null)
        {
            currentCam = FindFirstObjectByType<CinemachineCamera>();
            virtualCamera = currentCam;
        }

        if (currentCam != null)
        {
            if (isOrthographic)
            {
                currentCam.Lens.OrthographicSize = Mathf.Lerp(currentCam.Lens.OrthographicSize, dynamicTargetZoom, Time.deltaTime * smoothSpeed);
            }
            else
            {
                currentCam.Lens.FieldOfView = Mathf.Lerp(currentCam.Lens.FieldOfView, dynamicTargetZoom, Time.deltaTime * smoothSpeed);
            }
        }
        else if (cam != null)
        {
            if (isOrthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, dynamicTargetZoom, Time.deltaTime * smoothSpeed);
            }
            else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, dynamicTargetZoom, Time.deltaTime * smoothSpeed);
            }
        }
    }

    private float GetDynamicZoomOffset()
    {
        if (!enableDynamicZoom) return 0f;

        if (playerRigidbody == null)
        {
            playerJ player = FindFirstObjectByType<playerJ>();
            if (player != null)
            {
                playerRigidbody = player.GetComponent<Rigidbody2D>();
                Debug.Log($"[CameraZoom] Successfully linked player: {player.name}, Rigidbody2D: {playerRigidbody != null}");
            }
        }

        if (playerRigidbody != null)
        {
            float verticalVelocity = Mathf.Abs(playerRigidbody.linearVelocity.y);
            if (verticalVelocity > velocityThreshold)
            {
                float excess = verticalVelocity - velocityThreshold;
                float offset = Mathf.Min(excess * zoomPerVelocityUnit, maxDynamicZoomOffset);
                if (Time.frameCount % 15 == 0)
                {
                    Debug.Log($"[CameraZoom] Zooming out! Velocity: {verticalVelocity:0.00}, Zoom Offset: +{offset:0.00}");
                }
                return offset;
            }
        }

        return 0f;
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
