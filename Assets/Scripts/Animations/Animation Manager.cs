using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    [SerializeField] private List<AnimCreator> animationCreators = new List<AnimCreator>();
    private List<SO_AnimBase> animations = new List<SO_AnimBase>();

    private Animator animator;

    private string newAnimationName;
    private string currentAnimName;
    private SO_AnimBase currentAnimation;

    private bool isInitialized;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (!isInitialized)
        {
            InitTasks();
            isInitialized = true;
        }
    }

    private void InitTasks()
    {
        foreach (AnimCreator animCreator in animationCreators)
        {
            animations.Add(animCreator.CreateAnim());
        }
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        newAnimationName = GetCurrentAnimation();

        if (newAnimationName != currentAnimName)
        {
            currentAnimation.OnPlayEnd(gameObject);

            currentAnimName = newAnimationName;
            PlayAnimation();

            currentAnimation.OnStartPlaying(gameObject);
        }

        currentAnimation.OnPlaying(gameObject);
    }

    private string GetCurrentAnimation()
    {
        foreach (SO_AnimBase anim in animations)
        {
            if (anim.CanPlay(gameObject))
            {
                currentAnimation = anim;
                return anim.animationName;
            }
        }

        return "";
    }

    private void PlayAnimation()
    {
        if (currentAnimName == "") { return; }
        animator.CrossFade(currentAnimName, 0.1f);
    }

    private void OnDestroy()
    {
        if(animator != null) return;
    }
}