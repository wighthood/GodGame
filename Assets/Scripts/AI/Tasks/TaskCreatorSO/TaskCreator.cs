using UnityEngine;

[CreateAssetMenu(fileName = "TaskCreator", menuName = "Tasks/TaskCreator")]
public class TaskCreator : ScriptableObject
{
    public TaskBase task;

     public TaskBase CreateTask(TaskManager _manager, AgentActions _action)
     {
        if (task is TaskHarverestBase)
        {
            TaskHarverestBase harvrestInstance = Instantiate((TaskHarverestBase)task);
            harvrestInstance.Init(_manager, _action, 10);
            return harvrestInstance;
        }

        TaskBase instance = Instantiate(task);
        instance.Init(_manager, _action);
        return instance;
     }
}