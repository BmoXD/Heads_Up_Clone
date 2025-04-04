using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

// Static class to handle passing theme data between scenes
public static class ThemePassingManager
{
    private static TextAsset selectedThemeJson;

    public static void SetSelectedTheme(TextAsset themeJson)
    {
        selectedThemeJson = themeJson;
    }

    public static TextAsset GetSelectedTheme()
    {
        return selectedThemeJson;
    }
}