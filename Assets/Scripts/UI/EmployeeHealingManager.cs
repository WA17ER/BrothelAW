using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeeHealingManager : MonoBehaviour
{
    [SerializeField] private GameObject employeeHealPanel;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Transform employeeGrid;
    [SerializeField] private GameObject employeeHealPanelPrefab;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI goldIndicator; // Индикатор золота в UI

    private List<Employee> selectedEmployees = new List<Employee>();

    private void Start()
    {
        if (employeeHealPanel == null || cancelButton == null || confirmButton == null || employeeGrid == null || employeeHealPanelPrefab == null || priceText == null || goldIndicator == null)
        {
            Debug.LogError("Одна или несколько ссылок в EmployeeHealingManager не привязаны.");
            return;
        }
        employeeHealPanel.SetActive(false);
        if (cancelButton != null) cancelButton.onClick.AddListener(ClosePanel);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmHealing);
        UpdateIndicators();
        PopulateEmployeeGrid();
        UpdatePriceText();
    }

    private void UpdateIndicators()
    {
        if (GameManager.Instance != null)
        {
            goldIndicator.text = $"Золото: {GameManager.Instance.CurrentGold}";
            Debug.Log($"Обновлён индикатор: Золото = {GameManager.Instance.CurrentGold}");
        }
    }

    private void ClosePanel()
    {
        if (employeeHealPanel != null)
        {
            employeeHealPanel.SetActive(false);
            Debug.Log("Панель успешно закрыта через CancelButton");
        }
        else
        {
            Debug.LogError("employeeHealPanel не инициализирован в ClosePanel");
        }
    }

    private void PopulateEmployeeGrid()
    {
        if (employeeGrid == null || employeeHealPanelPrefab == null || EmployeeManager.Instance == null) return;

        // Очистка всех предыдущих элементов грида
        foreach (Transform child in employeeGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var employee in EmployeeManager.Instance.SickEmployees)
        {
            if (employee != null && (employee.GetState() == Employee.EmployeeState.Sick || employee.GetState() == Employee.EmployeeState.HeavySick))
            {
                GameObject panelInstance = Instantiate(employeeHealPanelPrefab, employeeGrid);
                EmployeeHealPanel panelScript = panelInstance.GetComponent<EmployeeHealPanel>();
                if (panelScript != null)
                {
                    panelScript.Initialize(employee, this);
                }
            }
        }
    }

    public void AddEmployee(Employee employee)
    {
        if (!selectedEmployees.Contains(employee))
        {
            selectedEmployees.Add(employee);
            float cost = employee.ActiveSick.HealingCost;
            Debug.Log($"Добавлена сотрудница {employee.Data.employeeName}, стоимость лечения {cost}");
            UpdatePriceText();
            LogSelectedEmployees();
        }
    }

    public void CancelEmployee(Employee employee)
    {
        if (selectedEmployees.Contains(employee))
        {
            selectedEmployees.Remove(employee);
            Debug.Log($"Сотрудница удалена, цена лечения {GetTotalCost()}");
            UpdatePriceText();
            LogSelectedEmployees();
        }
    }

    private float GetTotalCost()
    {
        return selectedEmployees.Sum(e => e.ActiveSick.HealingCost);
    }

    private void UpdatePriceText()
    {
        priceText.text = $"Стоимость лечения: {GetTotalCost()}";
    }

    public List<Employee> GetSelectedEmployees()
    {
        return selectedEmployees;
    }

    private void LogSelectedEmployees()
    {
        if (selectedEmployees.Count == 0)
        {
            Debug.Log("Список добавленных сотрудниц: None");
        }
        else
        {
            string names = string.Join(", ", selectedEmployees.Select(e => e.Data.employeeName));
            Debug.Log($"Список добавленных сотрудниц: {names}");
        }
    }

    private void ConfirmHealing()
    {
        if (GameManager.Instance == null || EmployeeManager.Instance == null)
        {
            Debug.LogError("GameManager или EmployeeManager не инициализированы.");
            return;
        }
        float totalCost = GetTotalCost();
        if (GameManager.Instance.CurrentGold >= totalCost)
        {
            foreach (var employee in selectedEmployees)
            {
                employee.Heal();
                employee.ProgressHealing();
                EmployeeManager.Instance.MoveEmployeeToList(employee);
                EmployeeManager.Instance.HealingEmployees.Add(employee);
                Debug.Log($"Сотрудница {employee.Data.employeeName} переведена в Healing, счётчик запущен");
            }
            GameManager.Instance.AddGold(-totalCost);
            Debug.Log($"Вычтено золото: {totalCost}, остаток: {GameManager.Instance.CurrentGold}");
            selectedEmployees.Clear();
            UpdatePriceText();
            UpdateIndicators(); // Обновление индикатора золота
            PopulateEmployeeGrid(); // Перегенерация грида после лечения
            employeeHealPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Недостаточно золота: требуется {totalCost}, доступно {GameManager.Instance.CurrentGold}");
        }
    }

    public void OnPanelActivated()
    {
        if (employeeHealPanel != null)
        {
            employeeHealPanel.SetActive(true);
            PopulateEmployeeGrid(); // Перегенерация грида при активации
        }
        else
        {
            Debug.LogError("employeeHealPanel не инициализирован при активации");
        }
    }
}