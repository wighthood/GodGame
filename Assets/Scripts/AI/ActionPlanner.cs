using System.Collections.Generic;
using System.Linq;
using AI.Action;
using UnityEngine;

namespace AI
{
    public class Planner
    {
        public static Queue<ActionBase> Plan(
            WorldState worldState,
            List<ActionBase> availableActions,
            WorldState goal)
        {
            List<Node> leaves = new List<Node>();
            Node start = new Node(null, 0, worldState.Clone(), null);

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

        private static bool BuildGraph(Node parent, List<Node> leaves, List<ActionBase> actions, WorldState goal)
        {
            bool foundPath = false;

            foreach (ActionBase action in actions)
            {
                if (MeetsPreconditions(action.Preconditions, parent.state))
                {

                    WorldState newState = parent.state.Clone();
                    foreach (var eff in action.Effects)
                        newState.Set(eff.Key, eff.Value);

                    Node node = new Node(parent, parent.cost + action.cost, newState, action);

                    if (newState.MeetsGoal(goal))
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

        private static bool MeetsPreconditions(Dictionary<string, object> preconditions, WorldState state)
        {
            foreach (var precondition in preconditions)
            {
                if (!state.Has(precondition.Key)) return false;

                object stateValue = state.Get<object>(precondition.Key);
                if (!precondition.Value.Equals(stateValue)) return false;
            }

            return true;
        }

        private class Node
        {
            public Node parent;
            public float cost;
            public WorldState state;
            public ActionBase action;

            public Node(Node parent, float cost, WorldState state, ActionBase action)
            {
                this.parent = parent;
                this.cost = cost;
                this.state = state;
                this.action = action;
            }
        }
    }
}
