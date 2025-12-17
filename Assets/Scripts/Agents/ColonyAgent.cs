using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SpeciesType
{
    Pimu,
    EvilPimu
}

[DisallowMultipleComponent]
public class ColonyAgent : MonoBehaviour, I_ColonyAgent
{
    [Header("Settings")]
    public bool autoRegister = true;
    public bool canFormColony = true;
    public SpeciesType speciesType = SpeciesType.Pimu;

    [Header("Optimization Settings")]
    public float positionUpdateInterval = 0.5f;
    public float movementThreshold = 0.25f;
    
    private Vector3 lastPosition;
    private bool registered;
    private float sqrMovementThreshold;
    private WaitForSeconds waitObj;
    private Coroutine checkRoutine;

    void Awake()
    {
        sqrMovementThreshold = movementThreshold * movementThreshold;
        
        waitObj = new WaitForSeconds(positionUpdateInterval);
    }

    void OnEnable()
    {
        lastPosition = transform.position;
        checkRoutine = StartCoroutine(CheckingPosition());
    }

    void OnDisable()
    {
        if (registered)
        {
            ColonyEvents.OnUnregisterAgentEvent?.Invoke(this);
            registered = false;
        }
        if (checkRoutine != null) StopCoroutine(checkRoutine);
    }
    
    IEnumerator CheckingPosition()
    {
        yield return null;

        if (autoRegister && !registered)
        {
            RegisterAgent();
        }

        while (true)
        {
            yield return waitObj;
            
            CheckMovement();
        }
    }

    private void CheckMovement()
    {
        if ((transform.position - lastPosition).sqrMagnitude >= sqrMovementThreshold)
        {
            if (registered)
            {
                ColonyEvents.OnUpdateAgentPositionEvent?.Invoke(this, lastPosition);
            }
            lastPosition = transform.position;
        }
    }

    private void RegisterAgent()
    {
        if (registered) return;
        ColonyEvents.OnRegisterAgentEvent?.Invoke(this);
        registered = true;
        lastPosition = transform.position;
    }

    public void ForceRegister()
    {
        RegisterAgent();
    }

    public SpeciesType GetSpecies() => speciesType;
    public bool CanFormColony() => canFormColony;

    private I_Colony _currentColony;
    public void SetCurrentColony(I_Colony _colony) => _currentColony = _colony;
    public I_Colony GetCurrentColony() => _currentColony;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.12f);

        I_Colony col = GetCurrentColony();
        string label = "No colony";
        if (col != null)
        {
            label = $"Colony {col.GetId()} - {col.GetInhabitants()}/{col.GetMaxInhabitants()}";
        }
        Handles.Label(transform.position + Vector3.up * 1.2f, label);
    }
#endif
}