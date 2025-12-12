using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public BlackBoard agentBlackboard { get; private set; }
    public BlackBoard colonieBlackboard { get; private set; }

    private List<TaskBase> tasks = new List<TaskBase>();
    [SerializeField]
    private List<TaskCreator> taskCreators = new List<TaskCreator>();
    private bool isInitialized;

    private bool isTaskFinished = true;
    private TaskBase currentTask;

    private AgentActions actions;

    private void Awake()
    {
        agentBlackboard = new();

        agentBlackboard.AddValue("transform", transform);
    }

    private void Start()
    {
        if (!isInitialized)
        {
            InitTasks();
            isInitialized = true;
        }
    }

    private void InitTasks()
    {
        actions = GetComponent<AgentActions>();

        foreach (TaskCreator tc in taskCreators)
        {
            tasks.Add(tc.CreateTask(this, actions));
        }
    }

    private TaskBase GetHigherPriorityTask()
    {
        TaskBase higherPriorityTask = tasks[0];
        foreach (TaskBase task in tasks)
        {
            if (higherPriorityTask.GetPriority() < task.GetPriority())
            {
                higherPriorityTask = task;
            }
        }

        print($"starting {higherPriorityTask.name}");
        higherPriorityTask.OnStart();
        isTaskFinished = false;
        return higherPriorityTask;
    }

    private void ExecuteTask()
    {
        if (currentTask == null) return;

        isTaskFinished = currentTask.Do();
        print($"doing {currentTask.name}");

        if (isTaskFinished && currentTask != null)
        {
            print($"finish {currentTask.name}");
            currentTask.OnFinish();
            currentTask = null;
        }
    }

    private void Update()
    {
        //the colony blackboard linked
        if (colonieBlackboard == null)
        {
            ColonyAgent agent = GetComponent<ColonyAgent>();
            if (agent != null)
            {
                IColony col = agent.GetCurrentColony();
                if (col != null && col is Colony concreteColony)
                {
                    colonieBlackboard = concreteColony.BlackBoard;
                }
            }
        }

        if (IsOccupied()) { return; }

        if (isTaskFinished)
        {
            currentTask = GetHigherPriorityTask();
        }
        else
        {
            ExecuteTask();
        }
    }

    private bool IsOccupied()
    {
        return actions.isBuilding;
    }

    private void OnDrawGizmosSelected()
    {
        if (!currentTask)
        {
            GUIStyle style = new GUIStyle();

            if (!IsOccupied())
            {
                style.normal.textColor = Color.red;
                Handles.Label(transform.position + Vector3.up * 0.5f, $"Idle", style);
                return;
            }

            style.normal.textColor = Color.green;

            if (actions.isBuilding)
            {
                Handles.Label(transform.position + Vector3.up * 0.5f, $"Building", style);
            }

            return;
        }

        currentTask.DrawActionsGizmo();
    }
}
