using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadUIManager : MonoBehaviour
{
    string mainMenuSceneName = "MainMenuScene";

    // 현재 씬 다시 시작
    public void RestartScene()
    {
        // 게임 멈춤 상태였으면 재개
        Time.timeScale = 1f;

        // 현재 활성화된 씬 다시 불러오기
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
