using System.Collections.Generic;
using System.Linq;
using AI.Jobs;
using UnityEngine;

namespace AI
{
    public class JobBoard : MonoBehaviour
    {
        public static JobBoard Instance { get; private set; }
        
        private List<Job> _jobs = new List<Job>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public Job PostJob(string jobType, WorldState goal, float priority = 1f)
        {
            Job job = new Job(jobType, goal, priority);
            _jobs.Add(job);
            Debug.Log($"[JobBoard] Nouveau job posté: {jobType} (ID: {job.jobId})");
            return job;
        }
        
        public Job GetBestAvailableJob(Agent agent)
        {
            return _jobs
                .Where(j => j.status == JobStatus.Available)
                .OrderByDescending(j => j.priority)
                .FirstOrDefault();
        }
        
        public bool AssignJob(Job job, Agent agent)
        {
            if (job.status != JobStatus.Available) return false;
            
            job.assignedAgent = agent;
            job.status = JobStatus.Assigned;
            Debug.Log($"[JobBoard] Job {job.jobType} assigné à {agent.name}");
            return true;
        }
        
        public void CompleteJob(Job job)
        {
            job.status = JobStatus.Completed;
            Debug.Log($"[JobBoard] Job {job.jobType} complété par {job.assignedAgent?.name}");
            _jobs.Remove(job);
        }
        
        public void FailJob(Job job)
        {
            job.status = JobStatus.Failed;
            job.assignedAgent = null;
            Debug.Log($"[JobBoard] Job {job.jobType} échoué, remis en disponible");
            job.status = JobStatus.Available;
        }
        
        public List<Job> GetAllJobs() => new List<Job>(_jobs);
        
        public List<Job> GetJobsByType(string jobType) => _jobs.Where(j => j.jobType == jobType).ToList();
    }
}


