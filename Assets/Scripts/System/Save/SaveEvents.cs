using System;

public static class SaveEvents
{
    public static Action<ISaveable> OnRegisterSaveableEvent;
    public static Action<ISaveable> OnUnregisterSaveableEvent;
    public static Action OnGraphRefreshRequestedEvent;
    public static Action OnNewGameStartEvent;
    
    public static Action OnRequestSaveEvent;
    public static Action OnSaveCompletedEvent;

    public static bool ShouldLoadOnStart = false;
}
