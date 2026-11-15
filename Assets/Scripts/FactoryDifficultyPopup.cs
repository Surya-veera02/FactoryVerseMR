
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactoryDifficultyPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactoryDifficultyManager difficultyManager;

    [SerializeField]
    private GameObject popupPanel;

    [SerializeField]
    private Button continueButton;

    [Header("Existing Popup Text")]
    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text warningText;

    [SerializeField]
    private TMP_Text warningText2;

    private readonly Queue<int> pendingLevels = new();

    private int highestLevelShown = 1;
    private bool popupOpen;

    private void Awake()
    {
        if (difficultyManager == null)
        {
            difficultyManager =
                FindFirstObjectByType<FactoryDifficultyManager>();
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePopup);
    }

    private void OnEnable()
    {
        if (difficultyManager == null)
            return;

        difficultyManager.OnLevelChanged += HandleLevelChanged;

        // Catch up if difficulty changed while disabled.
        HandleLevelChanged(difficultyManager.CurrentLevel);
    }

    private void OnDisable()
    {
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged(int level)
    {
        if (level <= 1)
        {
            highestLevelShown = 1;
            pendingLevels.Clear();
            popupOpen = false;

            if (popupPanel != null)
                popupPanel.SetActive(false);

            return;
        }

        if (level <= highestLevelShown)
            return;

        for (int nextLevel = highestLevelShown + 1;
             nextLevel <= level;
             nextLevel++)
        {
            pendingLevels.Enqueue(nextLevel);
        }

        highestLevelShown = level;

        if (!popupOpen)
            ShowNextPopup();
    }

    private void ShowNextPopup()
    {
        if (pendingLevels.Count == 0)
        {
            popupOpen = false;
            return;
        }

        if (popupPanel == null)
        {
            Debug.LogError(
                "[Difficulty Popup] Popup Panel missing."
            );

            popupOpen = false;
            return;
        }

        int level = pendingLevels.Dequeue();

        if (titleText != null)
            titleText.text = "DIFFICULTY INCREASED!";

        if (levelText != null)
            levelText.text = $"LEVEL {level}";

        switch (level)
        {
            case 2:
                if (descriptionText != null)
                {
                    descriptionText.text =
                        "Quality control just got harder! " +
                        "Defective packages are now more common.";
                }

                if (warningText != null)
                {
                    warningText.text =
                        "40%\nDefect rate";
                }

                if (warningText2 != null)
                {
                    warningText2.text =
                        "-10 CR\nWrong rejection";
                }

                break;

            case 3:
                if (descriptionText != null)
                {
                    descriptionText.text =
                        "Packing failures introduced! " +
                        "Some products now leave the " +
                        "Packing Machine unpacked. " +
                        "Grab them and place them " +
                        "in the Return Bin.";
                }

                if (warningText != null)
                {
                    warningText.text =
                        "20%\nUnpacked rate";
                }

                if (warningText2 != null)
                {
                    warningText2.text =
                        "40%\nDefect rate";
                }

                break;
        }

        popupOpen = true;
        popupPanel.SetActive(true);

        Debug.Log(
            $"[Difficulty Popup] Level {level} popup shown."
        );
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        popupOpen = false;

        if (pendingLevels.Count > 0)
            ShowNextPopup();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ClosePopup);
    }

    [ContextMenu("TEST - Show Level 2 Popup")]
    public void TestLevel2Popup()
    {
        ShowTestPopup(2);
    }

    [ContextMenu("TEST - Show Level 3 Popup")]
    public void TestLevel3Popup()
    {
        ShowTestPopup(3);
    }

    private void ShowTestPopup(int level)
    {
        pendingLevels.Clear();
        pendingLevels.Enqueue(level);

        // Test helpers are intended for Play Mode.
        // Don't interfere with a real unlock popup.
        if (popupOpen)
            ClosePopup();
        else
            ShowNextPopup();
    }
}
