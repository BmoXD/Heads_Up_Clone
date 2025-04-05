using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ThemeBankCreationManager : MonoBehaviour
{
    [Header("Theme Bank UI References")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button themeButtonPrefab;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private TMP_InputField descriptionInputField;
    [SerializeField] private Button createButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    
    [Header("Theme Editor UI References")]
    [SerializeField] private GameObject themeBankPanel; // Parent panel for theme bank UI
    [SerializeField] private GameObject themeEditorPanel; // Parent panel for theme editor UI
    [SerializeField] private Transform questionButtonContainer; // Container for question buttons
    [SerializeField] private Button questionButtonPrefab; // Prefab with 2 TMPro texts
    [SerializeField] private TMP_InputField questionInputField; // Input for question editing
    [SerializeField] private Transform hintFieldsContainer; // Container for hint input fields
    [SerializeField] private TMP_InputField hintInputFieldPrefab; // Prefab for hint inputs
    [SerializeField] private Button createQuestionButton;
    [SerializeField] private Button saveQuestionButton;
    [SerializeField] private Button deleteQuestionButton;
    [SerializeField] private Button addHintButton;
    [SerializeField] private Button backToThemesButton;
    
    [Header("Theme Settings")]
    [SerializeField] private string userThemesFolder = "user_themes";
    [SerializeField] private Color selectedButtonColor = new Color(0.8f, 0.8f, 1f);
    [SerializeField] private Color normalButtonColor = Color.white;
    
    private List<UserThemeData> userThemes = new List<UserThemeData>();
    private UserThemeData selectedTheme;
    private Button selectedButton;
    
    // Question editor state
    private Question selectedQuestion;
    private Button selectedQuestionButton;
    private int selectedQuestionIndex = -1;
    private List<TMP_InputField> currentHintFields = new List<TMP_InputField>();
    
    // Path where user themes will be stored
    private string userThemesPath;
    
    // Class to hold theme data and file info
    private class UserThemeData
    {
        public string name;
        public string description;
        public string filePath;
        public List<Question> questions = new List<Question>();
    }
    
    private void Awake()
    {
        // Set up the path for user themes
        userThemesPath = Path.Combine(Application.persistentDataPath, userThemesFolder);
        
        // Create the directory if it doesn't exist
        if (!Directory.Exists(userThemesPath))
        {
            Directory.CreateDirectory(userThemesPath);
        }
    }
    
    private void Start()
    {
        // Set up button interactivity
        UpdateButtonStates();
        
        // Load existing themes
        LoadUserThemes();
        PopulateThemeButtons();
        
        // Set up theme bank button click events
        createButton.onClick.AddListener(CreateNewTheme);
        saveButton.onClick.AddListener(SaveSelectedTheme);
        deleteButton.onClick.AddListener(DeleteSelectedTheme);
        editButton.onClick.AddListener(ShowThemeEditor);
        
        // Set up theme editor button click events
        createQuestionButton.onClick.AddListener(CreateNewQuestion);
        saveQuestionButton.onClick.AddListener(SaveSelectedQuestion);
        deleteQuestionButton.onClick.AddListener(DeleteSelectedQuestion);
        addHintButton.onClick.AddListener(() => AddHintField());
        backToThemesButton.onClick.AddListener(ShowThemeBank);
        
        // Set up input field validation
        titleInputField.onValueChanged.AddListener(_ => ValidateInputs());
        descriptionInputField.onValueChanged.AddListener(_ => ValidateInputs());
        
        // Start with theme bank panel active
        ShowThemeBank();
    }

    // --- Theme Bank Functions ---
    
    private void LoadUserThemes()
    {
        userThemes.Clear();
        
        // Get all JSON files in the user themes directory
        string[] themeFiles = Directory.GetFiles(userThemesPath, "*.json");
        
        foreach (string filePath in themeFiles)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                Theme theme = JsonUtility.FromJson<Theme>(json);
                
                // Only add themes marked as visible in bank editor
                if (theme != null && theme.visibleInBankEditor)
                {
                    UserThemeData themeData = new UserThemeData
                    {
                        name = theme.themeName,
                        description = theme.description,
                        filePath = filePath,
                        questions = theme.questions ?? new List<Question>()
                    };
                    
                    userThemes.Add(themeData);
                    Debug.Log($"Loaded user theme: {theme.themeName}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading theme file {filePath}: {e.Message}");
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
        
        // Create buttons for each user theme
        foreach (UserThemeData theme in userThemes)
        {
            Button themeButton = Instantiate(themeButtonPrefab, buttonContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = themeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = theme.name;
            }
            
            // Add click event
            themeButton.onClick.AddListener(() => SelectTheme(theme, themeButton));
        }
    }
    
    private void SelectTheme(UserThemeData theme, Button clickedButton)
    {
        // Update selected theme
        selectedTheme = theme;
        
        // Update input fields
        titleInputField.text = theme.name;
        descriptionInputField.text = theme.description;
        
        // Update button visuals
        if (selectedButton != null)
        {
            selectedButton.GetComponent<Image>().color = normalButtonColor;
        }
        
        selectedButton = clickedButton;
        selectedButton.GetComponent<Image>().color = selectedButtonColor;
        
        // Update button states
        UpdateButtonStates();
    }
    
    private void CreateNewTheme()
    {
        string title = titleInputField.text.Trim();
        string description = descriptionInputField.text.Trim();
        
        // Check if inputs are valid
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Title and description are required to create a theme.");
            return;
        }
        
        // Create a new theme
        Theme newTheme = new Theme
        {
            themeName = title,
            description = description,
            visibleInBankEditor = true,
            questions = new List<Question>()
        };
        
        // Generate a safe filename based on title
        string safeFileName = GenerateSafeFileName(title);
        string filePath = Path.Combine(userThemesPath, $"{safeFileName}.json");
        
        // Check if file already exists
        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(userThemesPath, $"{safeFileName}_{counter}.json");
            counter++;
        }
        
        // Save theme to file
        string json = JsonUtility.ToJson(newTheme, true);
        File.WriteAllText(filePath, json);
        
        // Create user theme data
        UserThemeData themeData = new UserThemeData
        {
            name = title,
            description = description,
            filePath = filePath,
            questions = new List<Question>()
        };
        
        userThemes.Add(themeData);
        
        // Add button for the new theme
        Button themeButton = Instantiate(themeButtonPrefab, buttonContainer);
        
        // Set button text
        TextMeshProUGUI buttonText = themeButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = title;
        }
        
        // Add click event
        themeButton.onClick.AddListener(() => SelectTheme(themeData, themeButton));
        
        // Select the new theme
        SelectTheme(themeData, themeButton);
        
        Debug.Log($"Created new theme: {title} at {filePath}");
    }
    
    private void SaveSelectedTheme()
    {
        if (selectedTheme == null) return;
        
        string title = titleInputField.text.Trim();
        string description = descriptionInputField.text.Trim();
        
        // Check if inputs are valid
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Title and description are required to save a theme.");
            return;
        }
        
        // Update theme data
        selectedTheme.name = title;
        selectedTheme.description = description;
        
        // Load current theme from file
        string json = File.ReadAllText(selectedTheme.filePath);
        Theme theme = JsonUtility.FromJson<Theme>(json);
        
        // Update theme properties
        theme.themeName = title;
        theme.description = description;
        
        // Save updated theme
        json = JsonUtility.ToJson(theme, true);
        File.WriteAllText(selectedTheme.filePath, json);
        
        // Update button text
        if (selectedButton != null)
        {
            TextMeshProUGUI buttonText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = title;
            }
        }
        
        Debug.Log($"Saved theme: {title}");
    }
    
    public void DeleteSelectedTheme()
    {
        if (selectedTheme == null) return;
        
        try
        {
            // Delete the file
            File.Delete(selectedTheme.filePath);
            
            // Remove from our list
            userThemes.Remove(selectedTheme);
            
            // Destroy the button
            if (selectedButton != null)
            {
                Destroy(selectedButton.gameObject);
            }
            
            // Clear selection
            selectedTheme = null;
            selectedButton = null;
            
            // Clear input fields
            titleInputField.text = "";
            descriptionInputField.text = "";
            
            // Update button states
            UpdateButtonStates();
            
            Debug.Log("Theme deleted successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting theme: {e.Message}");
        }
    }
    
    private void ValidateInputs()
    {
        bool inputsValid = !string.IsNullOrEmpty(titleInputField.text.Trim()) && 
                          !string.IsNullOrEmpty(descriptionInputField.text.Trim());
        
        createButton.interactable = inputsValid;
        saveButton.interactable = inputsValid && selectedTheme != null;
    }
    
    private void UpdateButtonStates()
    {
        // Enable/disable save, edit and delete buttons based on selection
        bool hasSelection = selectedTheme != null;
        bool validInputs = !string.IsNullOrEmpty(titleInputField.text.Trim()) && 
                        !string.IsNullOrEmpty(descriptionInputField.text.Trim());
        
        saveButton.interactable = hasSelection && validInputs;
        editButton.interactable = hasSelection;
        deleteButton.interactable = hasSelection;
    }
    
    private string GenerateSafeFileName(string name)
    {
        // Replace invalid characters with underscores
        string invalidChars = new string(Path.GetInvalidFileNameChars());
        string safeName = name;
        
        foreach (char c in invalidChars)
        {
            safeName = safeName.Replace(c, '_');
        }
        
        return safeName;
    }
    
    // --- UI Navigation ---
    
    public void ShowThemeBank()
    {
        // Switch to theme bank panel
        themeBankPanel.SetActive(true);
        themeEditorPanel.SetActive(false);
        
        // Clear selection on editor
        ClearQuestionSelection();
    }
    
    public void ShowThemeEditor()
    {
        if (selectedTheme == null) return;
        
        // Switch to theme editor panel
        themeBankPanel.SetActive(false);
        themeEditorPanel.SetActive(true);
        
        // Reload theme data to ensure it's up to date
        ReloadSelectedThemeData();
        
        // Populate questions for this theme
        PopulateQuestionButtons();
        
        // Clear question selection initially
        ClearQuestionSelection();
        
        // Update button states
        UpdateQuestionButtonStates();
    }
    
    private void ReloadSelectedThemeData()
    {
        try
        {
            // Load from file to get the latest data
            string json = File.ReadAllText(selectedTheme.filePath);
            Theme theme = JsonUtility.FromJson<Theme>(json);
            
            if (theme != null)
            {
                // Update questions list
                selectedTheme.questions = theme.questions ?? new List<Question>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reloading theme data: {e.Message}");
        }
    }
    
    // --- Theme Editor Functions ---
    
    private void PopulateQuestionButtons()
    {
        // Clear existing buttons
        foreach (Transform child in questionButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each question
        for (int i = 0; i < selectedTheme.questions.Count; i++)
        {
            Question question = selectedTheme.questions[i];
            int questionIndex = i; // Capture the index for the lambda
            
            Button questionButton = Instantiate(questionButtonPrefab, questionButtonContainer);
            
            // Set question text
            TextMeshProUGUI questionText = questionButton.GetComponentsInChildren<TextMeshProUGUI>()[0];
            if (questionText != null)
            {
                questionText.text = question.answer;
            }
            
            // Set hints preview text (first 3 hints or fewer)
            TextMeshProUGUI hintsText = questionButton.GetComponentsInChildren<TextMeshProUGUI>()[1];
            if (hintsText != null)
            {
                string hintPreview = "";
                if (question.clues != null && question.clues.Count > 0)
                {
                    int hintCount = Mathf.Min(question.clues.Count, 3);
                    for (int j = 0; j < hintCount; j++)
                    {
                        hintPreview += question.clues[j];
                        if (j < hintCount - 1)
                            hintPreview += ", ";
                    }
                }
                else
                {
                    hintPreview = "No hints";
                }
                hintsText.text = hintPreview;
            }
            
            // Add click event
            questionButton.onClick.AddListener(() => SelectQuestion(questionIndex, questionButton));
        }
    }
    
    private void SelectQuestion(int questionIndex, Button clickedButton)
    {
        if (questionIndex < 0 || questionIndex >= selectedTheme.questions.Count)
            return;
        
        // Set selected question
        selectedQuestionIndex = questionIndex;
        selectedQuestion = selectedTheme.questions[questionIndex];
        
        // Update button visuals
        if (selectedQuestionButton != null)
        {
            selectedQuestionButton.GetComponent<Image>().color = normalButtonColor;
        }
        
        selectedQuestionButton = clickedButton;
        selectedQuestionButton.GetComponent<Image>().color = selectedButtonColor;
        
        // Update question input field
        questionInputField.text = selectedQuestion.answer;
        
        // Clear existing hint fields
        ClearHintFields();
        
        // Create hint fields for each clue
        if (selectedQuestion.clues != null)
        {
            foreach (string hint in selectedQuestion.clues)
            {
                AddHintField(hint);
            }
        }
        
        // Update button states
        UpdateQuestionButtonStates();
    }
    
    private void CreateNewQuestion()
    {
        // Create a new question with no hints
        Question newQuestion = new Question
        {
            answer = "New Question",
            clues = new List<string>()
        };
        
        // Add to the list
        if (selectedTheme.questions == null)
            selectedTheme.questions = new List<Question>();
            
        selectedTheme.questions.Add(newQuestion);
        
        // Save to file
        SaveThemeToFile();
        
        // Refresh the question buttons
        PopulateQuestionButtons();
        
        // Select the new question
        int newIndex = selectedTheme.questions.Count - 1;
        SelectQuestion(newIndex, questionButtonContainer.GetChild(newIndex).GetComponent<Button>());
    }
    
    private void SaveSelectedQuestion()
    {
        if (selectedQuestion == null || selectedQuestionIndex < 0)
            return;
        
        // Update question text
        selectedQuestion.answer = questionInputField.text.Trim();
        
        // Update hints
        selectedQuestion.clues = new List<string>();
        foreach (TMP_InputField hintField in currentHintFields)
        {
            string hint = hintField.text.Trim();
            if (!string.IsNullOrEmpty(hint))
            {
                selectedQuestion.clues.Add(hint);
            }
        }
        
        // Update the question in the theme
        selectedTheme.questions[selectedQuestionIndex] = selectedQuestion;
        
        // Save to file
        SaveThemeToFile();
        
        // Refresh question buttons
        PopulateQuestionButtons();
        
        // Reselect the question (to update the button text)
        SelectQuestion(selectedQuestionIndex, questionButtonContainer.GetChild(selectedQuestionIndex).GetComponent<Button>());
    }
    
    private void DeleteSelectedQuestion()
    {
        if (selectedQuestion == null || selectedQuestionIndex < 0)
            return;
        
        // Remove the question
        selectedTheme.questions.RemoveAt(selectedQuestionIndex);
        
        // Save to file
        SaveThemeToFile();
        
        // Refresh question buttons
        PopulateQuestionButtons();
        
        // Clear selection
        ClearQuestionSelection();
    }
    
    private void AddHintField(string initialText = "")
    {
        // Create a new hint input field
        TMP_InputField hintField = Instantiate(hintInputFieldPrefab, hintFieldsContainer);
        hintField.text = initialText;
        
        // Add a button to remove this hint field
        Button removeButton = hintField.GetComponentInChildren<Button>();
        if (removeButton != null)
        {
            removeButton.onClick.AddListener(() => RemoveHintField(hintField));
        }
        
        // Add to list
        currentHintFields.Add(hintField);
        
        // Update button states
        UpdateQuestionButtonStates();
    }
    
    private void RemoveHintField(TMP_InputField hintField)
    {
        if (hintField != null)
        {
            // Remove from list
            currentHintFields.Remove(hintField);
            
            // Destroy GameObject
            Destroy(hintField.gameObject);
            
            // Update button states
            UpdateQuestionButtonStates();
        }
    }
    
    private void ClearHintFields()
    {
        foreach (TMP_InputField field in currentHintFields)
        {
            Destroy(field.gameObject);
        }
        
        currentHintFields.Clear();
    }
    
    private void ClearQuestionSelection()
    {
        selectedQuestion = null;
        selectedQuestionButton = null;
        selectedQuestionIndex = -1;
        
        // Clear input fields
        questionInputField.text = "";
        ClearHintFields();
        
        // Update button states
        UpdateQuestionButtonStates();
    }
    
    private void UpdateQuestionButtonStates()
    {
        bool hasSelection = selectedQuestion != null;
        bool hasQuestionText = !string.IsNullOrEmpty(questionInputField.text.Trim());
        
        saveQuestionButton.interactable = hasSelection && hasQuestionText;
        deleteQuestionButton.interactable = hasSelection;
    }
    
    private void SaveThemeToFile()
    {
        try
        {
            // Create theme object to save
            Theme themeToSave = new Theme
            {
                themeName = selectedTheme.name,
                description = selectedTheme.description,
                visibleInBankEditor = true,
                questions = selectedTheme.questions
            };
            
            // Convert to JSON and save
            string json = JsonUtility.ToJson(themeToSave, true);
            File.WriteAllText(selectedTheme.filePath, json);
            
            Debug.Log($"Saved theme to file: {selectedTheme.filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving theme: {e.Message}");
        }
    }
}