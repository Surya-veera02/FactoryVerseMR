
using UnityEngine;
using UnityEngine.UI;

public class FactoryDifficultyPopup : MonoBehaviour
{
    [SerializeField]
    private FactoryDifficultyManager difficultyManager;

    [SerializeField]
    private GameObject popupPanel;

    [SerializeField]
    private Button continueButton;

    private bool level2PopupShown;

    private void Awake()
    {
        if (difficultyManager == null)
            difficultyManager =
                FindFirstObjectByType<FactoryDifficultyManager>();

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePopup);
    }

    private void OnEnable()
    {
        if (difficultyManager == null)
        {
            Debug.LogError(
                "[Difficulty Popup] Difficulty Manager missing."
            );
            return;
        }

        difficultyManager.OnLevelChanged += HandleLevelChanged;

        // Catch up if Level 2 was unlocked while
        // this component was disabled.
        HandleLevelChanged(difficultyManager.CurrentLevel);
    }

    private void OnDisable()
    {
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(int level)
    {
        Debug.Log(
            $"[Difficulty Popup] Level changed: {level}"
        );

        if (popupPanel == null)
        {
            Debug.LogError(
                "[Difficulty Popup] Popup Panel missing."
            );
            return;
        }

        if (level == 1)
        {
            level2PopupShown = false;
            popupPanel.SetActive(false);
            return;
        }

        if (level == 2 && !level2PopupShown)
        {
            popupPanel.SetActive(true);
            level2PopupShown = true;

            Debug.Log(
                "[Difficulty Popup] Level 2 popup activated!"
            );
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ClosePopup);
    }

    [ContextMenu("TEST - Show Difficulty Popup")]
    public void TestShowPopup()
    {
        if (popupPanel == null)
        {
            Debug.LogError("[Popup] Panel reference missing!");
            return;
        }

        popupPanel.SetActive(true);

        Debug.Log("[Popup] Manually activated.");
    }
}
