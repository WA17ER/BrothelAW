using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeHealPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI employeeNameText;
    [SerializeField] private Button addButton;
    [SerializeField] private Button cancelButton;

    private Employee employee;
    private EmployeeHealingManager healingManager;

    public void Initialize(Employee emp, EmployeeHealingManager manager)
    {
        employee = emp;
        healingManager = manager;
        employeeNameText.text = employee.Data.employeeName;
        addButton.onClick.AddListener(OnAddButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
        cancelButton.interactable = false;
    }

    private void OnAddButtonClick()
    {
        healingManager.AddEmployee(employee);
        addButton.interactable = false;
        cancelButton.interactable = true;
    }

    private void OnCancelButtonClick()
    {
        healingManager.CancelEmployee(employee);
        addButton.interactable = true;
        cancelButton.interactable = false;
    }
}