using UnityEngine;

[CreateAssetMenu(fileName = "Ressource", menuName = "Scriptable Objects/SO_Ressource", order = 1)]
public class SO_Ressource : ScriptableObject
{
    public new string name;
    public int dropValue;
    public RessourceType ressourceType;
}


public enum RessourceType
{
    none = 0,
    food = 1,
    wood = 2,
}