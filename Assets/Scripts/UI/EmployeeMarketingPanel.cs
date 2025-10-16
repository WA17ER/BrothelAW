using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class EmployeeMarketingPanel : MonoBehaviour
{
    [SerializeField] private Transform employeeListGrid;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject employeePanelPrefab;
    [SerializeField] private Employee selectedEmployee;
    private DistrictData currentDistrict;
    private Button confirmButton, cancelButton;

    void Start()
    {
        confirmButton = buttonPanel?.transform.Find("ConfirmButton")?.GetComponent<Button>();
        cancelButton = buttonPanel?.transform.Find("CancelButton")?.GetComponent<Button>();
        confirmButton?.onClick.AddListener(ConfirmSelection);
        cancelButton?.onClick.AddListener(CancelSelection);
    }

    public void OpenPanel(DistrictData district)
    {
        currentDistrict = district;
        selectedEmployee = null;
        ClearGrid();
        PopulateEmployeeList();
        gameObject.SetActive(true);
    }

    void ClearGrid()
    {
        foreach (Transform child in employeeListGrid)
        {
            Destroy(child.gameObject);
        }
    }

    void PopulateEmployeeList()
    {
        var available = EmployeeManager.Instance?.AvailableEmployees?
            .Where(e => e != null && e.GetState() == Employee.EmployeeState.Available &&
                        e.Data.race != EmployeeDataSO.Race.Ангел &&
                        e.Data.race != EmployeeDataSO.Race.Допельгангер &&
                        e.Data.race != EmployeeDataSO.Race.Суккуб) ?? Enumerable.Empty<Employee>();
        foreach (var employee in available)
        {
            var panel = Instantiate(employeePanelPrefab, employeeListGrid);
            var btn = panel.GetComponent<Button>();
            var icon = panel.transform.Find("EmployeeIcon")?.GetComponent<Image>();
            var nameTxt = panel.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
            if (btn != null && icon != null && nameTxt != null)
            {
                icon.sprite = employee.Data.listIcon;
                nameTxt.text = employee.Data.employeeName;
                btn.onClick.AddListener(() => SelectEmployee(employee));
            }
            else
            {
                Destroy(panel);
            }
        }
    }

    void SelectEmployee(Employee employee)
    {
        selectedEmployee = employee;
    }

    void ConfirmSelection()
    {
        if (currentDistrict != null && selectedEmployee != null)
        {
            currentDistrict.SetAssignedEmployee(selectedEmployee);
            currentDistrict.ConfirmAssignment();
            gameObject.SetActive(false);
        }
    }

    void CancelSelection()
    {
        if (currentDistrict != null)
        {
            currentDistrict.ClearAssignment();
        }
        gameObject.SetActive(false);
    }
}