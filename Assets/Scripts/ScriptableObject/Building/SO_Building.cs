using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/SO_Building", order = 1)]
public class SO_Building : ScriptableObject
{
    public new string name;
    public int hp;
    public int ressourceCost;
    public GameObject buidingPrefab;
}
