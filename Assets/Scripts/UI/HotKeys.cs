using UnityEngine;

public class HotKeys : MonoBehaviour
{
    [SerializeField] private GameObject localMenuPrefab;
    private GameObject localMenuInstance;
    private bool isPaused = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Time.timeScale = 1f;
                Destroy(localMenuInstance);
                localMenuInstance = null;
                isPaused = false;
            }
            else
            {
                Time.timeScale = 0f;
                var canvas = GameObject.FindWithTag("Canvas")?.GetComponent<Canvas>()?.transform;
                localMenuInstance = Instantiate(localMenuPrefab, canvas);
                isPaused = true;
            }
        }
    }
}