
using UnityEngine;
using TMPro;

public class FloatingCreditFeedback : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float riseSpeed = 0.08f;

    [Header("References")]
    [SerializeField] private TMP_Text feedbackText;

    private Color originalColor;
    private float elapsedTime;

    private void Awake()
    {
        if (feedbackText == null)
            feedbackText = GetComponent<TMP_Text>();
    }

    public void Show(int amount)
    {
        if (feedbackText == null)
            return;

        feedbackText.text =
            amount > 0
                ? $"+{amount} CR"
                : $"{amount} CR";

        originalColor =
            amount >= 0 ? Color.green : Color.red;

        feedbackText.color = originalColor;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        transform.position +=
            Vector3.up * riseSpeed * Time.deltaTime;

        // Face the player's camera.
        Camera cam = Camera.main;

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.transform.position
            );
        }

        float alpha =
            1f - Mathf.Clamp01(elapsedTime / lifetime);

        if (feedbackText != null)
        {
            Color color = originalColor;
            color.a = alpha;
            feedbackText.color = color;
        }

        if (elapsedTime >= lifetime)
            Destroy(gameObject);
    }
}
