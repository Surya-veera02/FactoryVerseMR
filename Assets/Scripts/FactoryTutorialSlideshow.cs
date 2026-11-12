
using UnityEngine;
using UnityEngine.UI;

public class FactoryTutorialSlideshow : MonoBehaviour
{
    [Header("Tutorial Images")]
    [SerializeField]
    private Sprite[] tutorialPages = new Sprite[5];

    [Header("UI References")]
    [SerializeField]
    private Image tutorialImage;

    [SerializeField]
    private Button previousButton;

    [SerializeField]
    private Button nextButton;

    private int currentPage = 0;

    private void OnEnable()
    {
        currentPage = 0;
        ShowPage();
    }

    public void NextPage()
    {
        if (tutorialPages == null ||
            tutorialPages.Length == 0)
            return;

        currentPage = Mathf.Min(
            currentPage + 1,
            tutorialPages.Length - 1
        );

        ShowPage();
    }

    public void PreviousPage()
    {
        currentPage = Mathf.Max(currentPage - 1, 0);
        ShowPage();
    }

    private void ShowPage()
    {
        if (tutorialPages == null ||
            tutorialPages.Length == 0)
            return;

        if (tutorialImage != null)
        {
            tutorialImage.sprite = tutorialPages[currentPage];
            tutorialImage.preserveAspect = true;
        }

        if (previousButton != null)
        {
            previousButton.interactable =
                currentPage > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable =
                currentPage < tutorialPages.Length - 1;
        }

        Debug.Log(
            "[FactoryVerseMR] Tutorial page: " +
            (currentPage + 1)
        );
    }
}
