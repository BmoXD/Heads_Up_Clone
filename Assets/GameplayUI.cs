using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI answerWhenHintText;
    [SerializeField] private TextMeshProUGUI pointDeductionText;
    [SerializeField] private float gameDuration = 60f;
    
    private float currentTime;
    private string currentAnswer;
    private bool isHintShowing = false;
    
    private void Start()
    {
        currentTime = gameDuration;
        UpdateTimerDisplay();
        UpdateScoreDisplay(0); // Initialize score display
        
        // Hide hint-related elements initially
        if (answerWhenHintText != null) answerWhenHintText.gameObject.SetActive(false);
        if (pointDeductionText != null) pointDeductionText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            // Time's up!
            currentTime = 0;
            // Handle end of round
        }
    }
    
    private void UpdateTimerDisplay()
    {
        int seconds = Mathf.FloorToInt(currentTime);
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
}