using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlackBoard
{
    private Dictionary<string, object> blackBoardValues = new Dictionary<string, object>();

    public T GetValue<T>(string _varName)
    {
        if (!blackBoardValues.ContainsKey(_varName)) return default;
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

    public void AddValueOrModify(string _varName, object value)
    {
        if (blackBoardValues.ContainsKey(_varName)) blackBoardValues[_varName] = value;
        else blackBoardValues[_varName] = value;
    }
    
    public Dictionary<string, object> BbValues()
    {
        return blackBoardValues;
    }
}
