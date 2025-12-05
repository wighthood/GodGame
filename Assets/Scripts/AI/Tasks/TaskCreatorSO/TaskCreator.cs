using UnityEngine;

[CreateAssetMenu(fileName = "TaskCreator", menuName = "Tasks/TaskCreator")]
public class TaskCreator : ScriptableObject
{
    public TaskBase task;

     public TaskBase CreateTask(TaskManager _manager, AgentActions _action)
     {
         TaskBase instance = Instantiate(task);
        instance.Init(_manager, _action);
        return instance;
     }
}