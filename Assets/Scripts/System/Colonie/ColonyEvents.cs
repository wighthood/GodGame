using System;
using System.Collections.Generic;
using UnityEngine;

public static class ColonyEvents
{
    public static Action<I_ColonyAgent> OnRegisterAgentEvent;
    public static Action<I_ColonyAgent> OnUnregisterAgentEvent;

    public static Action<I_ColonyAgent, Vector3> OnUpdateAgentPositionEvent;

    public static Func<Vector3, float, List<I_ColonyAgent>> OnRequestNeighborsEvent;
}
