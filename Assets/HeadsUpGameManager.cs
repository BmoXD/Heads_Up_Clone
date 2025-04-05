using System.Collections.Generic;
using UnityEngine;

public class HeadsUpGameManager : MonoBehaviour
{
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private OrientationDetector orientationDetector;
    [SerializeField] private QuestionDataManager dataManager;
    [SerializeField] private TextAsset defaultThemeFile; // For manually setting a theme
    [SerializeField] private int pointsPerCorrectAnswer = 100;
    [SerializeField] private int pointDeductionPerHint = 25;
    [SerializeField] private int maxPointDeduction = 75;
    [SerializeField] private float gameDuration = 60f; // Game time limit in seconds
    
    private List<Question> currentQuestions;
    private int currentQuestionIndex = -1;
    private bool isGameActive = false;
    private int currentScore = 0;
    private int correctGuessesCount = 0;
    private int currentHintDeduction = 0;
    private List<int> availableHintIndices = new List<int>();
    private float currentTime; // Current time remaining
    
    private void Awake()
    {
        // Lock screen to landscape mode
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Check if there's a theme passed from theme selection
        TextAsset themeToLoad = ThemePassingManager.GetSelectedTheme();
        
        // If nothing was passed, use the default theme
        if (themeToLoad == null && defaultThemeFile != null)
        {
            themeToLoad = defaultThemeFile;
        }
        
        // Load the theme
        if (themeToLoad != null)
        {
            dataManager.LoadTheme(themeToLoad);
        }
    }
    
    private void Start()
    {
        orientationDetector.OnCorrectGuessTilt += HandleCorrectGuess;
        orientationDetector.OnSkipTilt += HandleSkip;
        orientationDetector.OnHintTilt += HandleHintRequest;
        
        // Get questions from current theme
        currentQuestions = dataManager.GetCurrentThemeQuestions();
        
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogError("No questions available! Please ensure a theme is loaded.");
            return;
        }
        
        ShuffleQuestions();
        ResetScore();
        StartGame();
    }
    
    private void Update()
    {
        if (isGameActive)
        {
            // Update timer
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                gameplayUI.UpdateTimerDisplay(currentTime);
                
                // Check if time's up
                if (currentTime <= 0)
                {
                    currentTime = 0;
                    EndGame();
                }
            }
        }
    }
    
    public void StartGame()
    {
        currentTime = gameDuration;
        isGameActive = true;
        correctGuessesCount = 0;
        gameplayUI.UpdateTimerDisplay(currentTime);
        NextQuestion();
    }
    
    private void EndGame()
    {
        isGameActive = false;
        
        // Show game over screen with final score and correct guesses
        gameplayUI.ShowGameOver(currentScore, correctGuessesCount);
    }
    
    private void NextQuestion()
    {
        if (!isGameActive) return;
        
        currentQuestionIndex++;
        
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            // End of questions reached, restart from beginning
            ShuffleQuestions();
            currentQuestionIndex = 0;
        }
        
        // Reset hint state for new question
        ResetHints();
        
        // Prepare randomized hint indices for the new question
        PrepareRandomizedHints();
        
        gameplayUI.DisplayAnswer(currentQuestions[currentQuestionIndex].answer);
    }
    
    private void PrepareRandomizedHints()
    {
        availableHintIndices.Clear();
        
        Question currentQuestion = currentQuestions[currentQuestionIndex];
        if (currentQuestion.clues != null)
        {
            // Create a list of indices
            for (int i = 0; i < currentQuestion.clues.Count; i++)
            {
                availableHintIndices.Add(i);
            }
            
            // Shuffle the indices
            ShuffleList(availableHintIndices);
        }
    }
    
    private void HandleCorrectGuess()
    {
        if (!isGameActive) return;
        
        // Hide hint if it's showing
        gameplayUI.HideHint();
        
        // Increment correct guesses counter
        correctGuessesCount++;
        
        // Add points for correct answer (minus any hint deductions)
        int pointsAwarded = pointsPerCorrectAnswer - currentHintDeduction;
        AddPoints(pointsAwarded);
        
        // Play correct sound/animation
        Debug.Log($"Correct! Awarded {pointsAwarded} points. Moving to next question.");
        NextQuestion();
    }
    
    private void HandleSkip()
    {
        if (!isGameActive) return;
        
        // Hide hint if it's showing
        gameplayUI.HideHint();
        
        // No points for skipped questions
        
        // Play skip sound/animation
        Debug.Log("Skipped! Moving to next question.");
        NextQuestion();
    }
    
    private void HandleHintRequest()
    {
        if (!isGameActive) return;
        
        // Get current question
        Question currentQuestion = currentQuestions[currentQuestionIndex];
        
        // Check if there are clues available
        if (currentQuestion.clues == null || currentQuestion.clues.Count == 0 || 
            availableHintIndices.Count == 0)
        {
            Debug.Log("No hints available for this question.");
            return;
        }
        
        // Apply point deduction (up to max)
        if (currentHintDeduction < maxPointDeduction)
        {
            currentHintDeduction += pointDeductionPerHint;
            if (currentHintDeduction > maxPointDeduction)
                currentHintDeduction = maxPointDeduction;
        }
        
        // Get the next randomized hint
        int hintIndex = availableHintIndices[0];
        availableHintIndices.RemoveAt(0); // Remove this hint from available hints
        
        string hint = currentQuestion.clues[hintIndex];
        
        gameplayUI.ShowHint(hint, currentHintDeduction);
        
        Debug.Log($"Hint shown: {hint}. Point deduction: {currentHintDeduction}");
    }
    
    private void ResetHints()
    {
        currentHintDeduction = 0;
        availableHintIndices.Clear();
        gameplayUI.HideHint();
    }
    
    private void AddPoints(int points)
    {
        currentScore += points;
        gameplayUI.UpdateScoreDisplay(currentScore);
    }
    
    private void ResetScore()
    {
        currentScore = 0;
        gameplayUI.UpdateScoreDisplay(currentScore);
    }
    
    private void ShuffleQuestions()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = currentQuestions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Question temp = currentQuestions[i];
            currentQuestions[i] = currentQuestions[j];
            currentQuestions[j] = temp;
        }
    }
    
    // Generic method to shuffle any list
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}