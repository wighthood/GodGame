using UnityEngine;

[CreateAssetMenu(fileName = "IdleAnim", menuName = "Animation/IdleAnim")]
internal class IdleAnim : SO_AnimBase
{
    public override bool CanPlay(GameObject _entity)
    {
        return true;
    }
}
