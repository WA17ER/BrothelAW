using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LocalMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject savePanelPrefab;    
    [SerializeField] private Button saveButton, loadButton, optionsButton, menuButton, exitButton;

    void Start()
    {
        saveButton?.onClick.AddListener(OpenSavePanel);
        loadButton?.onClick.AddListener(OpenLoadPanel);
        optionsButton?.onClick.AddListener(OpenOptions);
        menuButton?.onClick.AddListener(ReturnToMainMenu);
        exitButton?.onClick.AddListener(ExitGame);
    }

    void OpenSavePanel()
    {
        var menuCanvas = GameObject.FindWithTag("Canvas")?.GetComponent<Canvas>()?.transform;
        var savePanel = Instantiate(savePanelPrefab, menuCanvas.transform);
        savePanel.SetActive(true);
    }

    void OpenLoadPanel()
    {
        // Load logic
    }

    void OpenOptions()
    {
        // Options logic
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void ExitGame()
    {
        Application.Quit();
    }
}