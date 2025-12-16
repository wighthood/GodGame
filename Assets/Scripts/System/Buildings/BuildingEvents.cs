using System;
using UnityEngine;

public static class BuildingEvents
{
    public static Action<BuildType, Vector3, Colony> OnSpawnRequestedEvent;
    public static Func<Vector3, Building> OnGetNearestBuildingEvent;

    public static Action<BuildType, Colony> OnBuildingSpawnedEvent;
    public static Action<Building> OnBuildingDestroyedEvent;
    public static Action OnBuildingsChangedEvent;

    public static Func<Vector3, I_Colony, Vector3?> OnGetBuildPositionEvent;
    public static Func<BuildType, Colony, int> OnGetBuildingCountOfTypeEvent;
}
