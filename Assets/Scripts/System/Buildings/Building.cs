using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField]
    private Sprite[] Sprites;

    public BuildType Type { get; private set; }
    public Colony Owner { get; private set; }
    public GameObject Root { get; private set; }

    private bool _initialized = false;

    public BuildingTable BuildingTable;

    public void Initialize(BuildType type, Colony owner, GameObject root)
    {
        Type = type;
        Root = root != null ? root : gameObject;

        if (_initialized)
        {
            if (Owner != owner)
            {
                if (Owner != null) Owner.RemoveBuilding(Root);
                Owner = owner;
                if (Owner != null) Owner.AddBuilding(Root);
            }
            return;
        }

        Owner = owner;
        if (Owner != null) Owner.AddBuilding(Root);
        _initialized = true;
        RandomSprite();
    }

    private void RandomSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (!spriteRenderer) { return; }

        spriteRenderer.sprite = Sprites[Random.Range(1, Sprites.Length)];
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
