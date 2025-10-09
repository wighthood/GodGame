using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI
{
    public class RequestBoard : MonoBehaviour
    {
        public static RequestBoard Instance { get; private set; }

        [SerializeField] private List<Request> activeRequests = new List<Request>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PostRequest(Request request)
        {
            if (request == null) return;

            bool hasSimilarRequest = activeRequests.Any(r => 
                !r.isFulfilled && 
                r.requester == request.requester && 
                r.type == request.type && 
                r.description == request.description
            );

            if (hasSimilarRequest)
            {
                Debug.Log($"[RequestBoard] Request already exists for {request.requester.name}: {request.description}");
                return;
            }
            
            activeRequests.Add(request);
            Debug.Log($"[RequestBoard] New request posted: {request.description} (Priority: {request.priority})");
        }

        public Request GetHighestPriorityRequest()
        {
            if (activeRequests.Count == 0) return null;

            var unfulfilledRequests = activeRequests.Where(r => !r.isFulfilled).ToList();
            if (unfulfilledRequests.Count == 0) return null;

            return unfulfilledRequests.OrderByDescending(r => r.priority).First();
        }

        public List<Request> GetRequestsByType(RequestType type)
        {
            return activeRequests.Where(r => r.type == type && !r.isFulfilled).ToList();
        }

        public void FulfillRequest(string requestId)
        {
            var request = activeRequests.FirstOrDefault(r => r.requestId == requestId);
            if (request != null)
            {
                request.Fulfill();
            }
        }

        public void CleanupFulfilledRequests()
        {
            activeRequests.RemoveAll(r => r.isFulfilled);
        }

        private void Update()
        {
            // Nettoyer périodiquement les demandes satisfaites
            if (Time.frameCount % 300 == 0) // Toutes les ~5 secondes à 60fps
            {
                CleanupFulfilledRequests();
            }
        }

        public int GetActiveRequestCount()
        {
            return activeRequests.Count(r => !r.isFulfilled);
        }
    }
}
