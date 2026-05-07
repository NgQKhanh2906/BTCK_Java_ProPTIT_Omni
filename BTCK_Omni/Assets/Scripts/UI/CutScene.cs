using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Cutscene : MonoBehaviour
{
    public TMP_Text uiText;
    public AudioSource audioSrc;
    public string[] dialogueLines;
    public AudioClip[] dialogueSounds;
    public float typingSpeed;
    public string nextSceneName;
    public RectTransform btnSkip;

    private int currentLineIndex;
    private bool isTyping;
    private bool isCutsceneStarted;
    private bool wasPaused;

    void Start()
    {
        currentLineIndex = 0;
        isTyping = false;
        isCutsceneStarted = false;
        wasPaused = false;
        uiText.text = "";
    }

    void Update()
    {
        CheckAudioPauseState();

        if (Input.anyKeyDown && Time.timeScale != 0f)
        {
            if (Input.GetMouseButtonDown(0) && btnSkip != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(btnSkip, Input.mousePosition, null))
                {
                    return;
                }
            }

            if (!isCutsceneStarted)
            {
                isCutsceneStarted = true;
                StartCoroutine(ShowText());
            }
            else if (!isTyping)
            {
                NextLine();
            }
        }
    }

    private void CheckAudioPauseState()
    {
        bool isPaused = (Time.timeScale == 0f);

        if (isPaused && !wasPaused)
        {
            audioSrc.Pause();
            wasPaused = true;
        }
        else if (!isPaused && wasPaused)
        {
            audioSrc.UnPause();
            wasPaused = false;
        }
    }

    IEnumerator ShowText()
    {
        isTyping = true;
        uiText.text = "";

        if (currentLineIndex < dialogueSounds.Length && dialogueSounds[currentLineIndex] != null)
        {
            audioSrc.clip = dialogueSounds[currentLineIndex];
            audioSrc.Play();
        }

        foreach (char letter in dialogueLines[currentLineIndex].ToCharArray())
        {
            uiText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            StartCoroutine(ShowText());
        }
        else
        {
            GameManager.Instance.LoadScene(nextSceneName);
        }
    }

    public void Skip()
    {
        GameManager.Instance.LoadScene(nextSceneName);
    }
}