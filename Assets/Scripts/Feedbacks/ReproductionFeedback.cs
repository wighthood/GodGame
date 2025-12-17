using UnityEngine;

public class ReproductionFeedback : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnFinishAnimation()
    {
        Destroy(gameObject);
    }
}
