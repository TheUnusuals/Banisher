using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public SettingsMenu settingsMenu;
    public string firstLevelName = "Level1";
    public void PlayGame() {
        SceneManager.LoadScene(firstLevelName);
    }

    public void Start() {
        settingsMenu.LoadSettings();
    }

    public void QuitGame() {
        Debug.Log("QUIT");
        Application.Quit();
    }
}