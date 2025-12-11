using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RessourcesNoiseRule", menuName = "Scriptable Objects/SO_RessourcesNoiseRule")]
public class SO_RessourcesNoiseRules : ScriptableObject
{
    public List<RessourceRule> ressourceRules = new List<RessourceRule>();
}
