using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmployeeMarketingPanel : MonoBehaviour
{
    [SerializeField] private Transform employeeListGrid;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject employeePanelPrefab; // Префаб EmployeePanel
    [SerializeField] private Employee selectedEmployee; // Параметр для предварительно выбранной сотрудницы

    private DistrictData currentDistrict;

    private void Start()
    {
        if (employeeListGrid == null || buttonPanel == null || employeePanelPrefab == null)
        {
            Debug.LogError("Не все поля инициализированы в EmployeeMarketingPanel.");
            return;
        }
        var confirmButton = buttonPanel.transform.Find("ConfirmButton")?.GetComponent<UnityEngine.UI.Button>();
        var cancelButton = buttonPanel.transform.Find("CancelButton")?.GetComponent<UnityEngine.UI.Button>();
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmSelection);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelSelection);
    }

    public void OpenPanel(DistrictData district)
    {
        currentDistrict = district;
        if (currentDistrict != null)
        {
            gameObject.SetActive(true);
            selectedEmployee = null; // Сброс выбранной сотрудницы при открытии
            PopulateEmployeeList();
        }
    }

    private void OnDisable()
    {
        // Очистка EmployeeListGrid при закрытии панели
        foreach (Transform child in employeeListGrid)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateEmployeeList()
    {
        if (EmployeeManager.Instance == null)
        {
            Debug.LogError("EmployeeManager.Instance не инициализирован.");
            return;
        }
        foreach (Transform child in employeeListGrid)
        {
            Destroy(child.gameObject);
        }
        if (EmployeeManager.Instance.AvailableEmployees == null)
        {
            Debug.LogError("AvailableEmployees в EmployeeManager не инициализирован.");
            return;
        }
        foreach (var employee in EmployeeManager.Instance.AvailableEmployees)
        {
            if (employee == null) continue;
            if (employee.GetState() == Employee.EmployeeState.Available &&
                employee.Data.race != EmployeeDataSO.Race.Ангел &&
                employee.Data.race != EmployeeDataSO.Race.Допельгангер &&
                employee.Data.race != EmployeeDataSO.Race.Суккуб)
            {
                var employeePanel = Instantiate(employeePanelPrefab, employeeListGrid);
                if (employeePanel != null)
                {
                    var button = employeePanel.GetComponent<UnityEngine.UI.Button>();
                    var employeeIcon = employeePanel.transform.Find("EmployeeIcon")?.GetComponent<Image>();
                    var employeeNameText = employeePanel.transform.Find("EmployeeNameText")?.GetComponent<TMP_Text>();
                    if (button != null && employeeIcon != null && employeeNameText != null)
                    {
                        employeeIcon.sprite = employee.Data.listIcon;
                        if (employeeIcon.sprite == null)
                        {
                            Debug.LogWarning($"listIcon для {employee.Data.employeeName} не задан.");
                        }
                        employeeNameText.text = employee.Data.employeeName;
                        employeePanel.SetActive(true);
                        button.onClick.AddListener(() => SelectEmployee(employee));
                    }
                    else
                    {
                        Debug.LogError($"Отсутствуют компоненты на {employeePanel.name}: Button, EmployeeIcon или EmployeeNameText.");
                    }
                }
                else
                {
                    Debug.LogError("Не удалось создать экземпляр employeePanelPrefab.");
                }
            }
        }
    }

    private void SelectEmployee(Employee employee)
    {
        if (currentDistrict != null)
        {
            selectedEmployee = employee; // Запись предварительного выбора
            Debug.Log($"Выбрана сотрудница {selectedEmployee.Data.employeeName} для района {currentDistrict.GetDistrictData().DistrictName}");
        }
    }

    private void ConfirmSelection()
    {
        if (currentDistrict != null && selectedEmployee != null)
        {
            foreach (var districtPanel in Object.FindObjectsByType<DistrictData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (districtPanel != currentDistrict && districtPanel.GetAssignedEmployee() != null && districtPanel.GetAssignedEmployee() == selectedEmployee)
                {
                    districtPanel.SetAssignedEmployee(null);
                    Debug.Log($"Сотрудница {selectedEmployee.Data.employeeName} освобождена из района {districtPanel.GetDistrictData().DistrictName} для назначения в район {currentDistrict.GetDistrictData().DistrictName}.");
                }
            }
            currentDistrict.SetAssignedEmployee(selectedEmployee); // Прямое назначение в assignedEmployee
            currentDistrict.ConfirmAssignment(); // Вызываем для логики и обновления UI
            gameObject.SetActive(false);
            Debug.Log($"Сотрудница {selectedEmployee.Data.employeeName} подтверждена для района {currentDistrict.GetDistrictData().DistrictName}");
        }
    }

    private void CancelSelection()
    {
        if (currentDistrict != null)
        {
            currentDistrict.ClearAssignment();
        }
        gameObject.SetActive(false);
    }
}