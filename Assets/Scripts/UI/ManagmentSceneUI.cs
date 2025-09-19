using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ManagmentSceneUI : MonoBehaviour
{
    [SerializeField] private Button startDayButton;
    [SerializeField] private Button healButton;
    [SerializeField] private Button marketingButton;
    [SerializeField] private GameObject employeeHealPanel;
    [SerializeField] private GameObject cityPanel;

    private void Start()
    {
        startDayButton.onClick.AddListener(StartDay);
        healButton.onClick.AddListener(OpenHealPanel);
        marketingButton.onClick.AddListener(OpenCityPanel);
        employeeHealPanel.SetActive(false);
        cityPanel.SetActive(false);
    }

    private void StartDay()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void OpenHealPanel()
    {
        employeeHealPanel.SetActive(true);
    }

    private void OpenCityPanel()
    {
        if (cityPanel != null)
        {
            cityPanel.SetActive(true);
        }
    }
}