
using UnityEngine;
using UnityEngine.SceneManagement;

public class FactoryReturnHome : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    private string homeSceneName = "FactoryVerse_Main";

    [Header("Confirmation UI")]
    [SerializeField]
    private GameObject confirmationPanel;

    public void OpenConfirmation()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    public void CancelReturn()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    public void ConfirmReturnHome()
    {
        Debug.Log(
            "[FactoryVerseMR] Returning to Home Scene."
        );

        SceneManager.LoadScene(homeSceneName);
    }
}
