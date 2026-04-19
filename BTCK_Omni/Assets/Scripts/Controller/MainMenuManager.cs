using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;
public class MenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject tutorialPanel; 
   
    [Header("Tutorial Pagination")]
    public GameObject[] tutorialPages; 
    public TextMeshProUGUI pageIndicatorText; 
    private int currentPageIndex = 0;
    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenTutorial()
    {
        tutorialPanel.SetActive(true);
        currentPageIndex = 0; 
        UpdatePageDisplay();
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Map1 1"); 
    }

    public void QuitGame()
    {
        Debug.Log("Đang thoát game..."); 
        Application.Quit();
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