using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ManagmentSceneUI : MonoBehaviour
{
    [SerializeField] private Button startDayButton;
    [SerializeField] private Button healButton;
    [SerializeField] private Button marketingButton;
    [SerializeField] private Button hireingButton;
    [SerializeField] private Button trainButton;
    [SerializeField] private GameObject employeeHealPanel;
    [SerializeField] private GameObject cityPanel;
    [SerializeField] private GameObject hirePanel;
    [SerializeField] private GameObject employeeTrainPanel;
    private void Start()
    {
        startDayButton.onClick.AddListener(StartDay);
        healButton.onClick.AddListener(OpenHealPanel);
        marketingButton.onClick.AddListener(OpenCityPanel);
        hireingButton.onClick.AddListener(OpenHirePanel);
        trainButton.onClick.AddListener(OpenTrainPanel);
        employeeHealPanel.SetActive(false);
        cityPanel.SetActive(false);
        hirePanel.SetActive(false);
        employeeTrainPanel.SetActive(false);
    }
    private void StartDay()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetPreviousScene("ManagmentScene");
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
    private void OpenHirePanel()
    {
        if (hirePanel != null)
        {
            hirePanel.SetActive(true);
        }
    }
    private void OpenTrainPanel()
    {
        if (employeeTrainPanel != null)
        {
            employeeTrainPanel.SetActive(true);
        }
    }
}