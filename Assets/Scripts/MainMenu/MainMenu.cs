using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton;

    [Header("Options Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Animation")]
    [SerializeField] private float buttonAnimationSpeed = 0.1f;
    [SerializeField] private float panelFadeSpeed = 0.3f;
    
    [Header("Game Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";
    
    private CanvasGroup mainMenuCanvasGroup;
    private CanvasGroup optionsMenuCanvasGroup;
    
    private Resolution[] resolutions;

    private void Awake()
    {
        // Get canvas groups for fading effects
        mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
        if (mainMenuCanvasGroup == null)
            mainMenuCanvasGroup = mainMenuPanel.AddComponent<CanvasGroup>();
            
        optionsMenuCanvasGroup = optionsMenuPanel.GetComponent<CanvasGroup>();
        if (optionsMenuCanvasGroup == null)
            optionsMenuCanvasGroup = optionsMenuPanel.AddComponent<CanvasGroup>();
            
        // Initially hide options panel
        optionsMenuPanel.SetActive(false);
        
        // Setup button listeners
        startButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(OpenOptions);
        exitButton.onClick.AddListener(ExitGame);
        backButton.onClick.AddListener(CloseOptions);
        
        // Setup resolution dropdown
        SetupResolutionDropdown();
        
        // Load saved settings
        LoadSettings();
    }
    
    private void Start()
    {
        // Animate buttons
        StartCoroutine(AnimateButtons());
    }
    
    private IEnumerator AnimateButtons()
    {
        // Animate each button with a scale effect
        foreach (var button in new Button[] { startButton, optionsButton, exitButton })
        {
            button.transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(0.1f);
            
            float time = 0;
            while (time < 1)
            {
                time += Time.deltaTime / buttonAnimationSpeed;
                button.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, time));
                yield return null;
            }
            
            button.transform.localScale = Vector3.one;
        }
    }
    
    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            
            int currentResolutionIndex = 0;
            System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
            
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                
                if (resolutions[i].width == Screen.currentResolution.width && 
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
            
            resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(); });
        }
    }
    
    private void LoadSettings()
    {
        // Load volume settings
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            
        // Load fullscreen setting
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        // Set up listeners for saving settings
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(delegate { SaveVolumeSettings(); });
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(delegate { SaveVolumeSettings(); });
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(delegate { SetFullscreen(); });
    }
    
    private void SaveVolumeSettings()
    {
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            
        PlayerPrefs.Save();
        
        // Here you would also update your audio mixer if you have one
        // Example: audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolumeSlider.value) * 20);
    }
    
    private void SetFullscreen()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void SetResolution()
    {
        Resolution resolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.Save();
    }
    
    public void StartGame()
    {
        StartCoroutine(FadeAndLoadScene());
    }
    
    private IEnumerator FadeAndLoadScene()
    {
        // Fade out the main menu
        float fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            mainMenuCanvasGroup.alpha = Mathf.Lerp(1, 0, fadeTime);
            yield return null;
        }
        
        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void OpenOptions()
    {
        StartCoroutine(FadeToOptions());
    }
    
    private IEnumerator FadeToOptions()
    {
        // Fade out main menu
        float fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            mainMenuCanvasGroup.alpha = Mathf.Lerp(1, 0, fadeTime);
            yield return null;
        }
        
        // Switch panels
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
        optionsMenuCanvasGroup.alpha = 0;
        
        // Fade in options menu
        fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            optionsMenuCanvasGroup.alpha = Mathf.Lerp(0, 1, fadeTime);
            yield return null;
        }
    }
    
    public void CloseOptions()
    {
        StartCoroutine(FadeToMainMenu());
    }
    
    private IEnumerator FadeToMainMenu()
    {
        // Fade out options menu
        float fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            optionsMenuCanvasGroup.alpha = Mathf.Lerp(1, 0, fadeTime);
            yield return null;
        }
        
        // Switch panels
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainMenuCanvasGroup.alpha = 0;
        
        // Fade in main menu
        fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            mainMenuCanvasGroup.alpha = Mathf.Lerp(0, 1, fadeTime);
            yield return null;
        }
    }
    
    public void ExitGame()
    {
        StartCoroutine(FadeAndQuit());
    }
    
    private IEnumerator FadeAndQuit()
    {
        // Fade out the main menu
        float fadeTime = 0;
        while (fadeTime < 1)
        {
            fadeTime += Time.deltaTime / panelFadeSpeed;
            mainMenuCanvasGroup.alpha = Mathf.Lerp(1, 0, fadeTime);
            yield return null;
        }
        
        // Quit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 