using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public BlackBoard agentBlackboard {  get; private set; }
    public BlackBoard colonieBlackboard { get; private set; } 

    private List<TaskBase> tasks = new List<TaskBase>();
    [SerializeField]
    private List<TaskCreator> taskCreators = new List<TaskCreator>();
    private bool isInitialized;

    private bool isTaskFinished = true;
    private TaskBase currentTask;

    private void Awake()
    {
        agentBlackboard = new();

        agentBlackboard.AddValue("transform", transform);
    }

    private void Start()
    {
        if(!isInitialized)
        {
            InitTasks();
            isInitialized = true;
        }
    }

    private void InitTasks()
    {
        AgentActions actions = GetComponent<AgentActions>();

        foreach(TaskCreator tc in taskCreators)
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

        higherPriorityTask.OnStart();
        isTaskFinished = false;
        return higherPriorityTask;
    }

    private void ExecuteTask()
    {
        if (currentTask == null) return;

        isTaskFinished = currentTask.Do();
        
        if (isTaskFinished)
        {
            currentTask.OnFinish();
            currentTask = null;
        }
    }

    private bool IsTooHungry()
    {
        float hunger = agentBlackboard.GetValue<float>("hunger");
        float hungerPriority = Mathf.Sqrt(hunger);
        return (hungerPriority > 0.75f && GetHigherPriorityTask() is TaskEat);
    }

    public void ResetTask()
    {
        isTaskFinished = true;
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
            if (IsTooHungry() && currentTask is not TaskEat)
            {
                currentTask.Cancel();
                return;
            }

            ExecuteTask();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!currentTask) return;

        currentTask.DrawActionsGizmo();
    }
}
