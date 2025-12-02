using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjectScript", menuName = "Scriptable Objects/SO_Ressource", order = 1)]
public class ScriptableObjectScript : ScriptableObject
{ 
    public string name { get; private set;}
    public int dropValue { get; private set;}
}
