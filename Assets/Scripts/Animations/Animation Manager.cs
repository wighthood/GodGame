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
        foreach ()
        {
            
        }
    }

    private void Update()
    {
        foreach (SO_AnimBase Animname in animBase)
        {
            if (Animname.CondAnim() == true)
            {
                isPlaying = true;
                animator.Play();
                break;
            }

            if (!isPlaying)
            {
                animator.Play("Idle");
            }
        }
    }
}
