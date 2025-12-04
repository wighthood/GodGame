using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public BlackBoard agentBlackboard {  get; private set; }
    public BlackBoard colonieBlackboard { get; private set; }

    public List<TaskBase> tasks = new List<TaskBase>();

    private bool isTaskFinished = true;
    private TaskBase currentTask;

    private void Awake()
    {
        agentBlackboard = new();

        agentBlackboard.AddValue("transform", transform);

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
        print("exe");

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

    private void OnDrawGizmosSelected()
    {
        if(currentTask != null)
        {
            print("no task");
            return;
        }

        if(currentTask is TaskATest)
        {
            Gizmos.color = Color.green;
            foreach(Cell cell in ((TaskATest)currentTask).pathDebug)
            {
                Gizmos.DrawCube(cell.position + new Vector2(0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.1f));
            }
        }

        if (currentTask is TaskBTest)
        {
            Gizmos.color = Color.green;
            foreach (Cell cell in ((TaskATest)currentTask).pathDebug)
            {
                Gizmos.DrawCube(cell.position + new Vector2(0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.1f));
            }
        }

        if (currentTask is TaskEat)
        {
            Gizmos.color = Color.green;
            if(((TaskATest)currentTask).pathDebug.Count > 1)
            {
                foreach (Cell cell in ((TaskATest)currentTask).pathDebug)
                {
                    Gizmos.DrawCube(cell.position + new Vector2(0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.1f));
                }
            }
        }
    }
}
