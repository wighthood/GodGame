using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildTable", menuName = "BuildTable/BuildTable")]
public class BuildingTable : ScriptableObject
{
    public BuildType buildType;
    public List<RessourceCollection> ressourcesNeeded = new List<RessourceCollection>();
}
