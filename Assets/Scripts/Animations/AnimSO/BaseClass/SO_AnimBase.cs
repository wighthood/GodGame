using System;
using UnityEngine;

public abstract class SO_AnimBase : ScriptableObject
{
    public string animationName;

    public abstract bool CanPlay(GameObject _entity);

    public virtual void OnPlaying(GameObject _entity)
    {

    }

    public virtual void OnStartPlaying(GameObject _entity)
    {

    }

    public virtual void OnPlayEnd(GameObject _entity)
    {

    }
}