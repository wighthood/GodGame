using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] private List<GameObject> Weather;
    [SerializeField] private List<GameObject> Entity;
    [SerializeField] private LayerMask layermask;
    [SerializeField] private Vector3 treeOffset;
    [SerializeField] private Vector3 berryBushOffset;
    [SerializeField] private Vector3 stoneOffset;
    [SerializeField] private float tileOffset;

    [SerializeField] private List<TileBase> notWalkableSprites = new();

    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;
    public static event Func<Vector3, Cell> GetCell;
    public static event Action<Cell> OnGraphChange;

    private Camera _camera;
    private TileBase _selectedTile;
    private GameObject _selectedObject;
    private bool _isPainting = false;
    private Vector2 _cellposForRaycast;

    void Start()
    {
        _camera = Camera.main;
        Tilebutton();
        Ressourcebutton();
        Ressource.GetTile += GetTile;


    }
    public void Tilebutton ()
    {
        foreach (TileBase tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            if (tile is RuleTile T)
            {
                buttonImage.sprite = T.m_DefaultSprite;
            }
            button.onClick.AddListener((() =>
               SetSelector(null, tile)));
        }
    }

    public void Ressourcebutton()
    {
        foreach (GameObject prefab in Prefabs)
        {
            GameObject newButton = Instantiate(buttonPrefab, selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            button.onClick.AddListener((() =>
                SetSelector(prefab)));
        }
    }

    public void Weatherbutton()
    {

    }

    public void Entitybutton()
    {
        foreach (GameObject prefab in Entity)
        {
            GameObject newButton = Instantiate(buttonPrefab, selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            button.onClick.AddListener((() =>
                SetSelector(prefab)));
        }
    }

    private TileBase GetTile(Vector3 position)
    {
        Vector3Int Position = Vector3Int.FloorToInt(position);
        return tilemap.GetTile(Position);
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
            _isPainting = true;
        }
        else if (context.canceled)
        {
            _isPainting = false;
        }
    }

    private bool IsWater(TileBase _tile)
    {
        return notWalkableSprites.Contains(_tile);
    }

    public bool IsObject(Vector2 _mousePosition)
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

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (_selectedTile)
        {
            PaintTile(mousePosition);
        }
        else if (_selectedObject)
        {
            PaintObject(mousePosition);
        }
    }

    private void PaintTile(Vector2 _mousePosition)
    {
        Vector3Int cellpos = tilemap.WorldToCell(_mousePosition);
        TileBase tile = tilemap.GetTile(cellpos);
        tilemap.SetTile(cellpos, _selectedTile);

        Cell cellToChange = GetCell?.Invoke(cellpos);
        cellToChange.SetIsWalakble(!IsWater(_selectedTile));
        OnGraphChange?.Invoke(cellToChange);

        if (IsWater(_selectedTile))
        {
            RaycastHit2D result;
            _cellposForRaycast.Set(cellpos.x + tileOffset, cellpos.y + tileOffset);

            if (IsObject(_cellposForRaycast, out result))
            {
                Destroy(result.collider.gameObject);
            }
        }
    }

    private void PaintObject(Vector2 mousePosition)
    {
        Vector3 cellpos = tilemap.CellToWorld(tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0)));
        if (tilemap.GetTile(tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0))) == tiles[2] || IsObject(mousePosition))
        {
            return;
        }

        Ressource ressource = _selectedObject.GetComponent<Ressource>();

        if (ressource)
        {
            if (ressource.GetRessourceType() == RessourceType.wood)
            {
                AddNewRessource.Invoke(ressource.GetRessourceType(), cellpos + treeOffset);
            }
            else if (ressource.GetRessourceType() == RessourceType.food )
            {
                AddNewRessource.Invoke(ressource.GetRessourceType(), cellpos + berryBushOffset);
            }
            else
            {
                AddNewRessource.Invoke(ressource.GetRessourceType(), cellpos + stoneOffset);
            }
        }
        else
        {
            GameObject SpawnedObject = Instantiate(_selectedObject, cellpos, Quaternion.identity);
            _isPainting = false;
        }
    }

    private void OnDestroy()
    {
        Ressource.GetTile -= GetTile;
    }
}
