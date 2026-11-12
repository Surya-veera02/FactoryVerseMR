
using UnityEngine;
using UnityEngine.SceneManagement;

public class FactoryHomeMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    private string gameplaySceneName = "FactoryVerse_Game";

    [Header("Home UI")]
    [SerializeField]
    private GameObject homePanel;

    [SerializeField]
    private GameObject howToPlayPanel;

    [SerializeField]
    private GameObject aboutPanel;

    public void StartExperience()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenHowToPlay()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void OpenAbout()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (aboutPanel != null)
            aboutPanel.SetActive(true);
    }

    public void ReturnToHomeMenu()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (homePanel != null)
            homePanel.SetActive(true);
    }

    public void QuitExperience()
    {
        Debug.Log("[FactoryVerseMR] Quit requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
