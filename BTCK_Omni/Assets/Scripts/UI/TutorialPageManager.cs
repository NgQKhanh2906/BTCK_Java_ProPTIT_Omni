using UnityEngine;
using TMPro;

public class TutorialPagination : MonoBehaviour
{
    [Header("Tutorial Pagination")]
    public GameObject[] tutorialPages; 
    public TextMeshProUGUI pageIndicatorText; 
    private int currentPageIndex = 0;
    private void OnEnable()
    {
        currentPageIndex = 0; 
        UpdatePageDisplay();
    }

    public void NextPage()
    {
        if (currentPageIndex < tutorialPages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        for (int i = 0; i < tutorialPages.Length; i++)
        {
            tutorialPages[i].SetActive(i == currentPageIndex);
        }
        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = (currentPageIndex + 1) + "/" + tutorialPages.Length;
        }
    }
}