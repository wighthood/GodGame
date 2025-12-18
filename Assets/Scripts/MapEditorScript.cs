using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorScript : MonoBehaviour
{
    [Header("map edition")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<TileBase> tiles;

    [Header("Tool Bar")]
    [SerializeField] private Transform selectionBar;
    [SerializeField] private GameObject buttonPrefab;

    [Header("object to spawn")]
    [SerializeField] private List<GameObject> Prefabs;
    [SerializeField] private List<GameObject> Entity;
    [SerializeField] private List<WeatherState> weatherState;

    [Header("objects offset")]
    [SerializeField] private LayerMask layermask;
    [SerializeField] private Vector3 treeOffset;
    [SerializeField] private Vector3 berryBushOffset;
    [SerializeField] private Vector3 stoneOffset;
    [SerializeField] private float tileOffset;

    [Header("meteo edition")]
    [SerializeField] private MeteoManager meteoManager;
    [SerializeField] private Sprite[] WeatherImages;

    [Header("setup not walkable tiles")]
    [SerializeField] private List<TileBase> notWalkableSprites = new List<TileBase>();

    [Header("setup the different references")]
    [SerializeField] private GameObject tileBar;
    [SerializeField] private GameObject ressourceBar;
    [SerializeField] private GameObject weatherBar;
    [SerializeField] private GameObject entityBar;
    [SerializeField] private Transform AgentParent;

    private Camera _camera;
    private Vector2 _cellposForRaycast;
    private bool _isPainting;
    private GameObject _selectedObject;
    private TileBase _selectedTile;


    private void Start()
    {
        Ressource.GetTile += GetTile;

        _camera = Camera.main;
        Tilebutton();
        Ressourcebutton();

        foreach (TileBase tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab, tileBar.transform);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            if (tile is RuleTile T)
            {
                buttonImage.sprite = T.m_DefaultSprite;
            }
            button.onClick.AddListener(() =>
                SetSelector(null, tile));
        }
        foreach (GameObject prefab in Prefabs)
        {
            GameObject newButton = Instantiate(buttonPrefab, ressourceBar.transform);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            button.onClick.AddListener(() =>
                SetSelector(prefab));
        }
        // entity and WeatherState are inverted it still works
        foreach (GameObject prefab in Entity)
        {
            GameObject newButton = Instantiate(buttonPrefab, weatherBar.transform);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            button.onClick.AddListener(() =>
                SetSelector(prefab));
        }
        int i = 0;
        foreach (WeatherState state in weatherState)
        {
            GameObject newButton = Instantiate(buttonPrefab, entityBar.transform);
            Image buttonImage = newButton.GetComponent<Image>();
            Button button = newButton.GetComponent<Button>();
            buttonImage.sprite = WeatherImages[i];
            button.onClick.AddListener(() => SetMeteo(state));
            i++;
        }
    }

    private void Update()
    {
        if (_selectedTile == null && _selectedObject == null) return;
        if (_camera == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!_isPainting) return;

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

    private void OnDestroy()
    {
        Ressource.GetTile -= GetTile;
    }

    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;
    public static event Func<Vector3, Cell> GetCell;
    public static event Action<Cell> OnGraphChange;

    public void HideButtons()
    {
        tileBar.SetActive(false);
        entityBar.SetActive(false);
        ressourceBar.SetActive(false);
        weatherBar.SetActive(false);
    }

    public void Tilebutton()
    {
        HideButtons();
        tileBar.SetActive(true);
    }

    public void Ressourcebutton()
    {
        HideButtons();
        ressourceBar.SetActive(true);
    }

    public void Weatherbutton()
    {
        HideButtons();
        weatherBar.SetActive(true);
    }

    public void Entitybutton()
    {
        HideButtons();
        entityBar.SetActive(true);
    }

    private void SetMeteo(WeatherState state)
    {
        meteoManager.MeteoChange(state);
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

    // REFACTO WHY IS HERE ?
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
            else if (ressource.GetRessourceType() == RessourceType.food)
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
            GameObject SpawnedObject = Instantiate(_selectedObject, cellpos, Quaternion.identity, AgentParent);
            _isPainting = false;
        }
    }
}
