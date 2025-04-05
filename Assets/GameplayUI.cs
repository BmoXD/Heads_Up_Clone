using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayUI : MonoBehaviour
{
    [Header("Main Game UI")]
    [SerializeField] private GameObject gameplayPanel; // Add this line - parent container for gameplay UI
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI answerWhenHintText;
    [SerializeField] private TextMeshProUGUI pointDeductionText;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI correctGuessesText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private string currentAnswer;
    private bool isHintShowing = false;
    
    private void Start()
    {
        UpdateScoreDisplay(0); // Initialize score display
        
        // Hide hint-related elements initially
        if (answerWhenHintText != null) answerWhenHintText.gameObject.SetActive(false);
        if (pointDeductionText != null) pointDeductionText.gameObject.SetActive(false);
        
        // Hide game over panel initially
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // Show gameplay panel
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        
        // Set up button listener
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }
    
    public void UpdateTimerDisplay(float time)
    {
        int seconds = Mathf.FloorToInt(time);
        timerText.text = seconds.ToString();
    }
    
    public void DisplayAnswer(string answer)
    {
        currentAnswer = answer.ToUpper();
        answerText.text = currentAnswer;
        
        // Reset hint state when showing a new answer
        HideHint();
    }
    
    public void UpdateScoreDisplay(int score)
    {
        scoreText.text = $"Score: {score}";
    }
    
    public void ShowHint(string hint, int pointDeduction)
    {
        // No need to check if a hint is already showing - we'll replace it
        isHintShowing = true;
        
        // Move answer to the alternative text field (if not already done)
        if (answerWhenHintText != null)
        {
            answerWhenHintText.text = currentAnswer;
            answerWhenHintText.gameObject.SetActive(true);
        }
        
        // Show hint in the main answer field
        answerText.text = hint.ToUpper();
        
        // Update point deduction
        if (pointDeductionText != null)
        {
            pointDeductionText.text = $"-{pointDeduction} pts";
            pointDeductionText.gameObject.SetActive(true);
        }
    }
    
    public void HideHint()
    {
        if (!isHintShowing) return;
        
        isHintShowing = false;
        
        // Restore answer to main text field
        answerText.text = currentAnswer;
        
        // Hide the alternate answer text
        if (answerWhenHintText != null) answerWhenHintText.gameObject.SetActive(false);
        
        // Hide point deduction text
        if (pointDeductionText != null) pointDeductionText.gameObject.SetActive(false);
    }
    
    public bool IsHintShowing()
    {
        return isHintShowing;
    }
    
    public void ShowGameOver(int score, int correctGuesses)
    {
        // Hide the gameplay UI
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        
        // Calculate total score
        int bonusPoints = correctGuesses * 25;
        int totalScore = score + bonusPoints;
        
        // Update UI texts
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {score}";
            
        if (correctGuessesText != null)
            correctGuessesText.text = $"Correct Guesses: {correctGuesses}";
            
        if (totalScoreText != null)
            totalScoreText.text = $"Total Score: {totalScore}";
        
        // Show the game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
    
    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}