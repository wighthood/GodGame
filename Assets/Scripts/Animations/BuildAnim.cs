using UnityEngine;

[CreateAssetMenu(fileName = "BuildAnim", menuName = "Animation/BuildAnim")]
public class BuildAnim : SO_AnimBase
{
    private AgentActions agentActions;

    public override bool CanPlay(GameObject _entity)
    {
        if (!agentActions)
        {
            agentActions = _entity.GetComponent<AgentActions>();
        }

        return agentActions.isBuilding;
    }
}
