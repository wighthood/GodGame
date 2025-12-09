using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_AnimCreator", menuName = "Scriptable Objects/SO_AnimCreator")]
public class SO_AnimCreator : ScriptableObject
{
    [SerializeField] private SO_AnimBase soAnim;
    
    public SO_AnimBase CreateAnim(AnimationManager anim)
    {
        SO_AnimBase animBase = Instantiate(soAnim);
        animBase.Init(anim);
        return animBase;
    }
}
