using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public BlackBoard agentBlackboard;
    public BlackBoard colonieBlackboard;

    public List<TaskBase> tasks = new List<TaskBase>();

    private bool isTaskFinished = true;
    private TaskBase currentTask;

    private void Awake()
    {
        agentBlackboard = GetComponent<BlackBoard>();

        AgentActions actions = GetComponent<AgentActions>();

        AddNewTask(new TaskEat(this, actions));
        AddNewTask(new TaskATest(this, actions));
        AddNewTask(new TaskBTest(this, actions));
    }

    private void AddNewTask(TaskBase task)
    {
        tasks.Add(task);
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

    public void ResetTask()
    {
        currentTask = null;
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
