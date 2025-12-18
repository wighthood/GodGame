using UnityEngine;

[CreateAssetMenu(fileName = "HarvrestAnim", menuName = "Animation/HarvrestAnim")]
public class HarvrestAnimation : SO_AnimBase
{
    private AgentActions agentActions;

    public override bool CanPlay(GameObject _entity)
    {
        if (!agentActions)
        {
            agentActions = _entity.GetComponent<AgentActions>();
        }

        return agentActions.isHarvesting;
    }
}
