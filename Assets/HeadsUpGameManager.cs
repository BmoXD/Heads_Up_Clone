using System.Collections.Generic;
using UnityEngine;

public class HeadsUpGameManager : MonoBehaviour
{
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private OrientationDetector orientationDetector;
    [SerializeField] private QuestionDataManager dataManager;
    [SerializeField] private string currentTheme = "Movies";
    [SerializeField] private int pointsPerCorrectAnswer = 100;
    [SerializeField] private int pointDeductionPerHint = 25;
    [SerializeField] private int maxPointDeduction = 75; // Cap at 75 point deduction (3 hints)
    
    private List<Question> currentQuestions;
    private int currentQuestionIndex = -1;
    private bool isGameActive = false;
    private int currentScore = 0;
    private int currentHintDeduction = 0;
    private List<int> availableHintIndices = new List<int>(); // Track available hint indices
    
    private void Awake()
    {
        // Lock screen to landscape mode
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        
        // Prevent screen from turning off during gameplay
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    
    private void Start()
    {
        orientationDetector.OnCorrectGuessTilt += HandleCorrectGuess;
        orientationDetector.OnSkipTilt += HandleSkip;
        orientationDetector.OnHintTilt += HandleHintRequest;
        
        currentQuestions = dataManager.GetQuestionsForTheme(currentTheme);
        ShuffleQuestions();
        ResetScore();
        StartGame();
    }
    
    public void StartGame()
    {
        isGameActive = true;
        NextQuestion();
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
    
    public void SetTheme(string themeName)
    {
        currentTheme = themeName;
        currentQuestions = dataManager.GetQuestionsForTheme(themeName);
        ShuffleQuestions();
        currentQuestionIndex = -1;
    }
}