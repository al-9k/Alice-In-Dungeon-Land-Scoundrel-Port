using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Change to UnityEngine.UI if using standard Text components

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject lastRunPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Last Run Text Fields")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text seedText;
    [SerializeField] private TMP_Text skipText;
    [SerializeField] private TMP_Text skullsText;
    [SerializeField] private TMP_Text goblinsText;
    [SerializeField] private TMP_Text heartsText;
    [SerializeField] private TMP_Text shieldsText;
    [SerializeField] private TMP_Text shatteredText;
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        // On menu load, check if run data exists for this session
        if (LastRunData.HasRunData)
        {
            PopulateStats();
            ShowLastRun();
        }
        else
        {
            ShowMainMenu();
        }
    }

    // Call this from the "Close / Continue" button on the Last Run panel
    public void CloseLastRunPanel()
    {
        ShowMainMenu();
    }

    private void ShowLastRun()
    {
        if (lastRunPanel != null) lastRunPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    private void ShowMainMenu()
    {
        if (lastRunPanel != null) lastRunPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void PopulateStats()
    {
        // Set Header
        if (string.IsNullOrEmpty(LastRunData.CauseOfDeath))
        {
            statusText.text = "<color=green>VICTORY!</color>";
        }
        else
        {
            statusText.text = $"<color=red>SLAIN BY: {LastRunData.CauseOfDeath.ToUpper()}";
        }

        // Format Stats
        seedText.text = $"{LastRunData.Seed}";
        skipText.text = $"{LastRunData.RoomsFled}";
        skullsText.text = $"{LastRunData.SkullTally}";
        goblinsText.text = $"{LastRunData.GoblinTally}";
        heartsText.text = $"{LastRunData.HeartsTally}";
        shieldsText.text = $"{LastRunData.ShieldsTally}";
        shatteredText.text = $"{LastRunData.ShieldsBroken}";

        // Format Time (mm:ss)
        int minutes = Mathf.FloorToInt(LastRunData.TimeSurvived / 60f);
        int seconds = Mathf.FloorToInt(LastRunData.TimeSurvived % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested");
        Application.Quit();
    }
}