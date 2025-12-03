using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.WSA;
using NavMeshSurface = NavMeshPlus.Components.NavMeshSurface;

public class MapEditorScript : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileBase> tiles;
    [SerializeField] private Transform selectionBar;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private List<GameObject> Prefabs;
    
    private Camera _camera;
    private TileBase _selectedTile;
    private GameObject _selectedObject;
    private bool _isPainting = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
        foreach (TileBase tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab,selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            if (tile is RuleTile T)
            {
                buttonImage.sprite = T.m_DefaultSprite;
            }
            button.onClick.AddListener((() => 
               SetSelector(null,tile)));
        }

        foreach (GameObject prefab in Prefabs)
        {
            GameObject newButton = Instantiate(buttonPrefab, selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = prefab.GetComponent<Sprite>();
            button.onClick.AddListener ((() => 
                SetSelector(prefab)));
        }
    }

    private void SetSelector(GameObject test = null, TileBase test2 = null)
    {
        _selectedTile = test2;
        _selectedObject = test;
    }

    public void DeselectTile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _selectedTile = null;
            _selectedObject = null;
        }
    }

    public void Paint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isPainting =  true;
        }
        else if (context.canceled)
        {
            _isPainting = false;
        }
    }

    private void Update()
    {
        if (_selectedTile == null && _selectedObject == null || _camera == null
                                  || EventSystem.current.IsPointerOverGameObject() || !_isPainting) return;

        if (_selectedTile)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int cellpos = tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0));
            TileBase TempTile = tilemap.GetTile(cellpos);
            tilemap.SetTile(cellpos, _selectedTile);
            if (_selectedTile == tiles.Find(o => o.name == "WaterRule") && !TempTile.name.Contains("WaterRule")
                || _selectedTile != tiles.Find(o => o.name == "WaterRule") && TempTile.name.Contains("WaterRule"))
            {
                navMesh.BuildNavMesh();
            }
        }
        if(_selectedObject)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 cellpos = tilemap.CellToWorld(tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0)));
            GameObject SpawnedObject = Instantiate(_selectedObject, cellpos, Quaternion.identity);
        }
    }
}
