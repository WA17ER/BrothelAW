using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HireEmployeeUI : MonoBehaviour
{
    [SerializeField] private Transform grid;
    [SerializeField] private GameObject employeePanelPrefab;
    [SerializeField] private GameObject employeeInfoPanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button hireButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text raceText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text breastText;
    [SerializeField] private TMP_Text priceText;

    private EmployeeDataSO selectedEmp;
    private Dictionary<EmployeeDataSO, GameObject> panels = new Dictionary<EmployeeDataSO, GameObject>();

    private void Start()
    {
        if (cancelButton != null) cancelButton.onClick.AddListener(ClosePanel);
        if (hireButton != null) hireButton.onClick.RemoveAllListeners();        
        PopulateGrid();
    }

    private void PopulateGrid()
    {
        var temps = Resources.LoadAll<EmployeeDataSO>("TemporaryEmployee");
        foreach (var emp in temps.Where(emp => emp != null))
        {
            var panel = Instantiate(employeePanelPrefab, grid);
            var icon = panel.transform.Find("EmployeeIcon")?.GetComponent<Image>();
            var nameTxt = panel.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
            if (icon != null) icon.sprite = emp.listIcon;
            if (nameTxt != null) nameTxt.text = emp.employeeName;
            var btn = panel.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => SelectEmployee(emp));
            panels[emp] = panel;
        }
    }
    void OnDestroy()
    {
        foreach (var panel in panels.Values)
        {
            if (panel != null) Destroy(panel);
        }
        panels.Clear();
    }

    private void SelectEmployee(EmployeeDataSO emp)
    {
        selectedEmp = emp;
        employeeInfoPanel.SetActive(true);
        if (nameText != null) nameText.text += emp.employeeName;
        if (raceText != null) raceText.text += emp.race.ToString();
        if (bodyText != null) bodyText.text += emp.bodyType.ToString();
        if (breastText != null) breastText.text += emp.breastSize.ToString();
        if (priceText != null) priceText.text += $"Price: {emp.hireCost}";
        if (hireButton != null)
        {
            hireButton.onClick.RemoveAllListeners();
            hireButton.onClick.AddListener(() => HireEmployee(emp));
        }
    }

    private void HireEmployee(EmployeeDataSO emp)
    {
        if (emp == null || GameStateManager.Instance.Gold < emp.hireCost) return;
        GameStateManager.Instance.UpdateGold(GameStateManager.Instance.Gold - emp.hireCost);
        string oldPath = Path.Combine("Assets/Resources/TemporaryEmployee", emp.name + ".asset").Replace("\\", "/");
        string newPath = Path.Combine("Assets/Resources/Employees", emp.name + ".asset").Replace("\\", "/");
        if (File.Exists(oldPath))
        {
            AssetDatabase.MoveAsset(oldPath, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        GameObject empObj = new GameObject(emp.employeeName);
        Employee employee = empObj.AddComponent<Employee>();
        employee.SetData(emp);
        DontDestroyOnLoad(empObj);
        GameStateManager.Instance.AddEmployee(employee);
        EmployeeManager.Instance.MoveEmployeeToList(employee);
        if (panels.ContainsKey(emp))
        {
            Destroy(panels[emp]);
            panels.Remove(emp);
        }
        employeeInfoPanel.SetActive(false);
        selectedEmp = null;
        Debug.Log($"Hired {emp.employeeName}");
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}