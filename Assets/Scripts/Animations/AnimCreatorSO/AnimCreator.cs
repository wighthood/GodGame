using UnityEngine;

[CreateAssetMenu(fileName = "AnimCreator", menuName = "Animation/AnimCreator")]
public class AnimCreator : ScriptableObject
{
    public SO_AnimBase anim;

    public SO_AnimBase CreateAnim()
    {
        SO_AnimBase instance = Instantiate(anim);
        return instance;
    }
}
