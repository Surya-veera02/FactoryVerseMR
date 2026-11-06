
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
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged -= HandleLevelChanged;
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ClosePopup);
    }

    private void HandleLevelChanged(int level)
    {
        if (popupPanel == null)
            return;

        popupPanel.SetActive(level == 2);
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
