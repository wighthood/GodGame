using UnityEngine;

namespace AI
{
    public enum RequestType
    {
        NeedResource,    // Besoin d'une ressource (nourriture, eau, etc.)
        NeedAction,      // Besoin qu'une action soit effectuée
        NeedHelp         // Besoin d'aide générale
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
            this.requestId = System.Guid.NewGuid().ToString();
            this.requester = requester;
            this.type = type;
            this.description = description;
            this.priority = priority;
            this.isFulfilled = false;
        }

        public void Fulfill()
        {
            isFulfilled = true;
            Debug.Log($"Request fulfilled: {description}");
        }
    }
}
