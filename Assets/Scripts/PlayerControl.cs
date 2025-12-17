using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour, ISaveable
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float edgescrollSpeed = 0.1f;
    [SerializeField] private float zoomSpeed = 12f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 25f;
    [SerializeField] private float edge = 10f;
    [SerializeField] private float initialDezoom = 60f;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settings;
    [SerializeField] private Texture2D pressedMouseCursor;
    [SerializeField] private Texture2D normalMouseCursor;
    [SerializeField] private Vector2 cameraLimit;

    [SerializeField] private Animator powerBarAnimator;

    public WorldGeneration worldGeneration;

    private Vector2 _direction;

    float oldCameraZoom;

    private Vector3 _origin;
    private Vector3 _difference;
    private Camera _maincamera;
    private bool _isDragging;
    private bool _InitDezoom = false;

    private Vector3 GetMousePosition => _maincamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    private void Awake()
    {
        _maincamera = Camera.main;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (!_InitDezoom) return;
        if (ctx.started) _origin = GetMousePosition;
        _isDragging = ctx.started || ctx.performed;
    }

    private void LateUpdate()
    {
        if (!_isDragging) return;
        if (_InitDezoom) return;
        if (pauseMenu.activeSelf)
            return;

        _difference = GetMousePosition - transform.position;
        transform.position = _origin - _difference;
        CameraLimit();
    }

    private void onEdgeScroll()
    {
        if(pauseMenu.activeSelf || !_InitDezoom)
            return;
        if (Mouse.current.position.ReadValue().x > Screen.width - edge)
        {
            transform.position = transform.position + Vector3.right * edgescrollSpeed;
        }
        if (Mouse.current.position.ReadValue().x < edge)
        {
            transform.position = transform.position + Vector3.left * edgescrollSpeed;
        }
        if (Mouse.current.position.ReadValue().y > Screen.height - edge)
        {
            transform.position = transform.position + Vector3.up * edgescrollSpeed;
        }
        if (Mouse.current.position.ReadValue().y < edge)
        {
            transform.position = transform.position + Vector3.down * edgescrollSpeed;
        }
        return;
    }


    public void Move(InputAction.CallbackContext context)
    {
        if (!_InitDezoom) return;
        _direction = context.ReadValue<Vector2>();
    }

    public void Zoom(InputAction.CallbackContext context)
    {
        if (Camera.main != null && Time.timeScale > 0f && _InitDezoom)
        {
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + context.ReadValue<float>() * zoomSpeed, maxZoom, minZoom);
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
        else
        {
            Debug.LogError("No pause menu assigned");
        }
        settings.SetActive(false);
        MenusScript.Pause();
    }

    public void Click(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Cursor.SetCursor(pressedMouseCursor, Vector2.zero, CursorMode.Auto);
        }

        if (context.canceled)
        {
            Cursor.SetCursor(normalMouseCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    private void Start()
    {
        Cursor.SetCursor(normalMouseCursor, Vector2.zero, CursorMode.Auto);
        oldCameraZoom = Camera.main.orthographicSize;

        cameraLimit.x = (float)worldGeneration.MapWidth() / 2;
        cameraLimit.y = (float)worldGeneration.MapHeight() / 2;
    }


    private void Update()
    {
        transform.Translate(_direction * (speed * Time.deltaTime), Space.World);
        CameraLimit();
        onEdgeScroll();

        if (!_InitDezoom && SaveEvents.ShouldLoadOnStart == false)
        {
            if (Camera.main.orthographicSize < 10 )
            {
                Camera.main.orthographicSize += initialDezoom * Time.deltaTime;
            }
            else
            {
                _InitDezoom = true;
            }
        }
        
    }

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    private void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
    }

    public string GetSaveID()
    {
        return "PlayerControl";
    }

    public string CaptureState()
    {
        PlayerCameraSaveData data = new PlayerCameraSaveData();
        data.position = transform.position;
        if (Camera.main != null)
        {
            data.zoom = Camera.main.orthographicSize;
        }
        else
        {
            data.zoom = 5f; // verification default
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string _state)
    {
        if (string.IsNullOrEmpty(_state)) return;

        PlayerCameraSaveData data = JsonUtility.FromJson<PlayerCameraSaveData>(_state);
        if (data == null) return;

        transform.position = data.position;
        if (Camera.main != null)
        {
            Camera.main.orthographicSize = data.zoom;
        }
    }

    private void CameraLimit()
    {
        Vector3 pos = transform.position;

        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;

        if (pos.x > cameraLimit.x - halfWidth)
            pos.x = cameraLimit.x - halfWidth;

        if (pos.x < -cameraLimit.x + halfWidth)
            pos.x = -cameraLimit.x + halfWidth;

        if (pos.y > cameraLimit.y - halfHeight)
            pos.y = cameraLimit.y - halfHeight;

        if (pos.y < -cameraLimit.y + halfHeight)
            pos.y = -cameraLimit.y + halfHeight;

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