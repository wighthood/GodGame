using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorScript : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] tiles;
    [SerializeField] private Transform selectionBar;
    [SerializeField] private GameObject buttonPrefab;
    
    private TileBase _selectedTile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
               _selectedTile = tile));
        }
    }

    public void Paint(InputAction.CallbackContext context)
    {
        if (_selectedTile == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3Int cellpos =  tilemap.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0));
        tilemap.SetTile(cellpos, _selectedTile);
    }
}
