using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ThemeSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform buttonContainer; // Parent transform of the scroll view content
    [SerializeField] private Button themeButtonPrefab; // Button prefab with TextMeshPro text
    [SerializeField] private TextMeshProUGUI selectedThemeTitleText; // Title display
    [SerializeField] private TextMeshProUGUI selectedThemeDescriptionText; // Description display
    [SerializeField] private Button playButton; // Button to start the game with the selected theme
    
    [Header("Theme Settings")]
    [SerializeField] private string questionBanksFolderPath = "question_banks"; // Folder containing theme files
    [SerializeField] private Color selectedButtonColor = new Color(0.8f, 0.8f, 1f); // Highlight color
    [SerializeField] private Color normalButtonColor = Color.white; // Default button color
    
    private List<ThemeData> availableThemes = new List<ThemeData>();
    private ThemeData selectedTheme;
    private Button selectedButton;
    
    // Structure to store theme data
    private class ThemeData
    {
        public string name;
        public string description;
        public TextAsset jsonFile;
        public bool isUserCreated;
        public string filePath;
    }
    
    private void Start()
    {
        LoadAvailableThemes();
        PopulateThemeButtons();
        
        // Disable play button until a theme is selected
        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }
    
    private void LoadAvailableThemes()
    {
        availableThemes.Clear();
        
        // 1. Load built-in themes from Resources folder
        TextAsset[] builtInThemeFiles = Resources.LoadAll<TextAsset>(questionBanksFolderPath);
        LoadThemesFromResources(builtInThemeFiles);
        
        // 2. Load user-created themes from persistent data path
        LoadUserCreatedThemes();
    }

    private void LoadThemesFromResources(TextAsset[] themeFiles)
    {
        foreach (TextAsset themeFile in themeFiles)
        {
            try
            {
                // Parse theme JSON
                Theme theme = JsonUtility.FromJson<Theme>(themeFile.text);
                
                if (theme != null)
                {
                    ThemeData themeData = new ThemeData
                    {
                        name = theme.themeName,
                        description = theme.description ?? "No description available",
                        jsonFile = themeFile,
                        isUserCreated = false
                    };
                    
                    availableThemes.Add(themeData);
                    Debug.Log($"Loaded built-in theme: {theme.themeName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading theme file {themeFile.name}: {e.Message}");
            }
        }
    }

    private void LoadUserCreatedThemes()
    {
        string userThemesPath = Path.Combine(Application.persistentDataPath, "user_themes");
        
        // Skip if directory doesn't exist
        if (!Directory.Exists(userThemesPath))
            return;
        
        // Get all JSON files in user themes directory
        string[] themeFiles = Directory.GetFiles(userThemesPath, "*.json");
        
        foreach (string filePath in themeFiles)
        {
            try
            {
                // Read file content
                string json = File.ReadAllText(filePath);
                Theme theme = JsonUtility.FromJson<Theme>(json);
                
                if (theme != null)
                {
                    // Create a TextAsset from the JSON (needed for ThemePassingManager)
                    TextAsset jsonAsset = new TextAsset(json);
                    
                    ThemeData themeData = new ThemeData
                    {
                        name = theme.themeName,
                        description = theme.description ?? "No description available",
                        jsonFile = jsonAsset,
                        isUserCreated = true,
                        filePath = filePath // Store original path for reference
                    };
                    
                    availableThemes.Add(themeData);
                    Debug.Log($"Loaded user theme: {theme.themeName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading user theme file {filePath}: {e.Message}");
            }
        }
    }
    
    private void PopulateThemeButtons()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create new theme buttons
        foreach (ThemeData theme in availableThemes)
        {
            Button themeButton = Instantiate(themeButtonPrefab, buttonContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = themeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = theme.name;
            }
            
            // Store theme data with the button
            themeButton.onClick.AddListener(() => SelectTheme(theme, themeButton));
        }
    }
    
    private void SelectTheme(ThemeData theme, Button clickedButton)
    {
        // Update selected theme
        selectedTheme = theme;
        
        // Update UI
        if (selectedThemeTitleText != null)
        {
            selectedThemeTitleText.text = theme.name;
        }
        
        if (selectedThemeDescriptionText != null)
        {
            selectedThemeDescriptionText.text = theme.description;
        }
        
        // Update button visuals
        if (selectedButton != null)
        {
            // Reset previous selected button color
            selectedButton.GetComponent<Image>().color = normalButtonColor;
        }
        
        // Highlight new selected button
        selectedButton = clickedButton;
        selectedButton.GetComponent<Image>().color = selectedButtonColor;
        
        // Enable play button
        if (playButton != null)
        {
            playButton.interactable = true;
        }
    }
    
    public void StartGameWithSelectedTheme()
    {
        if (selectedTheme != null)
        {
            // Pass the selected theme to the game scene
            if (selectedTheme.isUserCreated)
            {
                // For user themes, we need to give the file path instead
                ThemePassingManager.SetUserThemePath(selectedTheme.filePath);
            }
            else
            {
                // For built-in themes, use the TextAsset
                ThemePassingManager.SetSelectedTheme(selectedTheme.jsonFile);
            }
            
            // Load the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
        }
    }
}