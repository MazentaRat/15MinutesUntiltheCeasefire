using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;

public enum EAudioMixerType { Master, BGM, SFX }

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance;

    [Header("UI References")]
    public GameObject optionUI;

    [Header("Audio Mixer Reference")]
    public AudioMixer audioMixer;
    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";

    private float bgmVolume = 0f;
    private float sfxVolume = 0f;
    private int selectedLanguage = 0;

    [Header("UI Elements")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public GameObject mainMenu;

    private bool isOptionOpen = false;

    string playSceneName = "PlayScene";
    string mainMenuSceneName = "MainMenuScene";
    string currentScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyLanguageSettings();

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener((value) => OnVolumeChange(EAudioMixerType.BGM, value));

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener((value) => OnVolumeChange(EAudioMixerType.SFX, value));

        if (optionUI != null)
            optionUI.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ToggleOptionUI();
    }

    public void OnSceneChanged(string sceneName)
    {
        currentScene = sceneName;

        if (mainMenu != null)
            mainMenu.SetActive(currentScene == mainMenuSceneName);

        // 옵션창은 씬 변경 시 무조건 닫힘
        if (optionUI != null)
            optionUI.SetActive(false);

        isOptionOpen = false;

        Time.timeScale = 1f;
    }


    // =============================================
    // 옵션창 열기/닫기 (기존 SetActive 방식)
    // =============================================
    public void ToggleOptionUI()
    {
        isOptionOpen = !isOptionOpen;

        if (optionUI != null)
            optionUI.SetActive(isOptionOpen);

        // 오직 메인메뉴씬일 때만 mainMenu UI를 켜거나 끌 수 있음
        if (currentScene == mainMenuSceneName)
        {
            if (mainMenu != null)
                mainMenu.SetActive(!isOptionOpen);
        }

        Time.timeScale = isOptionOpen ? 0f : 1f;
    }

    public void CloseOptionUI()
    {
        isOptionOpen = false;

        if (optionUI != null)
            optionUI.SetActive(false);

        if (currentScene == mainMenuSceneName && mainMenu != null)
            mainMenu.SetActive(true);

        Time.timeScale = 1f;
    }
    // =============================================
    // BGM / SFX
    // =============================================
    public void OnVolumeChange(EAudioMixerType type, float volume)
    {
        float safeVolume = (volume <= 0f) ? 0.0001f : volume;
        float dB = (volume <= 0f) ? -80f : Mathf.Log10(safeVolume) * 20f;

        switch (type)
        {
            case EAudioMixerType.BGM:
                audioMixer.SetFloat("BGM", dB);
                PlayerPrefs.SetFloat(BGM_KEY, volume);
                break;

            case EAudioMixerType.SFX:
                audioMixer.SetFloat("SFX", dB);
                PlayerPrefs.SetFloat(SFX_KEY, volume);
                break;
        }

        PlayerPrefs.Save();
    }

    // =============================================
    // 언어 설정
    // =============================================
    public void SetLanguage(int index)
    {
        selectedLanguage = Mathf.Clamp(index, 0, 1);
        StartCoroutine(ChangeLocaleCoroutine(selectedLanguage));

        PlayerPrefs.SetInt("Language", selectedLanguage);
        PlayerPrefs.Save();
    }

    private System.Collections.IEnumerator ChangeLocaleCoroutine(int localeIndex)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
    }

    private void ApplyLanguageSettings()
    {
        StartCoroutine(ChangeLocaleCoroutine(selectedLanguage));
    }

    public void SetLanguageEnglish() => SetLanguage(0);
    public void SetLanguageKorean() => SetLanguage(1);

    // =============================================
    // 씬 이동
    // =============================================
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GameStart()
    {
        Time.timeScale = 1f;

        if (mainMenu != null)
            mainMenu.SetActive(false);

        SceneManager.LoadScene(playSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =============================================
    // 저장값
    // =============================================
    private void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        OnVolumeChange(EAudioMixerType.BGM, bgmVolume);
        OnVolumeChange(EAudioMixerType.SFX, sfxVolume);

        selectedLanguage = PlayerPrefs.GetInt("Language", 0);
    }
}
