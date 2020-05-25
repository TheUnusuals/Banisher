using System;
using System.Collections.Generic;
using TheUnusuals.Banisher.BanisherPlayground.Scripts.Energy;
using TheUnusuals.Banisher.ExtreStuff;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class Menu : MonoBehaviour
{
    public float totalLevelTime = 10.0f;
    public bool pauseGame;
    public bool gameEnded;
    public string mainMenuLevelName = "MainMenu";
    public string nextLevelName = "Level2";

    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject nextLevelMenu;
    public TextMeshProUGUI timeLeftText;

    public AudioSource levelMusic;

    public AudioSource pauseMusic;

    public SteamVR_Action_Boolean pauseButton;

    public List<Behaviour> enableComponentsInMenus = new List<Behaviour>();

    [SerializeField] private VisEnergy visEnergy;

    [SerializeField] private BanishManager banishManager;
    [SerializeField] private NinjaManager ninjaManager;

    private float previousTimeScale;

    public float TimeLeft => totalLevelTime;

    private void Awake()
    {
        if (!banishManager) banishManager = BanishManager.GetInstance();
        if (!ninjaManager) ninjaManager = NinjaManager.GetInstance();
    }

    void Start()
    {
        Time.timeScale = 1f;
        pauseGame = false;
        gameEnded = false;

        DisableComponents();

        visEnergy.OnChange.AddListener(OnChangeVisEnergy);
        ninjaManager.OnNinjasChange.AddListener(OnNinjasChange);
    }

    private void OnNinjasChange()
    {
        if (ninjaManager.CurrentNinjasAlive <= 0)
        {
            ShowLevelCompletedMenu();
        }
    }

    private void OnChangeVisEnergy(VisEnergy visEnergy, float previousEnergy)
    {
        if (visEnergy.Energy <= 0)
        {
            ShowGameOverMenu();
        }
    }

    void Update()
    {
        if (totalLevelTime >= 0)
        {
            totalLevelTime -= banishManager.AdjustedDeltaTime;
        }
        else
        {
            ShowGameOverMenu();
        }

        if (!gameEnded && (Input.GetKeyDown(KeyCode.Escape) || pauseButton.stateDown))
        {
            pauseGame = !pauseGame;
            if (pauseGame)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowLevelCompletedMenu();
        }

        if (pauseGame || gameEnded)
        {
            Physics.SyncTransforms();
        }
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        pauseGame = true;
        PauseTimeScale();
        EnableComponents();
        PauseLevelMusic();
        pauseMusic.Play();
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        pauseGame = false;
        ResumeTimeScale();
        DisableComponents();
        ResumeLevelMusic();
        pauseMusic.Stop();
    }

    public void GoToMainMenu()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuLevelName);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        pauseGame = false;
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowLevelCompletedMenu()
    {
        nextLevelMenu.SetActive(true);
        gameEnded = true;
        PauseTimeScale();
        timeLeftText.text = $"Time left: {totalLevelTime}";
        EnableComponents();
        PauseLevelMusic();
    }

    public void NextLevel()
    {
        nextLevelMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }

    public void ShowGameOverMenu()
    {
        gameEnded = true;
        gameOverMenu.SetActive(true);
        PauseTimeScale();
        EnableComponents();
        PauseLevelMusic();
    }

    private void PauseLevelMusic()
    {
        levelMusic.Pause();
    }

    private void ResumeLevelMusic()
    {
        levelMusic.Play();
    }

    private void EnableComponents()
    {
        foreach (Behaviour component in enableComponentsInMenus)
        {
            component.enabled = true;
        }
    }

    private void DisableComponents()
    {
        foreach (Behaviour component in enableComponentsInMenus)
        {
            component.enabled = false;
        }
    }

    private void PauseTimeScale()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    private void ResumeTimeScale()
    {
        Time.timeScale = previousTimeScale;
    }
}