using System;
using UnityEngine;

namespace AI.Jobs
{
    [Serializable]
    public class Job
    {
        public string jobId;
        public string jobType;
        public WorldState requiredGoal;
        public float priority;
        public Agent assignedAgent;
        public JobStatus status;
        
        public Job(string type, WorldState goal, float prio = 1f)
        {
            jobId = Guid.NewGuid().ToString();
            jobType = type;
            requiredGoal = goal;
            priority = prio;
            status = JobStatus.Available;
        }
    }
    
    public enum JobStatus
    {
        Available,
        Assigned,
        InProgress,
        Completed,
        Failed
    }
}
