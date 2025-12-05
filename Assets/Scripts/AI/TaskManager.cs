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
        print(currentTask);
        if (!currentTask) return;

        if (currentTask is TaskWandering wander)
        {
            if (wander.pathDebug == null || wander.pathDebug.Count == 0)
                return;

            Gizmos.color = Color.green;

            for (int i = 0; i < wander.pathDebug.Count - 1; i++)
            {
                Vector2 firstPos = new();
                firstPos.Set(wander.pathDebug[i].position.x + 0.5f, wander.pathDebug[i].position.y + 0.5f);
                Vector2 secPos = new();
                secPos.Set(wander.pathDebug[i + 1].position.x + 0.5f, wander.pathDebug[i + 1].position.y + 0.5f);
                Gizmos.DrawLine(firstPos, secPos);
            }
        }

        if (currentTask is TaskEat eat)
        {
            if (eat.pathDebug == null || eat.pathDebug.Count == 0)
                return;

            Gizmos.color = Color.yellow;

            foreach (Cell cell in eat.pathDebug)
            {
                if (cell == null) continue;
                Gizmos.DrawCube(cell.position + new Vector2(0.5f, 0.5f),
                                new Vector3(0.5f, 0.5f, 0.1f));
            }
        }
    }


}
