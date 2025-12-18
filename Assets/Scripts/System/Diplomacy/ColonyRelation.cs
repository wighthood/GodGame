using System;

[Serializable]
public enum RelationState
{
    Neutral,
    War,
    Ally,
}

[Serializable]
public class ColonyRelation
{
    public float Opinion;
    public RelationState State;

    public ColonyRelation()
    {
        Opinion = 0f;
        State = RelationState.Neutral;
    }
}
