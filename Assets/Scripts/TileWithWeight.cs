using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileWithWeight
{
    public TileBase tile;
    [Range(0f,1f)]public float weight;
}
