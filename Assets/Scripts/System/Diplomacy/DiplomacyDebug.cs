using UnityEngine;

public class DiplomacyDebug : MonoBehaviour
{
    public Colony Source;
    public Colony Target;
    public float Amount = 10f;

    [ContextMenu("Trigger Relation Change")]
    public void TriggerRelationChange()
    {
        if (Source == null || Target == null)
        {
            Debug.LogWarning("DiplomacyDebug: Source or Target is null.");
            return;
        }

        Debug.Log($"DiplomacyDebug: Triggering relation change between {Source.name} and {Target.name} with amount {Amount}.");
        DiplomacyEvent.TriggerRelationChange(Source, Target, Amount);

        // Check result
        ColonyRelation relation = Target.GetRelationData(Source.GetId());
        Debug.Log($"DiplomacyDebug: Resulting relation in Target (towards Source): Opinion={relation.Opinion}, State={relation.State}");
    }
}
