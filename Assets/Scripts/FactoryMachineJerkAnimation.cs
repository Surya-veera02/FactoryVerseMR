
using System.Collections;
using UnityEngine;

public class FactoryMachineJerkAnimation : MonoBehaviour
{
    [Header("Moving Visual Part")]
    [SerializeField] private Transform movingPart;

    [Header("Animation")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float sidewaysDistance = 0.012f;
    [SerializeField] private float compression = 0.08f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Coroutine animationRoutine;

    private void Awake()
    {
        if (movingPart == null)
            return;

        originalPosition = movingPart.localPosition;
        originalScale = movingPart.localScale;
    }

    public void PlayAnimation()
    {
        if (movingPart == null)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / duration);

            // Side-to-side mechanical movement.
            float sideways =
                Mathf.Sin(progress * Mathf.PI * 4f) *
                sidewaysDistance *
                (1f - progress);

            // Compress and return to original size.
            float compress =
                Mathf.Sin(progress * Mathf.PI) *
                compression;

            movingPart.localPosition =
                originalPosition +
                Vector3.right * sideways;

            movingPart.localScale = new Vector3(
                originalScale.x,
                originalScale.y * (1f - compress),
                originalScale.z
            );

            yield return null;
        }

        RestoreOriginal();
        animationRoutine = null;
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        RestoreOriginal();
    }

    private void RestoreOriginal()
    {
        if (movingPart == null)
            return;

        movingPart.localPosition = originalPosition;
        movingPart.localScale = originalScale;
    }
}
