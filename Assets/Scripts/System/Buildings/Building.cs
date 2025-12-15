using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildType Type { get; private set; }
    public Colony Owner { get; private set; }
    public GameObject Root { get; private set; }

    private bool initialized = false;

    public BuildingTable BuildingTable;

    public void Initialize(BuildType _type, Colony _owner, GameObject _root)
    {
        Type = _type;
        Root = _root != null ? _root : gameObject;

        if (initialized)
        {
            if (Owner != _owner)
            {
                if (Owner != null) Owner.RemoveBuilding(Root);
                Owner = _owner;
                if (Owner != null) Owner.AddBuilding(Root);
            }
            return;
        }

        Owner = _owner;
        if (Owner != null) Owner.AddBuilding(Root);
        initialized = true;
    }

    void OnDestroy()
    {
        if (Owner != null) Owner.RemoveBuilding(Root);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Owner != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Owner.GetColonyCenter());
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, $"Owner Id={Owner.GetId()} size={Owner.GetInhabitants()}/{Owner.GetMaxInhabitants()}");
        }
        else
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1f, "Owner=null");
        }
    }
#endif
}

[System.Serializable]
public class RessourceCollection
{
    public RessourceType RessourceType;
    public uint number;
}

public enum BuildType : int
{
    House,
    Storage,
    Farm
}
