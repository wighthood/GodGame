using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObjectScript", menuName = "Scriptable Objects/SO_Building", order = 1)]
public class NewScriptableObjectScript : ScriptableObject
{
    public string name;
    public int hp;
    public int ressourceCost;
    public GameObject buidingPrefab;
}
