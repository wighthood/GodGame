using UnityEngine;

[CreateAssetMenu(fileName = "WalkAnim", menuName = "Animation/WalkAnim")]
internal class WalkAnim : SO_AnimBase
{
    private AgentActions agentActions;
    private SpriteRenderer spriteRenderer;

    public override bool CanPlay(GameObject _entity)
    {
        if (!agentActions)
        {
            agentActions = _entity.GetComponent<AgentActions>();
        }

        return agentActions.Velocity.magnitude > 0.1f;
    }

    public override void OnStartPlaying(GameObject _entity)
    {
        spriteRenderer = _entity.GetComponent<SpriteRenderer>();
    }

    public override void OnPlaying(GameObject _entity)
    {
        if (agentActions.Velocity.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (agentActions.Velocity.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }
}
