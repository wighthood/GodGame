using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapEditorScript : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Transform selectionBar;
    [SerializeField] private GameObject buttonPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Tile tile in tiles)
        {
            GameObject newButton = Instantiate(buttonPrefab,selectionBar);
            Image buttonImage = newButton.GetComponent<Image>();
            buttonImage.sprite = tile.sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
