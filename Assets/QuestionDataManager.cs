using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string answer;
    public List<string> clues;
}

[System.Serializable]
public class Theme
{
    public string themeName;
    public string description;
    public List<Question> questions;
}

public class QuestionDataManager : MonoBehaviour
{
    [SerializeField] private TextAsset defaultThemeFile; // Optional default theme for debugging
    
    private Theme currentTheme;
    private string currentThemeName;
    
    private void Awake()
    {
        // If default theme is assigned, load it immediately
        if (defaultThemeFile != null)
        {
            LoadTheme(defaultThemeFile);
        }
    }
    
    /// <summary>
    /// Loads a theme from a TextAsset (JSON file)
    /// </summary>
    public void LoadTheme(TextAsset jsonFile)
    {
        if (jsonFile == null)
        {
            Debug.LogError("Theme JSON file is null!");
            return;
        }
        
        try
        {
            currentTheme = JsonUtility.FromJson<Theme>(jsonFile.text);
            
            if (currentTheme != null)
            {
                currentThemeName = currentTheme.themeName;
                Debug.Log($"Loaded theme: {currentThemeName} with {currentTheme.questions.Count} questions");
            }
            else
            {
                Debug.LogError($"Failed to parse theme from file: {jsonFile.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading theme file {jsonFile.name}: {e.Message}");
            currentTheme = null;
        }
    }
    
    /// <summary>
    /// Gets the questions for the currently loaded theme
    /// </summary>
    public List<Question> GetQuestionsForTheme(string themeName)
    {
        // Check if this is requesting the currently loaded theme
        if (currentTheme != null && currentTheme.themeName == themeName)
        {
            return currentTheme.questions;
        }
        
        Debug.LogWarning($"Theme '{themeName}' is not currently loaded!");
        return null;
    }
    
    /// <summary>
    /// Gets the questions for the currently loaded theme
    /// </summary>
    public List<Question> GetCurrentThemeQuestions()
    {
        if (currentTheme != null)
        {
            return currentTheme.questions;
        }
        
        Debug.LogWarning("No theme is currently loaded!");
        return null;
    }
    
    /// <summary>
    /// Gets the name of the currently loaded theme
    /// </summary>
    public string GetCurrentThemeName()
    {
        return currentThemeName;
    }
    
    /// <summary>
    /// Checks if the QuestionDataManager has a theme loaded
    /// </summary>
    public bool HasLoadedTheme()
    {
        return currentTheme != null;
    }
}