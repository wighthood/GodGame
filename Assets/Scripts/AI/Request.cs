using UnityEngine;

namespace AI
{
    public enum RequestType
    {
        NeedResource,    
        NeedAction,      
        NeedHelp         
    }

    [System.Serializable]
    public class Request
    {
        public string requestId;
        public Agent requester;
        public RequestType type;
        public string description;
        public float priority;
        public bool isFulfilled;
        
        public string resourceName;
        public int resourceAmount;
        public Vector3 targetLocation;

        public Request(Agent requester, RequestType type, string description, float priority = 1f)
        {
            requestId = System.Guid.NewGuid().ToString();
            this.requester = requester;
            this.type = type;
            this.description = description;
            this.priority = priority;
            isFulfilled = false;
        }

        public void Fulfill()
        {
            isFulfilled = true;
        }
    }
}
