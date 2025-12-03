using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private SO_Ressource ressource;
    [SerializeField] private int ressourceRemaining;

    private void Start()
    {

    }

    public RessourceType GetRessourceType()
    {
        return ressource.ressourceType;
    }

    public void OnHarvrestingRessource()
    {
        print("harvresting");
        ressourceRemaining--;

        if (ressourceRemaining == 0)
        {
            print("No ressources remaining");
            GetComponentInParent<MapRessourceManager>().RemoveFromListForDestroy(this);
            Destroy(gameObject, 0.5f);
        }
    }
}
