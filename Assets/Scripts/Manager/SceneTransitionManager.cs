using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public void TransitionToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void EndCurrentDay()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndDayPublic();
        }
    }
}