using System;

public static class DiplomacyEvent
{
    public static Action<I_Colony, I_Colony, float> OnRelationChangeRequestEvent;

    public static void TriggerRelationChange(I_Colony _source, I_Colony _target, float _amount)
    {
        OnRelationChangeRequestEvent?.Invoke(_source, _target, _amount);
    }
}
