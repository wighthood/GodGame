using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    private Vector2 _direction;
    public void Move(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector2>();
    }
    
    public void Zoom(InputAction.CallbackContext context)
    {
        
    }
    
    private void Update()
    {
        transform.Translate(_direction * (speed * Time.deltaTime), Space.World);
    }
}
