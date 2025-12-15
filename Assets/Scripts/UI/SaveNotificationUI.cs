using UnityEngine;
using System.Collections;

public class SaveNotificationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (notificationGroup != null)
        {
            notificationGroup.alpha = 0;
        }
        else
        {
            Debug.LogWarning("SaveNotificationUI: CanvasGroup is missing!");
        }
    }

    private void OnEnable()
    {
        SaveEvents.OnSaveCompletedEvent += OnSaveCompleted;
    }

    private void OnDisable()
    {
        SaveEvents.OnSaveCompletedEvent -= OnSaveCompleted;
    }

    private void OnSaveCompleted()
    {
        if (notificationGroup == null) return;
        
        StopAllCoroutines();
        StartCoroutine(ShowNotificationRoutine());
    }

    private IEnumerator ShowNotificationRoutine()
    {
        notificationGroup.alpha = 1;

        yield return new WaitForSecondsRealtime(displayDuration);

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            notificationGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        notificationGroup.alpha = 0;
    }
}
