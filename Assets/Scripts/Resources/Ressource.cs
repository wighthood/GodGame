using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Ressource : MonoBehaviour
{

    [SerializeField] private RessourceType ressourceType = RessourceType.none;
    [SerializeField] private int ressourceRemaining = 1;

    [SerializeField]
    private Sprite[] Sprites;

    [SerializeField]
    private TileBase[] tiles;
   
    public static event Action<Ressource> OnEmptyRessource;
    public static event Func<Vector3, TileBase> GetTile;

    public bool isBeeingHarversted { get; set; }

    public RessourceType GetRessourceType()
    {
        return ressourceType;
    }

    public void OnHarvrestingRessource()
    {
        ressourceRemaining--;

        if (ressourceRemaining <= 0)
        {
            OnEmptyRessource?.Invoke(this);
        }
    }

    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if(!spriteRenderer) { return; }

        if (ressourceType == RessourceType.wood)
        {
            if (GetTile.Invoke(transform.position) == tiles[1])
            {
                spriteRenderer.sprite = Sprites[Random.Range(1, Sprites.Length)];
            }
            else
            {
                spriteRenderer.sprite = Sprites[0];
            }
        }
        else
        {
            spriteRenderer.sprite = Sprites[Random.Range(0, Sprites.Length)];
        }
    }
}
