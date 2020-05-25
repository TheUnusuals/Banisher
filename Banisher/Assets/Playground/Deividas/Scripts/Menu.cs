using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    public float totalLevelTime = 10.0f;
    public bool pauseGame;
    public bool gameEnded;
    public string mainMenuLevelName = "MainMenu";
    public string nextLevelName = "Level2";
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject nextLevelMenu;
    public TextMeshProUGUI timeLeftText;

    void Start() {
        Time.timeScale = 1f;
        pauseGame = false;
        gameEnded = false;
    }

    void Update() {
        if (totalLevelTime >= 0) {
            totalLevelTime -= Time.deltaTime;
        }
        else {
            ShowGameOverMenu();
        }

        if (!gameEnded && Input.GetKeyDown(KeyCode.Escape)) {
            pauseGame = !pauseGame;
            if (pauseGame) {
                Pause();
            }
            else {
                Resume();
            }
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            ShowLevelCompletedMenu();
        }
    }

    public void Pause() {
        pauseMenu.SetActive(true);
        pauseGame = true;
        Time.timeScale = 0f;
    }

    public void Resume() {
        pauseMenu.SetActive(false);
        pauseGame = false;
        Time.timeScale = 1f;
    }

    public void GoToMainMenu() {
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(mainMenuLevelName);
        Time.timeScale = 1f;
    }

    public void Restart() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        pauseGame = false;
        gameEnded = false;
    }

    public void ShowLevelCompletedMenu()
    {
        nextLevelMenu.SetActive(true);
        gameEnded = true;
        Time.timeScale = 0f;
        timeLeftText.text = $"Time left: {totalLevelTime}";
    }

    public void NextLevel() {
        nextLevelMenu.SetActive(false);
        SceneManager.LoadScene(nextLevelName);
        Time.timeScale = 1f;
    }
    
    public void ShowGameOverMenu() {
        gameEnded = true;
        gameOverMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    //void Start() {
    //    StartCoroutine(StartTimer());
    //}

    //IEnumerator StartTimer() {
    //    yield return new WaitForSeconds(5);
    //    nextLevelMenu.SetActive(true);
    //}
}