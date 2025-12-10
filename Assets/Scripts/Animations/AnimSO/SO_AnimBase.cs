using UnityEngine;

[CreateAssetMenu(fileName = "SO_AnimBase", menuName = "Scriptable Objects/SO_AnimBase")]
public class SO_AnimBase : ScriptableObject
{
    public string animName;
    private bool animCond;
    private AnimationManager animManager;

    public bool CondAnim()
    {
        return animCond;
    }
    
    public void Init(AnimationManager animMan)
    {
        animManager = animMan;
    }
}
