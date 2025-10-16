using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DistrictData : MonoBehaviour
{
    [SerializeField] private DistrictDataSO districtData;
    [SerializeField] private Image employeeImage;
    [SerializeField] private TMP_Text districtNameText;
    [SerializeField] private EmployeeMarketingPanel employeeMarketingPanel;

    [SerializeField] private Employee activeEmployee; // Остаётся private
    [SerializeField] private Employee assignedEmployee; // Остаётся private
    private bool isConfirmed;
    [SerializeField] private float popularityGain; // Новое поле для популярности
    private float _popularityGainCache;
    public float GetPopularityGain() => _popularityGainCache;

    void Start()
    {
        if (districtData == null || employeeImage == null || districtNameText == null || employeeMarketingPanel == null) return;
        districtNameText.text = districtData.DistrictName;
        UpdateEmployeeImage();
        CalculatePopularityGain();
    }

    public void OnEmployeeImageClicked()
    {
        if (employeeMarketingPanel != null && activeEmployee == null && !isConfirmed && districtData != null && !string.IsNullOrEmpty(districtData.DistrictName))
        {
            employeeMarketingPanel.OpenPanel(this);
        }
    }

    public void AssignEmployee(Employee employee)
    {
        if (employee == null || employee.GetState() != Employee.EmployeeState.Available) return;
        // Очистка активного сотрудника в текущем районе, если он есть и отличается
        if (GetActiveEmployee() != null && GetActiveEmployee() != employee)
        {
            GetActiveEmployee().SetState(Employee.EmployeeState.Available);
            EmployeeManager.Instance.MoveEmployeeToList(GetActiveEmployee());
            Debug.Log($"Сотрудница {GetActiveEmployee().Data.employeeName} освобождена от предварительного назначения в район {districtData.DistrictName}.");
            SetActiveEmployee(null);
        }
        SetActiveEmployee(employee);
        CalculatePopularityGain(); // Пересчет популярности при назначении
        districtData.PreliminaryPopularity = popularityGain; // Передача в DistrictDataSO
        UpdateEmployeeImage();
        Debug.Log($"Сотрудница {employee.Data.employeeName} предварительно назначена в район {districtData.DistrictName}. Популярность: {popularityGain}");
    }

    public void ClearActiveEmployee()
    {
        if (GetActiveEmployee() != null)
        {
            GetActiveEmployee().SetState(Employee.EmployeeState.Available);
            EmployeeManager.Instance.MoveEmployeeToList(GetActiveEmployee());
            Debug.Log($"Предварительное назначение {GetActiveEmployee().Data.employeeName} в район {districtData.DistrictName} сброшено.");
            SetActiveEmployee(null);
            districtData.PreliminaryPopularity = 0; // Очистка популярности при снятии
            popularityGain = 0;
            UpdateEmployeeImage();
        }
    }

    public void ClearAssignedEmployee()
    {
        if (GetAssignedEmployee() != null)
        {
            GetAssignedEmployee().SetState(Employee.EmployeeState.Available);
            EmployeeManager.Instance.MoveEmployeeToList(GetAssignedEmployee());
            Debug.Log($"Назначение {GetAssignedEmployee().Data.employeeName} в район {districtData.DistrictName} сброшено.");
            SetAssignedEmployee(null);
            UpdateEmployeeImage();
            isConfirmed = false;
            UpdateEmployeeImageInteractable(true);
        }
    }

    public void ClearAssignment()
    {
        if (GetActiveEmployee() != null)
        {
            GetActiveEmployee().SetState(Employee.EmployeeState.Available);
            EmployeeManager.Instance.MoveEmployeeToList(GetActiveEmployee());
            Debug.Log($"Предварительное назначение {GetActiveEmployee().Data.employeeName} в район {districtData.DistrictName} сброшено.");
            SetActiveEmployee(null);
            districtData.PreliminaryPopularity = 0; // Очистка популярности
            popularityGain = 0;
            UpdateEmployeeImage();
        }
    }

    public void ConfirmAssignment()
    {
        if (GetActiveEmployee() != null)
        {
            foreach (var districtPanel in Object.FindObjectsByType<DistrictData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (districtPanel != this && districtPanel.GetAssignedEmployee() != null && districtPanel.GetAssignedEmployee() == GetActiveEmployee())
                {
                    districtPanel.SetAssignedEmployee(null);
                    Debug.Log($"Сотрудница {GetActiveEmployee().Data.employeeName} освобождена из района {districtPanel.GetDistrictData().DistrictName} для назначения в район {districtData.DistrictName}.");
                }
            }
            SetAssignedEmployee(GetActiveEmployee());
            SetActiveEmployee(null); // Очистка ActiveEmployee после подтверждения
            UpdateEmployeeImage();
            Debug.Log($"Назначение {GetAssignedEmployee().Data.employeeName} подготовлено для подтверждения в район {districtData.DistrictName}.");
        }
    }

    public void FinalizeAssignment()
    {
        if (GetAssignedEmployee() != null)
        {
            GetAssignedEmployee().SetState(Employee.EmployeeState.Marketing);
            EmployeeManager.Instance.MoveEmployeeToList(GetAssignedEmployee());
            DistrictManager.Instance.AssignEmployeeToDistrict(GetAssignedEmployee(), districtData);
            isConfirmed = true;
            UpdateEmployeeImageInteractable(false);
            Debug.Log($"Подтверждено назначение {GetAssignedEmployee().Data.employeeName} в район {districtData.DistrictName}.");
        }
    }

    public void ClearActiveEmployeeFromOtherDistrict(DistrictData otherDistrict)
    {
        if (otherDistrict != null && otherDistrict.GetActiveEmployee() != null && otherDistrict.GetActiveEmployee() == GetActiveEmployee())
        {
            SetActiveEmployee(null);
            districtData.PreliminaryPopularity = 0; // Очистка популярности
            popularityGain = 0;
            UpdateEmployeeImage();
            Debug.Log($"ActiveEmployee очищен для района {districtData.DistrictName} из-за назначения в другой район.");
        }
    }

    public void UpdateEmployeeImage() // Остаётся public
    {
        if (GetActiveEmployee() != null)
        {
            employeeImage.sprite = GetActiveEmployee().Data.portraitIcon;
        }
        else if (GetAssignedEmployee() != null)
        {
            employeeImage.sprite = GetAssignedEmployee().Data.portraitIcon;
        }
        else
        {
            employeeImage.sprite = null; // Установить дефолтное изображение
        }
        UpdateEmployeeImageInteractable(GetActiveEmployee() == null && !isConfirmed);
    }

    public void UpdateEmployeeImageInteractable(bool interactable) // Остаётся public
    {
        var button = employeeImage.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public DistrictDataSO GetDistrictData()
    {
        return districtData;
    }

    public Employee GetActiveEmployee()
    {
        return activeEmployee;
    }

    public Employee GetAssignedEmployee()
    {
        return assignedEmployee;
    }

    public void SetActiveEmployee(Employee employee)
    {
        activeEmployee = employee;
        CalculatePopularityGain(); // Пересчет при изменении
        districtData.PreliminaryPopularity = popularityGain; // Обновление preliminaryPopularity
    }

    public void SetAssignedEmployee(Employee employee)
    {
        assignedEmployee = employee;
    }

    public bool IsConfirmed()
    {
        return isConfirmed;
    }

    private void CalculatePopularityGain()
    {
        if (activeEmployee == null || districtData?.EmployeePreferences == null)
        {
            _popularityGainCache = 0f;
            return;
        }
        var prefs = districtData.EmployeePreferences;
        var employeeData = activeEmployee.Data;
        _popularityGainCache = prefs.ChestSize.FirstOrDefault(p => p.Size == employeeData.breastSize).Value +
                               prefs.BodyType.FirstOrDefault(p => p.Type == employeeData.bodyType).Value +
                               prefs.RacePreference.FirstOrDefault(p => p.Race == employeeData.race).Value;
        districtData.PreliminaryPopularity = _popularityGainCache;
    }    
}