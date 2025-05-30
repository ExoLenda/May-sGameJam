using UnityEngine;
using UnityEngine.SceneManagement; // Essential for scene management!
using TMPro; // Required for TextMeshPro

public class SceneTransitionTimer : MonoBehaviour
{
    // Public variables configurable in the Inspector
    public float timeToWait = 5f; // Time to wait in seconds (e.g., 5 seconds)
    public string targetSceneName = "NextScene"; // The name of the scene to load (e.g., "MainMenu" or "Level2")

    public TMP_Text timerTextDisplay; // The TextMeshPro object to display the time
    private float _currentTimer; // Internal variable to track the countdown

    void Start()
    {
        // Initialize the timer when the game starts
        _currentTimer = timeToWait;
        Debug.Log("Waiting for " + timeToWait + " seconds before loading scene: " + targetSceneName);
    }

    void Update()
    {
        // Decrease the timer by the time elapsed since the last frame
        _currentTimer -= Time.deltaTime;

        // Display the timer on the screen, rounded up to the nearest whole number
        if (timerTextDisplay != null) // Check to prevent errors if not assigned
        {
            timerTextDisplay.text = Mathf.CeilToInt(_currentTimer).ToString();
        }

        // If the timer runs out (reaches zero or less)
        if (_currentTimer <= 0)
        {
            // Optional: You could set _currentTimer = 0; if you want to ensure it doesn't go negative,
            // but for a single scene load, it's not strictly necessary.

            // Load the target scene
            LoadTargetScene();
        }
    }

    void LoadTargetScene()
    {
        // Log a message to know which scene is being loaded
        Debug.Log("Time's up! Loading scene: " + targetSceneName);

        // Use Unity's SceneManager to load the specified scene
        SceneManager.LoadScene(targetSceneName);
    }
}
