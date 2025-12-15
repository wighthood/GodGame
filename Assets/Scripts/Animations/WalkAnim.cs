using UnityEngine;

[CreateAssetMenu(fileName = "WalkAnim", menuName = "Animation/WalkAnim")]
class WalkAnim : SO_AnimBase
{
    AgentActions agentActions;
    SpriteRenderer spriteRenderer;

    public override bool CanPlay(GameObject _entity)
    {
        if(!agentActions)
        {
            agentActions = _entity.GetComponent<AgentActions>();
        }

        return agentActions.Velocity.magnitude > 0.0f;
    }

    public override void OnStartPlaying(GameObject _entity)
    {
        spriteRenderer = _entity.GetComponent<SpriteRenderer>();
    }

    public override void OnPlaying(GameObject _entity)
    {
        if(agentActions.Velocity.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if(agentActions.Velocity.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }
}
