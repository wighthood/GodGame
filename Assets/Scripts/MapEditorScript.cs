using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorScript : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileBase> tiles;
    [SerializeField] private Transform selectionBar;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private List<GameObject> Prefabs;
    [SerializeField] private LayerMask layermask;
    [SerializeField] private Vector3 treeOffset;
    [SerializeField] private Vector3 berryBushOffset;
    [SerializeField] private float tileOffset;

    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;
    public static event Func<Vector3, Cell> GetCell;

    private Camera _camera;
    private TileBase _selectedTile;
    private GameObject _selectedObject;
    private bool _isPainting = false;
    private Vector2 _cellposForRaycast;
    
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
            buttonImage.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
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

    private bool IsWater(TileBase _tempTile)
    {
        return _selectedTile == tiles.Find(o => o.name == "WaterRule") && !_tempTile.name.Contains("WaterRule")
                || _selectedTile != tiles.Find(o => o.name == "WaterRule") && _tempTile.name.Contains("WaterRule");
    }

    private bool IsObject(Vector2 _mousePosition)
    {
        return Physics2D.Raycast(_mousePosition, Camera.main.transform.forward, layermask);
    }

    private bool IsObject(Vector2 _start, out RaycastHit2D _result)
    {
        _result = Physics2D.Raycast(_start, Camera.main.transform.forward, layermask);
        return _result.collider;
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
            Cell cellToChange = GetCell?.Invoke(cellpos);
            if (IsWater(TempTile))
            {
                cellToChange.isWalkable = false;

                RaycastHit2D result;
                _cellposForRaycast.Set(cellpos.x + tileOffset, cellpos.y + tileOffset);
                if (IsObject(_cellposForRaycast, out result))
                {
                    Destroy(result.collider.gameObject);
                }
            }
            else
            {
                cellToChange.isWalkable = false;
            }
        }
        if(_selectedObject)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 cellpos = tilemap.CellToWorld(tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0)));
            if (tilemap.GetTile(tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0))) == tiles[2])
            {
                return;
            }
            if (!IsObject(mousePosition))
            {
                Ressource ressource = _selectedObject.GetComponent<Ressource>();
                if (ressource) 
                {
                    if (ressource.GetRessourceType() == RessourceType.wood)
                    {
                        AddNewRessource.Invoke(ressource.GetRessourceType(), cellpos + treeOffset);
                    }
                    else
                    {
                        AddNewRessource.Invoke(ressource.GetRessourceType(), cellpos + berryBushOffset);
                    }
                }
                else
                {
                    GameObject SpawnedObject = Instantiate(_selectedObject, cellpos, Quaternion.identity);
                    _isPainting = false;
                }
            }
        }
    }
}
