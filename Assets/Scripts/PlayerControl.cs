using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float zoomSpeed = 12f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 25f;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Texture2D pressedMouseCursor;
    [SerializeField] private Texture2D normalMouseCursor;
    [SerializeField] private Vector2 cameraLimit;

    public WorldGeneration worldGeneration;
    
    private Vector2 _direction;

    float oldCameraZoom;

    public void Move(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector2>();
    }
    
    public void Zoom(InputAction.CallbackContext context)
    {
        if (Camera.main != null && Time.timeScale > 0f)
        {
           Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + context.ReadValue<float>()*zoomSpeed, maxZoom, minZoom);
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
}