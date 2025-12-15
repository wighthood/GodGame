using System;
using UnityEngine;

public static class BuildingEvents
{
    public static Action<BuildType, Vector3, Colony> OnSpawnRequested {  get; set; }
    public static Func<Vector3, Building> GetNearestBuilding { get; set; }

    public static Action<BuildType, Colony> OnBuildingSpawned { get; set; }
    public static Action<Building> OnBuildingDestroyed { get; set; }
    public static Action OnBuildingsChanged { get; set; }

    public static Func<Vector3, IColony, Vector3?> OnGetBuildPosition { get; set; }
}
