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
        startDayButton?.onClick.RemoveAllListeners();
        startDayButton?.onClick.AddListener(StartDay);
        healButton?.onClick.RemoveAllListeners();
        healButton?.onClick.AddListener(() => { Debug.Log("Heal clicked"); employeeHealPanel?.SetActive(true); });
        marketingButton?.onClick.RemoveAllListeners();
        marketingButton?.onClick.AddListener(() => cityPanel?.SetActive(true));
        hireingButton?.onClick.RemoveAllListeners();
        hireingButton?.onClick.AddListener(() => hirePanel?.SetActive(true));
        trainButton?.onClick.RemoveAllListeners();
        trainButton?.onClick.AddListener(() => employeeTrainPanel?.SetActive(true));
        employeeHealPanel?.SetActive(false);
        cityPanel?.SetActive(false);
        hirePanel?.SetActive(false);
        employeeTrainPanel?.SetActive(false);
    }
    private void StartDay()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetPreviousScene("ManagmentScene");
        SceneManager.LoadScene("MainScene");
    }    
}