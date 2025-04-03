using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Question
{
    public string answer;
    public List<string> clues; // Optional: can be used later for hints
}

[System.Serializable]
public class Theme
{
    public string themeName;
    public List<Question> questions;
}

[System.Serializable]
public class QuestionDatabase
{
    public List<Theme> themes;
}

public class QuestionDataManager : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;
    private QuestionDatabase database;
    
    private void Awake()
    {
        LoadQuestionDatabase();
    }
    
    private void LoadQuestionDatabase()
    {
        if (jsonFile != null)
        {
            database = JsonUtility.FromJson<QuestionDatabase>(jsonFile.text);
        }
        else
        {
            Debug.LogError("JSON file not assigned!");
        }
    }
    
    public List<Question> GetQuestionsForTheme(string themeName)
    {
        foreach (Theme theme in database.themes)
        {
            if (theme.themeName == themeName)
            {
                return theme.questions;
            }
        }
        
        Debug.LogWarning($"Theme '{themeName}' not found!");
        return null;
    }
    
    public List<string> GetAllThemeNames()
    {
        List<string> themeNames = new List<string>();
        
        foreach (Theme theme in database.themes)
        {
            themeNames.Add(theme.themeName);
        }
        
        return themeNames;
    }
}