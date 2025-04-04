using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject themeSelectionScreen;
    
    [Header("Theme Selection")]
    [SerializeField] private Transform themeButtonsContainer;
    [SerializeField] private Button themeButtonPrefab;
    [SerializeField] private TextAsset[] themeFiles;
    
    [Header("Navigation")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button backButton;
    
    private GameObject currentActiveScreen;
    
    private void Awake()
    {
        // Set up button listeners
        playButton.onClick.AddListener(ShowThemeSelection);
        
        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
            
        // Initialize with main menu active
        currentActiveScreen = mainMenuScreen;
        mainMenuScreen.SetActive(true);
        themeSelectionScreen.SetActive(false);
    }
    
    private void Start()
    {
        // Populate theme selection screen
        PopulateThemeButtons();
    }
    
    private void Update()
    {
        // Handle Android back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentActiveScreen == themeSelectionScreen)
            {
                ShowMainMenu();
            }
            else if (currentActiveScreen == mainMenuScreen)
            {
                // Quit application if on main menu
                Application.Quit();
            }
        }
    }
    
    public void ShowMainMenu()
    {
        themeSelectionScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
        currentActiveScreen = mainMenuScreen;
    }
    
    public void ShowThemeSelection()
    {
        mainMenuScreen.SetActive(false);
        themeSelectionScreen.SetActive(true);
        currentActiveScreen = themeSelectionScreen;
    }
    
    private void PopulateThemeButtons()
    {
        // Clear existing buttons if any
        foreach (Transform child in themeButtonsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new buttons for each theme
        foreach (TextAsset themeFile in themeFiles)
        {
            if (themeFile == null) continue;
            
            try {
                Theme theme = JsonUtility.FromJson<Theme>(themeFile.text);
                if (theme != null)
                {
                    Button themeButton = Instantiate(themeButtonPrefab, themeButtonsContainer);
                    
                    // Set button text to theme name
                    TMPro.TextMeshProUGUI buttonText = themeButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = theme.themeName;
                    }
                    
                    // Set button click to start game with theme
                    themeButton.onClick.AddListener(() => StartGameWithTheme(themeFile));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing theme file {themeFile.name}: {e.Message}");
            }
        }
    }
    
    private void StartGameWithTheme(TextAsset themeFile)
    {
        // Set the theme and load game scene
        //ThemePassingManager.SetSelectedTheme(themeFile);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
    }
}