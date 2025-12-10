using UnityEngine;

[CreateAssetMenu(fileName = "SO_AnimBase", menuName = "Scriptable Objects/SO_AnimBase")]
public abstract class SO_AnimBase : ScriptableObject
{
    public string animName;
    private AnimationManager animManager;
    public abstract bool CondAnim();
    
    public void Init(AnimationManager animMan)
    {
        animManager = animMan;
    }
}
