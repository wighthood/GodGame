using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour, ISaveable
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float edgeScrollSpeed = 20f;
    [SerializeField] private float edgeSize = 10f;

    [SerializeField] private Animator powerBarAnimator;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 12f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 25f;
    [SerializeField] private float initialDezoomSpeed = 60f;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settings;
    [SerializeField] private Texture2D pressedCursor;
    [SerializeField] private Texture2D normalCursor;

    [Header("World")]
    [SerializeField] private Vector2 cameraLimit;
    public WorldGeneration worldGeneration;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool initZoomDone;
    private bool isDragging;
    private Vector2 moveInput;

    private Vector3 MouseWorldPos
    {
        get { return cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()); }
    }

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
        cameraLimit = new Vector2(
            worldGeneration.MapWidth() / 2f,
            worldGeneration.MapHeight() / 2f
        );

        StartCoroutine(InitZoom());
    }

    private void Update()
    {
        if (!initZoomDone)
        {
            return;
        }

        MoveCamera();
        EdgeScroll();
        CameraLimit();
    }

    private void LateUpdate()
    {
        if (!isDragging || !initZoomDone || pauseMenu.activeSelf) return;

        Vector3 delta = MouseWorldPos - dragOrigin;
        transform.position -= delta;
        CameraLimit();
    }

    public string GetSaveID()
    {
        return "PlayerControl";
    }

    public string CaptureState()
    {
        return JsonUtility.ToJson(new PlayerCameraSaveData
        {
            position = transform.position,
            zoom = cam.orthographicSize,
        });
    }

    public void RestoreState(string state)
    {
        if (string.IsNullOrEmpty(state)) return;

        PlayerCameraSaveData data = JsonUtility.FromJson<PlayerCameraSaveData>(state);
        transform.position = data.position;
        cam.orthographicSize = data.zoom;
        initZoomDone = true;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        if (!initZoomDone) return;
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        if (Camera.main != null && Time.timeScale > 0f && initZoomDone)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + context.ReadValue<float>() * zoomSpeed, maxZoom, minZoom);
        }
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (!initZoomDone) return;

        if (ctx.started)
            dragOrigin = MouseWorldPos;

        isDragging = ctx.started || ctx.performed;
    }

    public void Click(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            Cursor.SetCursor(pressedCursor, Vector2.zero, CursorMode.Auto);
        else if (ctx.canceled)
            Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        pauseMenu.SetActive(!pauseMenu.activeSelf);
        settings.SetActive(false);
        MenusScript.Pause();
    }

    private void MoveCamera()
    {
        transform.Translate(moveInput * speed * Time.deltaTime, Space.World);
    }

    private void EdgeScroll()
    {
        if (pauseMenu.activeSelf) return;

        Vector3 dir = Vector3.zero;
        Vector2 mouse = Mouse.current.position.ReadValue();

        if (mouse.x > Screen.width - edgeSize) dir += Vector3.right;
        if (mouse.x < edgeSize) dir += Vector3.left;
        if (mouse.y > Screen.height - edgeSize) dir += Vector3.up;
        if (mouse.y < edgeSize) dir += Vector3.down;

        transform.position += dir * edgeScrollSpeed * Time.deltaTime;
    }

    private IEnumerator InitZoom()
    {
        while (cam.orthographicSize < 10)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + initialDezoomSpeed, maxZoom, minZoom);
            CameraLimit();
            yield return null;
        }
        initZoomDone = true;
    }

    private void CameraLimit()
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -cameraLimit.x + halfW, cameraLimit.x - halfW);
        pos.y = Mathf.Clamp(pos.y, -cameraLimit.y + halfH, cameraLimit.y - halfH);
        transform.position = pos;
    }

    public void ClosePowerBar()
    {
        powerBarAnimator.SetBool("IsClosing", true);
        powerBarAnimator.SetBool("IsOpen", false);
    }

    public void OpenPowerBar()
    {
        powerBarAnimator.SetBool("IsClosing", false);
        powerBarAnimator.SetBool("IsOpen", true);
    }
}
