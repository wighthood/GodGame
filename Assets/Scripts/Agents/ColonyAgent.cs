using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class ColonyAgent : MonoBehaviour, IColonyAgent
{
    public bool autoRegister = true;
    public bool canFormColony = true;
    public string species = "Pimu";

    public float positionUpdateInterval = 0.5f;
    public float movementThreshold = 0.25f;

    private Vector3 _lastPosition;
    private float _timer;
    private bool _registered;

    void Start()
    {
        _lastPosition = transform.position;
        if (autoRegister && ColonieSystem.Instance != null)
        {
            ColonieSystem.Instance.RegisterAgent(this);
            _registered = true;
        }
    }

    void Update()
    {
        if (!_registered && autoRegister && ColonieSystem.Instance != null)
        {
            ColonieSystem.Instance.RegisterAgent(this);
            _registered = true;
        }

        _timer += Time.deltaTime;
        if (_timer >= positionUpdateInterval)
        {
            _timer = 0f;
            Vector3 current = transform.position;
            float dist = Vector3.Distance(current, _lastPosition);
            if (dist >= movementThreshold)
            {
                if (_registered && ColonieSystem.Instance != null)
                {
                    ColonieSystem.Instance.UpdateAgentCell(this, _lastPosition);
                }
                _lastPosition = current;
            }
        }
    }

    void OnDestroy()
    {
        if (_registered && ColonieSystem.Instance != null)
        {
            ColonieSystem.Instance.UnregisterAgent(this);
            _registered = false;
        }
    }

    public void ForceRegister()
    {
        if (ColonieSystem.Instance != null)
        {
            ColonieSystem.Instance.RegisterAgent(this);
            _registered = true;
            _lastPosition = transform.position;
        }
    }

    public string GetSpecies()
    {
        return species != null ? species : string.Empty;
    }

    public bool CanFormColony() => canFormColony;

    private IColony _currentColony;

    public void SetCurrentColony(IColony colony)
    {
        _currentColony = colony;
    }

    public IColony GetCurrentColony()
    {
        return _currentColony;
    }

    public Transform GetTransform() => transform;
    public GameObject GetGameObject() => gameObject;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.12f);

        IColony col = GetCurrentColony();
        string label = "No colony";
        if (col != null)
        {
            label = $"Colony {col.GetId()} ({col.GetSpecies()}) - {col.GetInhabitants()}/{col.GetMaxInhabitants()}";
        }
        Handles.Label(transform.position + Vector3.up * 1.2f, label);
    }
#endif
}
