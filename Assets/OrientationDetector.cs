using UnityEngine;
using System;

public class OrientationDetector : MonoBehaviour
{
    public event Action OnCorrectGuessTilt;
    public event Action OnSkipTilt;
    public event Action OnHintTilt; // New event for hint
    
    [SerializeField] private float tiltThreshold = 0.7f;
    [SerializeField] private float neutralThreshold = 0.3f; // Threshold for detecting neutral position
    [SerializeField] private float sideTiltThreshold = 0.5f; // Threshold for detecting side tilt
    
    private bool isProcessingTilt = false;
    private bool isTilted = false; // Track if device is currently tilted
    
    private void Start()
    {
        // Lock screen to landscape mode
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    
    private void Update()
    {
        // Get device orientation from accelerometer
        Vector3 acceleration = Input.acceleration;
        
        // Check if device has returned to neutral position
        if (isTilted && Mathf.Abs(acceleration.z) < neutralThreshold && Mathf.Abs(acceleration.x) < neutralThreshold)
        {
            isTilted = false;
        }
        
        // Only process new tilts if device is not already tilted and not processing a previous tilt
        if (!isTilted && !isProcessingTilt)
        {
            // Check for downward tilt (correct guess) - when phone faces floor
            if (acceleration.z > tiltThreshold)
            {
                isProcessingTilt = true;
                isTilted = true;
                OnCorrectGuessTilt?.Invoke();
                Invoke(nameof(ResetTiltProcessing), 0.5f);
            }
            
            // Check for upward tilt (skip) - when phone faces sky
            else if (acceleration.z < -tiltThreshold)
            {
                isProcessingTilt = true;
                isTilted = true;
                OnSkipTilt?.Invoke();
                Invoke(nameof(ResetTiltProcessing), 0.5f);
            }
            
            // Check for left side tilt (hint)
            else if (acceleration.x < -sideTiltThreshold)
            {
                isProcessingTilt = true;
                isTilted = true;
                OnHintTilt?.Invoke();
                Invoke(nameof(ResetTiltProcessing), 0.5f);
            }
        }
    }
    
    private void ResetTiltProcessing()
    {
        isProcessingTilt = false;
    }
}