using System;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private ScriptableObject resource;

    private void Start()
    {
        if (resource is ScriptableObjectScript soResource)
        {
            Debug.Log($"Resource Name: {soResource.name}, Drop Value: {soResource.dropValue}");
        }
        else
        {
            Debug.LogError("Assigned resource is not of type ScriptableObjectScript");
        }
    }
}
