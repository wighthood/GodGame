using System.Collections.Generic;
using UnityEngine;

public class BlackBoard
{
    private Dictionary<string, object> blackBoardValues = new Dictionary<string, object>();

    public T GetValue<T>(string _varName)
    {
        return (T)blackBoardValues[_varName]; 
    }

    public void AddValue(string _varName, object value)
    {
        if (blackBoardValues.ContainsKey(_varName))
        {
            Debug.LogWarning($"A blackboard varriable with the name {_varName} already existe !");
            return;
        }

        blackBoardValues[_varName] = value;
    }

    public void ModifyValue(string _varName, object value)
    {
        if (!blackBoardValues.ContainsKey(_varName))
        {
            Debug.LogWarning($"No blackboard varriable with the name {_varName} existe !");
            return;
        }

        blackBoardValues[_varName] = value;
    }

    public bool HasKey(string _varName)
    {
        return blackBoardValues.ContainsKey(_varName);
    }
}
