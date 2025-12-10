using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private bool isPlaying;

    [SerializeField] private List<SO_AnimCreator> animCreator;
    [SerializeField] private List<SO_AnimBase> animBase;
    [SerializeField] private Animator animator;
    
    
    private void Start()
    {
        animBase = new List<SO_AnimBase>();
        
        foreach (SO_AnimCreator animCrea in animCreator)
        {
            SO_AnimBase based = animCrea.CreateAnim(this);
            animBase.Add(based);
        }
    }

    private void Update()
    {
        foreach (SO_AnimBase animName in animBase)
        {
            if (animName.CondAnim() == true)
            {
                isPlaying = true;
                animator.Play("WalkingAnim");
                break;
            }

            if (!isPlaying)
            {
                animator.Play("Idle");
            }
        }
    }
}
