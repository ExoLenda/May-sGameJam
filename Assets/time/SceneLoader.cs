using UnityEngine;
using UnityEngine.SceneManagement; // Add this namespace for scene management

public class SceneLoader : MonoBehaviour
{
    // This function will determine which scene to load based on the button clicked
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
