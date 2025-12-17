using UnityEngine;

public class DiplomacySystem : MonoBehaviour
{
    private void OnEnable()
    {
        DiplomacyEvent.OnRelationChangeRequestEvent += HandleRelationChangeRequest;
    }

    private void OnDisable()
    {
        DiplomacyEvent.OnRelationChangeRequestEvent -= HandleRelationChangeRequest;
    }

    private void HandleRelationChangeRequest(I_Colony _source, I_Colony _target, float _amount)
    {
        if (_source == null || _target == null)return;

        if (_source == _target)return; // Check au cas où
        
        ColonyRelation relation = _target.GetRelationData(_source.GetId());
        
        relation.Opinion = Mathf.Clamp(relation.Opinion + _amount, -100f, 100f); // Clamp entre -100 et 100 peut être modifié selon les besoins
        
        if (relation.Opinion <= -80f) // Seuil de guerre
        {
            relation.State = RelationState.War;
        }
        else if (relation.Opinion >= 80f) // Seuil d'alliance
        {
            relation.State = RelationState.Ally;
        }
        else // Neutre
        {
            relation.State = RelationState.Neutral;
        }

        // Debug.Log($"Diplomacy: Colony {_target.GetId()} -> {_source.GetId()} : Opinion={relation.Opinion}, State={relation.State}");
    }
}
