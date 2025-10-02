using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmployeeTrainPanelUI : MonoBehaviour
{
    [SerializeField] private Transform grid;
    [SerializeField] private GameObject employeePanelPrefab;
    [SerializeField] private GameObject employeeInfoPanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text skill1Text;
    [SerializeField] private TMP_Text skill2Text;
    [SerializeField] private TMP_Text skill3Text;
    [SerializeField] private TMP_Text skill4Text;
    [SerializeField] private TMP_Text skill5Text;
    [SerializeField] private TMP_Text skill6Text;
    [SerializeField] private TMP_Text skill7Text;
    [SerializeField] private TMP_Text skill8Text;
    [SerializeField] private Button closeButton;

    private Employee selectedEmp;
    private Dictionary<Employee, GameObject> panels = new Dictionary<Employee, GameObject>();
    private TMP_Text[] skillTexts = new TMP_Text[8];
    [SerializeField] private static int trainCounter = 3;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        skillTexts[0] = skill1Text;
        skillTexts[1] = skill2Text;
        skillTexts[2] = skill3Text;
        skillTexts[3] = skill4Text;
        skillTexts[4] = skill5Text;
        skillTexts[5] = skill6Text;
        skillTexts[6] = skill7Text;
        skillTexts[7] = skill8Text;
        employeeInfoPanel.SetActive(false);
        PopulateGrid();
    }

    private void PopulateGrid()
    {
        var employees = EmployeeManager.Instance.GetAllEmployees().Where(e => e.GetState() == Employee.EmployeeState.Available).ToList();
        foreach (var emp in employees)
        {
            GameObject panel = Instantiate(employeePanelPrefab, grid);
            Image icon = panel.transform.Find("EmployeeIcon")?.GetComponent<Image>();
            TMP_Text nameTxt = panel.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
            if (icon != null) icon.sprite = emp.Data.listIcon;
            if (nameTxt != null) nameTxt.text = emp.Data.employeeName;
            Button btn = panel.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => ShowInfo(emp));
            panels[emp] = panel;
        }
    }

    private void ShowInfo(Employee emp)
    {
        selectedEmp = emp;
        employeeInfoPanel.SetActive(true);
        if (nameText != null) nameText.text = "Èìÿ: " + emp.Data.employeeName;

        string[] baseSkills = emp.Data.BaseSkills;
        // Clear all skill texts
        for (int i = 0; i < 8; i++)
        {
            if (skillTexts[i] != null)
            {
                skillTexts[i].text = "None";
                Button btn = skillTexts[i].GetComponentInChildren<Button>();
                if (btn != null) btn.onClick.RemoveAllListeners();
            }
        }

        // Fill skills
        for (int i = 0; i < baseSkills.Length && i < 8; i++)
        {
            string skill = baseSkills[i];
            string displayLevel = emp.GetSkillLevelString(skill);
            if (skillTexts[i] != null)
            {
                skillTexts[i].text = $"{skill} (Lvl: {displayLevel})";
                Button btn = skillTexts[i].GetComponentInChildren<Button>();
                if (btn != null)
                {
                    bool canTrain = emp.CanLevelUpSkill(skill) && trainCounter > 0;
                    btn.interactable = canTrain;
                    int skillIndex = i;
                    btn.onClick.AddListener(() => TrainSkill(skillIndex));
                }
            }
        }
    }

    private void TrainSkill(int skillIndex)
    {
        if (selectedEmp == null || trainCounter <= 0) return;
        string[] baseSkills = selectedEmp.Data.BaseSkills;
        if (skillIndex < baseSkills.Length)
        {
            string skill = baseSkills[skillIndex];
            GameStateManager.Instance.SetTrainingData(selectedEmp, skill, false, false, false);
            SceneManager.LoadScene("TrainScene");
            trainCounter--;
            Debug.Log($"Training started for {skill}. Remaining trains: {trainCounter}");
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public static void ResetCounter()
    {
        trainCounter = 3;
    }

    private void OnDestroy()
    {
        foreach (var panel in panels.Values)
        {
            if (panel != null) Destroy(panel);
        }
    }
}