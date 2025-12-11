using System;
using UnityEngine;

public static class BuildingEvents
{
    public static Action<GameObject, Vector3, Quaternion, Colony, string> OnSpawnRequested;
    public static Func<Vector3, string, Building> GetNearestBuilding;

    public static Action<Building> OnBuildingSpawned;
    public static Action<Building> OnBuildingDestroyed;
    public static Action OnBuildingsChanged;

    public static Func<Vector3, IColony, Vector3?> OnGetBuildPosition;
}
