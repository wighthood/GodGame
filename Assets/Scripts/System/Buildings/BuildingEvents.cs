using System;
using UnityEngine;

public static class BuildingEvents
{
    public static Action<BuildType, Vector3, Colony> OnSpawnRequested;
    public static Func<Vector3, Building> GetNearestBuilding;

    public static Action<BuildType, Colony> OnBuildingSpawned;
    public static Action<Building> OnBuildingDestroyed;
    public static Action OnBuildingsChanged;

    public static Func<Vector3, IColony, Vector3?> OnGetBuildPosition;
}
