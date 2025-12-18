using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    [SerializeField] private List<AnimCreator> animationCreators = new List<AnimCreator>();
    private readonly List<SO_AnimBase> animations = new List<SO_AnimBase>();

    private Animator animator;
    private SO_AnimBase currentAnimation;
    private string currentAnimName;

    private bool isInitialized;

    private string newAnimationName;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (!isInitialized)
        {
            InitTasks();
            isInitialized = true;
        }
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void OnDestroy()
    {
        if (animator != null) return;
    }

    private void InitTasks()
    {
        foreach (AnimCreator animCreator in animationCreators)
        {
            animations.Add(animCreator.CreateAnim());
        }
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
}
