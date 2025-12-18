using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class TileWithWeight
{
    public TileBase tile;
    [Range(0f, 1f)] public float weight;
}
