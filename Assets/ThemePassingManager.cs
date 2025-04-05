using UnityEngine;
using System.IO;

// Static class to handle passing theme data between scenes
public static class ThemePassingManager
{
    private static TextAsset selectedTheme;
    private static string userThemePath;
    private static bool isUserTheme = false;
    
    public static void SetSelectedTheme(TextAsset theme)
    {
        selectedTheme = theme;
        isUserTheme = false;
    }
    
    public static void SetUserThemePath(string path)
    {
        userThemePath = path;
        isUserTheme = true;
    }
    
    public static TextAsset GetSelectedTheme()
    {
        if (isUserTheme)
        {
            // Load the user theme from file and create a TextAsset
            try
            {
                string json = File.ReadAllText(userThemePath);
                return new TextAsset(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading user theme: {e.Message}");
                return null;
            }
        }
        else
        {
            return selectedTheme;
        }
    }
    
    public static bool IsUserTheme()
    {
        return isUserTheme;
    }
    
    public static string GetUserThemePath()
    {
        return userThemePath;
    }
}