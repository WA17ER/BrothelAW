using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void TransitionToScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    public void EndCurrentDay()
    {
        GameManager.Instance?.EndDayPublic();
    }
}