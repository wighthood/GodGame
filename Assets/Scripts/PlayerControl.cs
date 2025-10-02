using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float zoomSpeed = 12f;
    [SerializeField] private float maxZoom = 1f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private GameObject pauseMenu;
    
    private Vector2 _direction;
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
    
    private void Update()
    {
        transform.Translate(_direction * (speed * Time.deltaTime), Space.World);
    }
}
