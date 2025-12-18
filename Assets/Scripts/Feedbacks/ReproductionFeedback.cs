using UnityEngine;

public class ReproductionFeedback : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnFinishAnimation()
    {
        Destroy(gameObject);
    }
}
