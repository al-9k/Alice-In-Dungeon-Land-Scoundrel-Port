using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Elements")]
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public GameObject resumeButton;
    public Slider sfxScroller;
    public bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
        resumeButton.SetActive(false);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        pauseButton.SetActive(false);
        resumeButton.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        pauseButton.SetActive(true);
        resumeButton.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
