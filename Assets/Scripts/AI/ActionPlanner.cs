using System.Collections.Generic;
using System.Linq;
using AI.Action;

public static class GoapPlanner
{
    public static Queue<ActionBase> Plan(
        Dictionary<string, bool> worldState,
        List<ActionBase> availableActions,
        Dictionary<string, bool> goal)
    {
        List<Node> leaves = new List<Node>();
        Node start = new Node(null, 0, new Dictionary<string, bool>(worldState), null);

        bool success = BuildGraph(start, leaves, availableActions, goal);

        if (!success) return null;

        Node cheapest = leaves.OrderBy(l => l.cost).First();

        List<ActionBase> result = new List<ActionBase>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
                result.Insert(0, n.action);
            n = n.parent;
        }

        return new Queue<ActionBase>(result);
    }

    private static bool BuildGraph(Node parent, List<Node> leaves, List<ActionBase> actions, Dictionary<string, bool> goal)
    {
        bool foundPath = false;

        foreach (ActionBase action in actions)
        {
            if (InState(action.Preconditions, parent.state))
            {
                Dictionary<string, bool> newState = new Dictionary<string, bool>(parent.state);
                foreach (var eff in action.Effects)
                    newState[eff.Key] = eff.Value;

                Node node = new Node(parent, parent.cost + action.cost, newState, action);

                if (InState(goal, newState))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    List<ActionBase> subset = actions.Where(a => a != action).ToList();
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found) foundPath = true;
                }
            }
        }

        return foundPath;
    }

    private static bool InState(Dictionary<string, bool> test, Dictionary<string, bool> state)
    {
        foreach (var t in test)
        {
            if (!state.ContainsKey(t.Key)) return false;
            if (state[t.Key] != t.Value) return false;
        }
        return true;
    }

    private class Node
    {
        public Node parent;
        public float cost;
        public Dictionary<string, bool> state;
        public ActionBase action;

        public Node(Node parent, float cost, Dictionary<string, bool> state, ActionBase action)
        {
            this.parent = parent;
            this.cost = cost;
            this.state = state;
            this.action = action;
        }
    }
}

