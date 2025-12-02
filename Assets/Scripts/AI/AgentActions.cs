using UnityEngine;

public class AgentActions : MonoBehaviour
{
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

    public void MoveAgent(Vector2 _movementAddition)
    {
        transform.position = (Vector2)transform.position + _movementAddition;
        agentBlackboard.ModifyValue("position", transform.position);
    }
}
