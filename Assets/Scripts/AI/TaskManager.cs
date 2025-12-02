using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public BlackBoard agentBlackboard;
    public BlackBoard colonieBlackboard;

    public List<TaskBase> tasks;

    private bool isTaskFinished = true;
    private TaskBase currentTask;

    private void Awake()
    {
        agentBlackboard = GetComponent<BlackBoard>();

        AgentActions actions = GetComponent<AgentActions>();

        tasks = new List<TaskBase>();
        tasks.Add(new TaskATest(this, actions));
        tasks.Add(new TaskBTest(this, actions));
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

        higherPriorityTask.OnStart();
        isTaskFinished = false;
        return higherPriorityTask;
    }

    private void ExecuteTask()
    {
        isTaskFinished = currentTask.Do();

        if (isTaskFinished)
        {
            currentTask.OnFinish();
            currentTask = null;
        }
    }

    private void Update()
    {
        if (isTaskFinished)
        {
            currentTask = GetHigherPriorityTask();
        }
        else
        {
            ExecuteTask();
        }
    }
}
