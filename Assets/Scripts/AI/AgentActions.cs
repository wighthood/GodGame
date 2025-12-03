using UnityEngine;

public class AgentActions : MonoBehaviour
{
    private float moveFactor = 0.01f;

    [HideInInspector]
    public BlackBoard agentBlackboard;

    private void Awake()
    {
        agentBlackboard = GetComponent<BlackBoard>();
    }

    private void Start()
    {
        agentBlackboard.AddValue("position", transform.position);
    }

    private void MoveAgent(Vector2 _movementAddition)
    {
        transform.position = (Vector2)transform.position + _movementAddition;
        agentBlackboard.ModifyValue("position", transform.position);
    }

    public void MoveTo(Vector2 _point)
    {
        Vector2 dir = (_point - (Vector2)transform.position).normalized;
        MoveAgent(dir * moveFactor);
    }

    public void MoveTo(Transform _target)
    {
        Vector2 dir = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        MoveAgent(dir * moveFactor);
    }
}
